import pandas as pd
import re
import matplotlib.pyplot as plt
import seaborn as sns
from sklearn.feature_extraction.text import TfidfVectorizer
from stop_words import get_stop_words
from sklearn.model_selection import train_test_split
from sklearn.ensemble import RandomForestClassifier
from sklearn.metrics import confusion_matrix

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

df['cleaned_text'] = df['text'].apply(clean_text)

vectorizer = TfidfVectorizer()
X = vectorizer.fit_transform(df['cleaned_text'])
y = df['label'] 

X_train, X_test, y_train, y_test = train_test_split(X, y, test_size=0.2, random_state=42)

model = RandomForestClassifier(n_estimators=100, random_state=42, n_jobs=-1)
model.fit(X_train, y_train)

predictions = model.predict(X_test)
cm = confusion_matrix(y_test, predictions)

plt.figure(figsize=(8, 6))
sns.heatmap(cm, annot=True, fmt='d', cmap='Blues', 
            xticklabels=['Правда (0)', 'Фейк (1)'], 
            yticklabels=['Правда (0)', 'Фейк (1)'],
            annot_kws={"size": 16})

plt.xlabel('Передбачений клас', fontsize=12)
plt.ylabel('Фактичний клас', fontsize=12)

plt.savefig('confusion_matrix.png', dpi=300, bbox_inches='tight')