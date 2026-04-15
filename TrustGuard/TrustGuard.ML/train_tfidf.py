import pandas as pd
import pickle
import re
from sklearn.feature_extraction.text import TfidfVectorizer
from stop_words import get_stop_words

print("1. Завантажуємо датасет...")
# ВАЖЛИВО: Заміни 'news_data.csv' на реальну назву твого завантаженого файлу!
df = pd.read_csv('data_set_4.csv')

# Виводимо назви колонок, щоб переконатися, що ми беремо правильну
print("Колонки в таблиці:", df.columns.tolist())

# Наша функція очищення з worker.py
UKRAINIAN_STOP_WORDS = set(get_stop_words('uk'))

def clean_text(raw_text):
    # Захист від порожніх рядків
    if not isinstance(raw_text, str):
        return ""
        
    text = raw_text.lower()
    text = re.sub(r'http[s]?://\S+', '', text)
    text = re.sub(r'[^\w\s]', ' ', text)
    text = re.sub(r'\d+', '', text)
    
    words = text.split()
    cleaned_words = [word for word in words if word not in UKRAINIAN_STOP_WORDS]
    return " ".join(cleaned_words)

print("2. Очищаємо всі тексти в датасеті (це може зайняти хвилину-дві)...")
# ВАЖЛИВО: Заміни 'text' на ту назву колонки, де в таблиці лежать самі новини!
df['cleaned_text'] = df['Text'].apply(clean_text)

print("3. Навчаємо TF-IDF (перетворюємо слова на математику)...")
# max_features=5000 означає, що ми беремо топ-5000 найважливіших слів, щоб словник не важив гігбайти
vectorizer = TfidfVectorizer(max_features=5000)
vectorizer.fit(df['cleaned_text'])

print("4. Зберігаємо 'мозок' у файл...")
with open('tfidf_vectorizer.pkl', 'wb') as f:
    pickle.dump(vectorizer, f)

print("✅ ГОТОВО! Файл tfidf_vectorizer.pkl успішно створено.")