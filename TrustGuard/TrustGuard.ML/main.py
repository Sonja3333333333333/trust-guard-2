from fastapi import FastAPI
from pydantic import BaseModel
import asyncio

# Імпортуємо нашого воркера
from worker import predict_news

app = FastAPI()

class NewsRequest(BaseModel):
    content: str
    content_type: str 



@app.post("/api/analyze")
async def analyze_news(request: NewsRequest):
    print(f"[API] Отримав текст ({len(request.content)} символів). Відправляю задачу в Redis чергу...")
    
    task = predict_news.delay(request.content)

    result = await asyncio.to_thread(task.get)
    return result