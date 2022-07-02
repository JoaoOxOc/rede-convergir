# syntax=docker/dockerfile:1
FROM python:3.9-slim
ENV PYTHONDONTWRITEBYTECODE=1
ENV PYTHONUNBUFFERED=1

# Section 3- Compiler and OS libraries
RUN apt-get update \  
  && apt-get install -y --no-install-recommends build-essential libpq-dev \  
  && rm -rf /var/lib/apt/lists/*

COPY ./APIs/NewsService/requirements.txt /
RUN pip install --no-cache-dir -r /requirements.txt \  
    && rm -rf /requirements.txt \  
    && useradd -U app_user \  
    && install -d -m 0755 -o app_user -g app_user /app/static

WORKDIR /app

USER app_user:app_user

COPY --chown=app_user:app_user ./APIs/NewsService/ .
