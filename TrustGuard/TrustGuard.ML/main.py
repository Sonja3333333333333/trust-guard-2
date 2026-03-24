from fastapi import FastAPI, File, UploadFile, Form
from pydantic import BaseModel
import random

app = FastAPI()

# Модель для текстових запитів
class NewsRequest(BaseModel):
    content: str
    content_type: str 

# 1. Ендпоінт для голого тексту та посилань
@app.post("/api/analyze")
async def analyze_news(request: NewsRequest):
    print(f"[TEXT API] Отримано запит типу: {request.content_type}")
    print(f"[TEXT API] Зміст: {request.content[:50]}...")
    return generate_mock_result()

# 2. НОВИЙ Ендпоінт для файлів (PDF, Word, Зображення, TXT)
@app.post("/api/analyze/file")
async def analyze_file(file: UploadFile = File(...), content_type: str = Form(...)):
    print(f"[FILE API] Отримано файл: {file.filename}")
    print(f"[FILE API] Формат: {content_type}")
    
    # В майбутньому ти будеш читати файл отак:
    # file_bytes = await file.read()
    
    return generate_mock_result()

def generate_mock_result():
    verdicts = ["Real", "Fake", "Uncertain"]
    chosen_verdict = random.choices(verdicts, weights=[45, 45, 10], k=1)[0] 
    confidence = round(random.uniform(0.60, 0.99), 2)
    
    return {
        "verdict": chosen_verdict,
        "confidenceScore": confidence,
        "message": "Аналіз успішно завершено (Mock)"
    }