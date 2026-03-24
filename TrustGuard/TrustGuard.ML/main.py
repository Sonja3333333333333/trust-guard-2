from fastapi import FastAPI, File, UploadFile, Form
from pydantic import BaseModel
from pypdf import PdfReader
import docx  # Бібліотека для Word
import io
import asyncio

# Імпортуємо нашого воркера
from worker import predict_news

app = FastAPI()

class NewsRequest(BaseModel):
    content: str
    content_type: str 

# ==========================================
# ПАРСЕРИ ДОКУМЕНТІВ
# ==========================================

def extract_pdf(file_bytes: bytes) -> str:
    reader = PdfReader(io.BytesIO(file_bytes))
    text = ""
    for page in reader.pages:
        extracted = page.extract_text()
        if extracted:
            text += extracted + "\n"
    return text

def extract_docx(file_bytes: bytes) -> str:
    doc = docx.Document(io.BytesIO(file_bytes))
    text = "\n".join([para.text for para in doc.paragraphs])
    return text

def extract_txt(file_bytes: bytes) -> str:
    return file_bytes.decode('utf-8')

# ==========================================
# API ЕНДПОІНТИ
# ==========================================

@app.post("/api/analyze")
async def analyze_news(request: NewsRequest):
    print("[API] Отримав текст. Відправляю задачу в Redis чергу...")
    task = predict_news.delay(request.content)
    result = await asyncio.to_thread(task.get)
    return result

@app.post("/api/analyze/file")
async def analyze_file(file: UploadFile = File(...), content_type: str = Form(...)):
    print(f"[API] Отримано файл: {file.filename}")
    
    file_bytes = await file.read()
    text = ""
    
    filename_lower = file.filename.lower()
    
    # Визначаємо формат і дістаємо текст
    if filename_lower.endswith(".pdf"):
        text = await asyncio.to_thread(extract_pdf, file_bytes)
    elif filename_lower.endswith(".docx"):
        text = await asyncio.to_thread(extract_docx, file_bytes)
    elif filename_lower.endswith(".txt"):
        text = await asyncio.to_thread(extract_txt, file_bytes)
    else:
        return {"error": f"Формат файлу {file.filename} не підтримується."}
        
    print(f"[API] Текст витягнуто ({len(text)} символів). Відправляю у Воркер...")
    
    # Відправляємо чистий текст у Celery Воркер
    task = predict_news.delay(text)
    
    # Чекаємо результат
    result = await asyncio.to_thread(task.get)
    return result