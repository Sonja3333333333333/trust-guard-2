from fastapi import FastAPI
from pydantic import BaseModel
from typing import List
import asyncio

from worker import predict_news

app = FastAPI()

class NewsRequest(BaseModel):
    content: str
    content_type: str = "Text"
    trustedDomains: List[str] = [] 

@app.post("/api/analyze")
async def analyze_news(request: NewsRequest):
    print(f"[API] Отримав текст ({len(request.content)} символів). Надійних доменів у списку: {len(request.trustedDomains)}")
    
    task = predict_news.delay(request.content, request.trustedDomains)

    result = await asyncio.to_thread(task.get)
    return result