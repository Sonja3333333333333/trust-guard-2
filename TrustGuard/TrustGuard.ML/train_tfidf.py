import pandas as pd
import pickle
import re
from sklearn.feature_extraction.text import TfidfVectorizer
from stop_words import get_stop_words
from sklearn.model_selection import train_test_split
from sklearn.metrics import accuracy_score

from sklearn.ensemble import RandomForestClassifier 

print("1. Завантажуємо датасет...")
df = pd.read_csv('WELFake_Dataset.csv')
df = df.dropna(subset=['text'])

UKRAINIAN_STOP_WORDS = set(get_stop_words('en'))

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

print("2. Cleaning dataset data...")
df['cleaned_text'] = df['text'].apply(clean_text)

print("3. TF-IDF...")
vectorizer = TfidfVectorizer()
X = vectorizer.fit_transform(df['cleaned_text'])

y = df['label'] 

print("4. Saving to... tfidf_vectorizer.pkl")
with open('tfidf_vectorizer.pkl', 'wb') as f:
    pickle.dump(vectorizer, f)

print("5. Spliting data: 80% train, 20% test...")
X_train, X_test, y_train, y_test = train_test_split(X, y, test_size=0.2, random_state=42)

print("6. AI learning (100 trees)..")
model = RandomForestClassifier(n_estimators=100, random_state=42, n_jobs=-1)
model.fit(X_train, y_train)

print("7. Testing on 20% of data")
predictions = model.predict(X_test)
accuracy = accuracy_score(y_test, predictions)
print(f"accuracy: {accuracy * 100:.2f}")

print("8. Saving to classifier_model.pkl...")
with open('classifier_model.pkl', 'wb') as f:
    pickle.dump(model, f)
