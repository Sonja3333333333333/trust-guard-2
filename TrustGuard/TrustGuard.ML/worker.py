from celery import Celery
import re
import pickle
import time
from stop_words import get_stop_words
from ddgs import DDGS
from urllib.parse import urlparse

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


# ---------------- OSINT SEARCH ---------------- #

TRUSTED_DOMAINS = [
    "bbc.com", "bbc.co.uk", "nytimes.com", "nature.com",
    "scientificamerican.com", "smithsonianmag.com",
    "nationalgeographic.com", "upi.com", "afp.com",
    "reuters.com", "apnews.com",
    "cnn.com", "washingtonpost.com", "theguardian.com",
    "foxnews.com", "cbsnews.com", "nbcnews.com", "latimes.com"
]


def is_trusted(url):
    try:
        domain = urlparse(url).netloc.lower()
        return any(trusted in domain for trusted in TRUSTED_DOMAINS)
    except:
        return False


def search_trusted_sources(query: str):
    headline = query.split('\n')[0].strip()
    clean_query = re.sub(r'[^\w\s]', '', headline)
    words = clean_query.split()
    clean_query = " ".join(words[:15]) + " news"

    print(f"ВОРКЕР: Пошук: {clean_query}")

    raw_results = []

    # retry (антибот)
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
    fallback_links = []

    for item in raw_results:
        url = item.get("href", "")
        title = item.get("title", "Без назви")

        if not url:
            continue

        link_obj = {"name": title, "url": url}

        if len(fallback_links) < 3:
            fallback_links.append(link_obj)

        if is_trusted(url):
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
        "links": fallback_links,
        "message": "Авторитетні джерела не знайдені."
    }


# ---------------- MAIN TASK ---------------- #

@app.task
def predict_news(text: str):
    print(f"ВОРКЕР: Отримав новий запит ({len(text)} симв.)")

    if not tfidf_vectorizer or not classifier_model:
        return {
            "mlAnalysis": {
                "verdict": "Error",
                "confidenceScore": 0,
                "message": "Моделі не завантажені!"
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

    ml_verdict = verdict_map.get(prediction, str(prediction).capitalize())

    print(f"ВОРКЕР: ML Verdict -> {ml_verdict} ({round(confidence*100)}%)")

    osint_result = search_trusted_sources(text)
    print(f"ВОРКЕР: OSINT Status -> {osint_result['status']}")

    return {
        "mlAnalysis": {
            "verdict": ml_verdict,
            "confidenceScore": round(confidence * 100, 2),
            "message": "Аналіз проведено на основі ML моделі."
        },
        "osintAnalysis": osint_result
    }