Currency Parser
Приложение для получения и сохранения курсов валют из API ЦБ РФ в PostgreSQL.
Реализовано на .NET 8 с использованием EF Core и Docker для развёртывания вместе с базой данных.
Локально использовалась PostgreSQL 17.5.
Docker-окружение использует образ PostgreSQL 15 для совместимости.

Структура проекта


CbrApp/ — исходный код консольного приложения

CbrApp.Tests/ - xUnit тесты

db-init/ — скрипты инициализации базы данных

docs/ — схема бд 

docker-compose.yml — конфигурация для запуска приложения и базы в контейнерах

Dockerfile — билд консольного приложения

appsettings.json — настройки подключения к БД и API


Запуск проекта с нуля
## Запуск проекта с нуля

1. Клонировать репозиторий:
```bash
git clone https://github.com/HighShaggy/currency_parser.git

```
2. Запустить в Docker
```bash
docker compose up --build
