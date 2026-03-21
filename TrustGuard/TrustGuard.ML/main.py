from fastapi import FastAPI
from pydantic import BaseModel

app = FastAPI()

class NewsRequest(BaseModel):
    text: str

@app.post("/analyze")
def analyze_news(request: NewsRequest):
    text_lower = request.text.lower()
    is_fake = "fraud" in text_lower or "fake" in text_lower
    
    return {
        "status": "fake" if is_fake else "real",
        "confidence": 92.5 if is_fake else 88.0
    }