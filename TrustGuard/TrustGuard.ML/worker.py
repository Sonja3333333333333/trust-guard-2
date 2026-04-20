from celery import Celery
import time
import re
import pickle
from stop_words import get_stop_words

app = Celery(
    'ml_tasks', 
    broker='redis://localhost:6379/0', 
    backend='redis://localhost:6379/0'
)

# Завантажуємо стоп-слова один раз при старті
UKRAINIAN_STOP_WORDS = set(get_stop_words('uk'))

print("ВОРКЕР: Завантажую математичні моделі...")
try:
    # 1. Завантажуємо "перекладач" (векторизатор)
    with open('tfidf_vectorizer.pkl', 'rb') as f:
        tfidf_vectorizer = pickle.load(f)
        
    # 2. Завантажуємо "мозок" (навчену модель)
    with open('classifier_model.pkl', 'rb') as f:
        classifier_model = pickle.load(f)
        
    print("ВОРКЕР: Обидві моделі успішно завантажені! Готовий до роботи.")
except FileNotFoundError as e:
    print(f"ВОРКЕР: ПОМИЛКА! Не знайдено файл моделі: {e}")
    tfidf_vectorizer = None
    classifier_model = None

def clean_text(raw_text: str) -> str:
    """Очищення тексту (має бути ідентичним тому, що було при навчанні)."""
    text = str(raw_text).lower()
    text = re.sub(r'http[s]?://\S+', '', text)
    text = re.sub(r'[^\w\s]', ' ', text)
    text = re.sub(r'\d+', '', text)
    
    words = text.split()
    cleaned_words = [word for word in words if word not in UKRAINIAN_STOP_WORDS]
    
    return " ".join(cleaned_words)

@app.task
def predict_news(text: str):
    print(f"ВОРКЕР: Отримав новий запит ({len(text)} симв.)")

    if not tfidf_vectorizer or not classifier_model:
        return {"verdict": "Error", "confidenceScore": 0, "message": "Моделі не завантажені!"}

    processed_text = clean_text(text)
    
    text_vector = tfidf_vectorizer.transform([processed_text])
    
    prediction = classifier_model.predict(text_vector)[0]
    
    probabilities = classifier_model.predict_proba(text_vector)[0]
    confidence = float(max(probabilities))

    verdict_map = {
        "1": "Fake", "0": "Real",
        1: "Fake", 0: "Real",
        "FAKE": "Fake", "REAL": "Real"
    }
    
    final_verdict = verdict_map.get(prediction, str(prediction).capitalize())

    print(f"ВОРКЕР: Результат аналізу -> {final_verdict} ({round(confidence*100)}%)")

    return {
        "verdict": final_verdict,
        "confidenceScore": round(confidence, 2),
        "message": "Аналіз проведено на основі навченої моделі ML."
    }