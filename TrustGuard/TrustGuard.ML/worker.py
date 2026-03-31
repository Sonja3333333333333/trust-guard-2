from celery import Celery
import time
import random
import re  # Додали для роботи з текстом (пошук і заміна)

# Підключаємося до нашого Redis у Докері
app = Celery(
    'ml_tasks', 
    broker='redis://localhost:6379/0', 
    backend='redis://localhost:6379/0'
)

# Список найпопулярніших українських слів-паразитів
UKRAINIAN_STOP_WORDS = {
    "і", "та", "або", "чи", "а", "але", "в", "у", "на", "з", "із", "зі", 
    "до", "від", "під", "над", "за", "про", "що", "як", "це", "то", "так", 
    "ні", "ми", "ви", "вони", "він", "вона", "воно", "я", "ти", "мене", 
    "тобі", "нам", "вам", "їх", "його", "її", "для", "щоб", "бо", "якщо"
}

def clean_text(raw_text: str) -> str:
    """Очищення тексту перед відправкою в ML-модель."""
    # 1. Переводимо все в нижній регістр
    text = raw_text.lower()
    
    # 2. Видаляємо всі URL-посилання
    text = re.sub(r'http[s]?://(?:[a-zA-Z]|[0-9]|[$-_@.&+]|[!*\\(\\),]|(?:%[0-9a-fA-F][0-9a-fA-F]))+', '', text)
    
    # 3. Видаляємо всі цифри та знаки пунктуації
    text = re.sub(r'[^\w\s]', ' ', text)
    text = re.sub(r'\d+', '', text)
    
    # 4. Фільтруємо стоп-слова
    words = text.split()
    cleaned_words = [word for word in words if word not in UKRAINIAN_STOP_WORDS]
    
    # 5. Зліплюємо слова назад
    return " ".join(cleaned_words)

# @app.task вказує, що ця функція може виконуватися в окремому процесі
@app.task
def predict_news(text: str):
    print(f"ВОРКЕР: Отримав текст довжиною {len(text)} символів. Починаю аналіз...")
    
    # Пропускаємо текст через наш очищувач
    processed_text = clean_text(text)
    print(f"ВОРКЕР: Текст очищено! Нова довжина: {len(processed_text)} символів.")
    # Виводимо перші 100 символів очищеного тексту, щоб ти сам побачив результат
    print(f"ВОРКЕР: Перевірка: {processed_text[:100]}...")
    
    time.sleep(2) # Імітація важкої роботи нейромережі
    
    verdicts = ["Real", "Fake", "Uncertain"]
    chosen_verdict = random.choices(verdicts, weights=[45, 45, 10], k=1)[0]
    confidence = round(random.uniform(0.60, 0.99), 2)
    
    print("ВОРКЕР: Аналіз завершено! Віддаю результат.")
    return {
        "verdict": chosen_verdict,
        "confidenceScore": confidence,
        "message": "Аналіз виконано на ізольованому Celery Worker!"
    }