from celery import Celery
import time
import random

# Підключаємося до нашого Redis у Докері
app = Celery(
    'ml_tasks', 
    broker='redis://localhost:6379/0', 
    backend='redis://localhost:6379/0'
)

# @app.task вказує, що ця функція може виконуватися в окремому процесі
@app.task
def predict_news(text: str):
    print(f"ВОРКЕР: Отримав текст довжиною {len(text)} символів. Починаю аналіз...")
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