# 1. Беремо легку і швидку версію Python
FROM python:3.10-slim

# 2. Вказуємо, що всі наші файли будуть лежати в папці /app всередині контейнера
WORKDIR /app

# 3. Копіюємо requirements.txt та встановлюємо всі бібліотеки
COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

# 4. ВАЖЛИВО: Завантажуємо словники NLTK прямо під час збірки контейнера!
RUN python -m nltk.downloader punkt
RUN python -m nltk.downloader punkt_tab

# 5. Копіюємо всі твої скрипти (main.py, worker.py) та ML-моделі (.pkl)
COPY . .

# 6. Відкриваємо порт 8000 для доступу з C#
EXPOSE 8000