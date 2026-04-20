import pandas as pd
import pickle
import re
from sklearn.feature_extraction.text import TfidfVectorizer
from stop_words import get_stop_words
from sklearn.linear_model import LogisticRegression

print("1. Завантажуємо датасет...")
df = pd.read_csv('data_set_4.csv')

print("Колонки в таблиці:", df.columns.tolist())

UKRAINIAN_STOP_WORDS = set(get_stop_words('uk'))

def clean_text(raw_text):
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
df['cleaned_text'] = df['Text'].apply(clean_text)

print("3. Навчаємо TF-IDF...")
vectorizer = TfidfVectorizer(max_features=5000)
print("5. Перетворюємо весь текст на матрицю для навчання...")

X = vectorizer.fit_transform(df['cleaned_text']) 

y = df['Label'] 

print("6. Навчаємо справжній ШІ (Логістичну регресію)...")
model = LogisticRegression(max_iter=1000)
model.fit(X, y)

print("7. Зберігаємо мозок ШІ у файл...")
with open('classifier_model.pkl', 'wb') as f:
    pickle.dump(model, f)