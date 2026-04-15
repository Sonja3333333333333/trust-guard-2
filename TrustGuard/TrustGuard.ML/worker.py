from celery import Celery
import time
import random
import re
import pickle # Бібліотека для розпакування нашого .pkl файлу
from stop_words import get_stop_words

# Підключаємося до Redis
app = Celery(
    'ml_tasks', 
    broker='redis://localhost:6379/0', 
    backend='redis://localhost:6379/0'
)

UKRAINIAN_STOP_WORDS = set(get_stop_words('uk'))

# --- НОВА МАГІЯ: ЗАВАНТАЖУЄМО TF-IDF ---
print("ВОРКЕР: Завантажую математичну модель (TF-IDF)...")
try:
    with open('tfidf_vectorizer.pkl', 'rb') as f:
        tfidf_vectorizer = pickle.load(f)
    print("ВОРКЕР: Модель успішно завантажена!")
except FileNotFoundError:
    print("ВОРКЕР: ПОМИЛКА! Файл tfidf_vectorizer.pkl не знайдено.")
    tfidf_vectorizer = None
# ---------------------------------------

def clean_text(raw_text: str) -> str:
    """Очищення тексту перед відправкою в ML-модель."""
    text = raw_text.lower()
    text = re.sub(r'http[s]?://\S+', '', text)
    text = re.sub(r'[^\w\s]', ' ', text)
    text = re.sub(r'\d+', '', text)
    
    words = text.split()
    cleaned_words = [word for word in words if word not in UKRAINIAN_STOP_WORDS]
    
    return " ".join(cleaned_words)

@app.task
def predict_news(text: str):
    print(f"ВОРКЕР: Отримав текст довжиною {len(text)} символів.")
    
    # 1. Пропускаємо текст через очищувач
    processed_text = clean_text(text)
    print(f"ВОРКЕР: Текст очищено: {processed_text[:50]}...")
    
    # 2. ПЕРЕТВОРЕННЯ В ЦИФРИ
    if tfidf_vectorizer:
        # transform бере наш текст і робить з нього матрицю
        text_matrix = tfidf_vectorizer.transform([processed_text])
        print(f"ВОРКЕР: Магія відбулася! Текст перетворено на матрицю розміром {text_matrix.shape}")
        # nnz показує, скільки унікальних слів із нашого словника знайшлося в цій новині
        print(f"ВОРКЕР: Знайдено {text_matrix.nnz} важливих слів.")
    
    time.sleep(2) # Імітація роботи нейромережі
    
    verdicts = ["Real", "Fake", "Uncertain"]
    chosen_verdict = random.choices(verdicts, weights=[45, 45, 10], k=1)[0]
    confidence = round(random.uniform(0.60, 0.99), 2)
    
    print("ВОРКЕР: Аналіз завершено! Віддаю результат.")
    return {
        "verdict": chosen_verdict,
        "confidenceScore": confidence,
        "message": "Текст очищено і перетворено на математичну матрицю!"
    }