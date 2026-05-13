from celery import Celery
import re
import pickle
import time
from stop_words import get_stop_words
from ddgs import DDGS
from urllib.parse import urlparse
import nltk
from nltk.tokenize import sent_tokenize, word_tokenize
import heapq

app = Celery(
    'ml_tasks',
    broker='redis://localhost:6379/0',
    backend='redis://localhost:6379/0'
)

ENGLISH_STOP_WORDS = set(get_stop_words('en'))

print("ВОРКЕР: Завантажую математичні моделі...")

try:
    with open('tfidf_vectorizer.pkl', 'rb') as f:
        tfidf_vectorizer = pickle.load(f)

    with open('classifier_model.pkl', 'rb') as f:
        classifier_model = pickle.load(f)

    print("ВОРКЕР: Обидві моделі успішно завантажені! Готовий до роботи.")

except FileNotFoundError as e:
    print(f"ВОРКЕР: ПОМИЛКА! Не знайдено файл моделі: {e}")
    tfidf_vectorizer = None
    classifier_model = None


def clean_text(raw_text: str) -> str:
    text = str(raw_text).lower()
    text = re.sub(r'http[s]?://\S+', '', text)
    text = re.sub(r'[^\w\s]', ' ', text)
    text = re.sub(r'\d+', '', text)

    words = text.split()
    cleaned_words = [word for word in words if word not in ENGLISH_STOP_WORDS]

    return " ".join(cleaned_words)


def is_trusted(url: str, trusted_domains: list) -> bool:
    try:
        domain = urlparse(url).netloc.lower()
        return any(trusted in domain for trusted in trusted_domains)
    except:
        return False

def search_trusted_sources(query: str, trusted_domains: list):
    headline = query.split('\n')[0].strip()
    clean_query = re.sub(r'[^\w\s]', '', headline)
    words = clean_query.split()
    clean_query = " ".join(words[:15]) + " news"

    print(f"ВОРКЕР: Пошук: {clean_query}")

    raw_results = []

    for attempt in range(3):
        try:
            with DDGS() as ddgs:
                raw_results = list(ddgs.text(clean_query, max_results=30))
            if raw_results:
                break
        except Exception as e:
            print(f"Retry {attempt+1} error: {e}")
            time.sleep(1)

    if not raw_results:
        return {
            "status": "Error",
            "trustedSourcesFound": 0,
            "links": [],
            "message": "Пошук не дав результатів."
        }

    trusted_links = []

    for item in raw_results:
        url = item.get("href", "")
        title = item.get("title", "Без назви")

        if not url:
            continue

        link_obj = {"name": title, "url": url}

        # Додаємо лише якщо джерело є в списку довірених
        if is_trusted(url, trusted_domains):
            trusted_links.append(link_obj)

        if len(trusted_links) >= 10:
            break

    if trusted_links:
        return {
            "status": "Confirmed",
            "trustedSourcesFound": len(trusted_links),
            "links": trusted_links,
            "message": "Новина підтверджена авторитетними джерелами."
        }

    return {
        "status": "Unverified",
        "trustedSourcesFound": 0,
        "links": [], 
        "message": "Авторитетні джерела не знайдені."
    }
    
    
def clean_extracted_sentence(sentence: str) -> str:
    cleaned = re.sub(r'^(Earlier|Also|However|Furthermore|In addition|Meanwhile|Separately|Additionally),\s*', '', sentence, flags=re.IGNORECASE)
 
    cleaned = re.sub(r'\s+also\s+', ' ', cleaned, flags=re.IGNORECASE)
    
    cleaned = re.sub(r',\s*(the statement added|he added|she added|according to[^\.]+)(?=\.)', '', cleaned, flags=re.IGNORECASE)

    if cleaned:
        cleaned = cleaned[0].upper() + cleaned[1:]
        
    return cleaned

def generate_summary(text: str, num_sentences: int = 3) -> str:
    try:
        fixed_text = re.sub(r'\.(?=[A-ZА-ЯІЇЄҐ])', '. ', text)
        sentences = sent_tokenize(fixed_text)
        
        if len(sentences) <= num_sentences:
            return "\n".join([f"• {sent.strip()}" for sent in sentences])

        word_frequencies = {}
        for word in word_tokenize(fixed_text.lower()):
            if word.isalnum() and word not in ENGLISH_STOP_WORDS:
                if word not in word_frequencies:
                    word_frequencies[word] = 1
                else:
                    word_frequencies[word] += 1

        if not word_frequencies:
            return f"• {fixed_text[:200]}..."

        max_frequency = max(word_frequencies.values())
        for word in word_frequencies.keys():
            word_frequencies[word] = (word_frequencies[word] / max_frequency)

        sentence_scores = {}
        for sent in sentences:
            for word in word_tokenize(sent.lower()):
                if word in word_frequencies:
                    if len(sent.split(' ')) < 30: 
                        if sent not in sentence_scores:
                            sentence_scores[sent] = word_frequencies[word]
                        else:
                            sentence_scores[sent] += word_frequencies[word]

        summary_sentences = heapq.nlargest(num_sentences, sentence_scores, key=sentence_scores.get)
        summary = [sent for sent in sentences if sent in summary_sentences]
        
        bullet_points = [f"• {clean_extracted_sentence(sent.strip())}" for sent in summary]
        
        return "\n".join(bullet_points)

    except Exception as e:
        print(f"ВОРКЕР: Помилка генерації summary: {e}")
        fallback_sentences = sent_tokenize(text)[:num_sentences]
        return "\n".join([f"• {sent.strip()}" for sent in fallback_sentences])

@app.task
def predict_news(text: str, trusted_domains: list):
    print(f"ВОРКЕР: Отримав новий запит ({len(text)} симв.)")

    if not tfidf_vectorizer or not classifier_model:
        return {
            "mlAnalysis": {
                "verdict": "Error",
                "confidenceScore": 0,
                "message": "Моделі не завантажені!",
                "summary": "" 
            },
            "osintAnalysis": {}
        }

    processed_text = clean_text(text)
    text_vector = tfidf_vectorizer.transform([processed_text])

    prediction = classifier_model.predict(text_vector)[0]
    probabilities = classifier_model.predict_proba(text_vector)[0]
    confidence = float(max(probabilities))

    verdict_map = {
        "1": "Fake", "0": "Real",
        1: "Fake", 0: "Real",
        "FAKE": "Fake", "REAL": "Real"
    }
    
    text_summary = generate_summary(text)
    print("ВОРКЕР: Summary успішно згенеровано!") 
    
    top_keywords = []
    try:
        feature_names = tfidf_vectorizer.get_feature_names_out()
        nonzero_indices = text_vector.nonzero()[1]
        
        word_scores = [(feature_names[idx], text_vector[0, idx]) for idx in nonzero_indices]
        word_scores.sort(key=lambda x: x[1], reverse=True)
        
        top_keywords = [word for word, score in word_scores[:5]]
    except Exception as e:
        print(f"ВОРКЕР: Помилка витягування ключових слів: {e}")

    ml_verdict = verdict_map.get(prediction, str(prediction).capitalize())

    print(f"ВОРКЕР: ML Verdict -> {ml_verdict} ({round(confidence*100)}%)")

    osint_result = search_trusted_sources(text, trusted_domains)
    print(f"ВОРКЕР: OSINT Status -> {osint_result['status']}")

    return {
        "mlAnalysis": {
            "verdict": ml_verdict,
            "confidenceScore": round(confidence * 100, 2),
            "message": "Аналіз проведено на основі ML моделі.",
            "summary": text_summary,
            "keyTriggers": top_keywords
        },
        "osintAnalysis": osint_result
    }