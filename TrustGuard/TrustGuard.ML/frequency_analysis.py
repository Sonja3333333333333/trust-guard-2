import re
import matplotlib.pyplot as plt
from sklearn.feature_extraction.text import TfidfVectorizer
from stop_words import get_stop_words
import pandas as pd


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

def get_top_n_words(corpus, n=15):
    vec = TfidfVectorizer().fit(corpus)
    bag_of_words = vec.transform(corpus)
    sum_words = bag_of_words.sum(axis=0) 
    words_freq = [(word, sum_words[0, idx]) for word, idx in vec.vocabulary_.items()]
    words_freq = sorted(words_freq, key=lambda x: x[1], reverse=True)
    return words_freq[:n]

print("Аналіз найбільш частотних слів...")

fake_news = df[df['label'] == 1]['cleaned_text']
real_news = df[df['label'] == 0]['cleaned_text']

top_fake = get_top_n_words(fake_news, 15)
top_real = get_top_n_words(real_news, 15)

print("\nТоп слів у ФЕЙКОВИХ новинах:")
for word, score in top_fake:
    print(f"{word}: {score:.2f}")

print("\nТоп слів у ПРАВДИВИХ новинах:")
for word, score in top_real:
    print(f"{word}: {score:.2f}")

# Бонус: Генерація простого графіка
def plot_words(data, title, color):
    words, scores = zip(*data)
    plt.figure(figsize=(10, 5))
    plt.barh(words, scores, color=color)
    plt.title(title)
    plt.gca().invert_yaxis()
    plt.show()

plot_words(top_fake, "Top 15 Words in Fake News", "red")
plot_words(top_real, "Top 15 Words in Real News", "green")