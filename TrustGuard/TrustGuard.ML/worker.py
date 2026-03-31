from celery import Celery
import time
import random
import re
from stop_words import get_stop_words # Підключаємо професійний датасет

# Підключаємося до Redis
app = Celery(
    'ml_tasks', 
    broker='redis://localhost:6379/0', 
    backend='redis://localhost:6379/0'
)

# Завантажуємо офіційний датасет українських стоп-слів
# Перетворюємо його у set (множину) для блискавичного пошуку
UKRAINIAN_STOP_WORDS = set(get_stop_words('uk'))

def clean_text(raw_text: str) -> str:
    """Очищення тексту перед відправкою в ML-модель."""
    # 1. Переводимо все в нижній регістр
    text = raw_text.lower()
    
    # 2. Видаляємо всі URL-посилання
    text = re.sub(r'http[s]?://\S+', '', text)
    
    # 3. Видаляємо всі цифри та знаки пунктуації
    text = re.sub(r'[^\w\s]', ' ', text)
    text = re.sub(r'\d+', '', text)
    
    # 4. Фільтруємо текст через датасет стоп-слів
    words = text.split()
    cleaned_words = [word for word in words if word not in UKRAINIAN_STOP_WORDS]
    
    # 5. Зліплюємо слова назад
    return " ".join(cleaned_words)

@app.task
def predict_news(text: str):
    print(f"ВОРКЕР: Отримав текст довжиною {len(text)} символів. Починаю аналіз...")
    
    # Пропускаємо текст через наш очищувач
    processed_text = clean_text(text)
    
    print(f"ВОРКЕР: В датасеті зараз {len(UKRAINIAN_STOP_WORDS)} стоп-слів.")
    print(f"ВОРКЕР: Текст очищено! Нова довжина: {len(processed_text)} символів.")
    print(f"ВОРКЕР: Перевірка: {processed_text[:100]}...")
    
    time.sleep(2) # Імітація важкої роботи нейромережі
    
    verdicts = ["Real", "Fake", "Uncertain"]
    chosen_verdict = random.choices(verdicts, weights=[45, 45, 10], k=1)[0]
    confidence = round(random.uniform(0.60, 0.99), 2)
    
    print("ВОРКЕР: Аналіз завершено! Віддаю результат.")
    return {
        "verdict": chosen_verdict,
        "confidenceScore": confidence,
        "message": "Аналіз виконано з використанням професійного датасету!"
    }