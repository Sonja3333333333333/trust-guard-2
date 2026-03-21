from fastapi import FastAPI
from pydantic import BaseModel
import random

app = FastAPI()

class NewsRequest(BaseModel):
    content: str
    content_type: str 

@app.post("/api/analyze")
def analyze_news(request: NewsRequest):
    print(f"Отримано запит типу: {request.content_type}")
    print(f"Текст для перевірки: {request.content[:50]}...")

    verdicts = ["Real", "Fake", "Uncertain"]

    chosen_verdict = random.choices(verdicts, weights=[45, 45, 10], k=1)[0] 
    
    confidence = round(random.uniform(0.60, 0.99), 2)
    
    return {
        "verdict": chosen_verdict,
        "confidenceScore": confidence,
        "message": "Аналіз успішно завершено (Mock)"
    }