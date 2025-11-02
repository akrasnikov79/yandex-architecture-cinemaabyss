# Быстрый старт - Events Microservice

## Что создано?

✅ **Основной вариант:** `src/microservices/events/CinemaAbyss.Events/` (MassTransit)
✅ **Пример для сравнения:** `src/microservices/events/events-confluent-example/` (Confluent.Kafka)

Оба варианта реализуют одинаковую функциональность согласно `api-specification.yaml`.

## Быстрый запуск

### Шаг 1: Запустите Docker Desktop

Убедитесь, что Docker Desktop запущен на вашей машине.

### Шаг 2: Запустите всю систему

Из корня проекта:

```bash
docker-compose up -d
```

Это запустит:
- ✅ PostgreSQL (порт 5432)
- ✅ Kafka + Zookeeper (порты 9092, 2181)
- ✅ Monolith (порт 8080)
- ✅ Movies Service (порт 8081)
- ✅ **Events Service** (порт 8082) ← **Новый!**
- ✅ Proxy Service (порт 8000)
- ✅ Kafka UI (порт 8090)

### Шаг 3: Проверьте работоспособность

```bash
# Health check
curl http://localhost:8082/api/events/health

# Ожидаемый ответ:
# {"status":true}
```

### Шаг 4: Запустите Postman тесты

```bash
cd tests/postman
npm install
node run-tests.js --environment=docker
```

Или запустите только тесты Events сервиса:

```bash
node run-tests.js --environment=docker --folder="Events Microservice"
```

## Ручное тестирование

### 1. Откройте Swagger UI

Перейдите в браузере: http://localhost:8082/swagger

### 2. Создайте события через API

#### Movie Event
```bash
curl -X POST http://localhost:8082/api/events/movie \
  -H "Content-Type: application/json" \
  -d '{
    "movie_id": 1,
    "title": "Inception",
    "action": "viewed",
    "user_id": 10,
    "rating": 8.8,
    "genres": ["Sci-Fi", "Action"],
    "description": "A mind-bending thriller"
  }'
```

#### User Event
```bash
curl -X POST http://localhost:8082/api/events/user \
  -H "Content-Type: application/json" \
  -d '{
    "user_id": 10,
    "username": "john_doe",
    "email": "john@example.com",
    "action": "logged_in",
    "timestamp": "2025-11-02T13:00:00Z"
  }'
```

#### Payment Event
```bash
curl -X POST http://localhost:8082/api/events/payment \
  -H "Content-Type: application/json" \
  -d '{
    "payment_id": 5,
    "user_id": 10,
    "amount": 9.99,
    "status": "completed",
    "timestamp": "2025-11-02T13:00:00Z",
    "method_type": "credit_card"
  }'
```

### 3. Проверьте логи Consumer'ов

```bash
docker logs cinemaabyss-events-service -f
```

Вы должны увидеть:
```
info: CinemaAbyss.Events.Consumers.MovieEventConsumer[0]
      Received Movie Event: MovieId=1, Title=Inception, Action=viewed, UserId=10, Partition=0, Offset=42
      
info: CinemaAbyss.Events.Consumers.UserEventConsumer[0]
      Received User Event: UserId=10, Username=john_doe, Email=john@example.com, Action=logged_in, Timestamp=2025-11-02T13:00:00Z, Partition=0, Offset=15
```

### 4. Мониторинг через Kafka UI

Откройте: http://localhost:8090

Здесь вы можете:
- Просмотреть топики: `movie-events`, `user-events`, `payment-events`
- Увидеть сообщения в топиках
- Проверить consumer groups и offsets

## Локальная разработка (без Docker)

### 1. Запустите только инфраструктуру

```bash
docker-compose up -d postgres kafka zookeeper kafka-ui
```

### 2. Запустите Events Service локально

```bash
cd src/microservices/events/CinemaAbyss.Events
dotnet run
```

Сервис будет доступен на http://localhost:8082

### 3. Тестируйте через Swagger

http://localhost:8082/swagger

## Тестирование Confluent.Kafka примера (опционально)

### Запуск

```bash
cd src/microservices/events/events-confluent-example/CinemaAbyss.Events.Confluent
dotnet run
```

Сервис будет доступен на http://localhost:8083

### API такой же

Все endpoints идентичны, только порт другой:
```bash
curl http://localhost:8083/api/events/health
curl -X POST http://localhost:8083/api/events/movie ...
```

## Проверка работы

### ✅ Health Check
```bash
curl http://localhost:8082/api/events/health
# {"status":true}
```

### ✅ Create Movie Event
```bash
curl -X POST http://localhost:8082/api/events/movie \
  -H "Content-Type: application/json" \
  -d '{"movie_id":1,"title":"Test","action":"viewed"}'

# {"status":"success","partition":0,"offset":0,"event":{...}}
```

### ✅ Check Logs
```bash
docker logs cinemaabyss-events-service | grep "Received Movie Event"
# Должны увидеть логи о полученном событии
```

### ✅ Check Kafka UI
1. Открыть http://localhost:8090
2. Перейти в Topics → movie-events
3. Увидеть сообщения

### ✅ Run Tests
```bash
cd tests/postman
node run-tests.js --environment=docker --folder="Events Microservice"
# Все тесты должны пройти ✓
```

## Структура проекта

```
src/microservices/events/
├── CinemaAbyss.Events/              # Основной вариант (MassTransit)
│   ├── Models/                      # Модели событий
│   │   ├── MovieEvent.cs
│   │   ├── UserEvent.cs
│   │   ├── PaymentEvent.cs
│   │   ├── EventData.cs
│   │   └── EventResponse.cs
│   ├── Consumers/                   # Обработчики событий
│   │   ├── MovieEventConsumer.cs
│   │   ├── UserEventConsumer.cs
│   │   └── PaymentEventConsumer.cs
│   ├── Controllers/                 # API контроллеры
│   │   └── EventsController.cs
│   ├── Program.cs                   # Конфигурация MassTransit
│   ├── appsettings.json
│   └── Dockerfile
│
├── events-confluent-example/        # Пример для сравнения
│   └── CinemaAbyss.Events.Confluent/
│       ├── Models/
│       ├── Services/                # Kafka Producer/Consumer
│       ├── Controllers/
│       ├── Program.cs
│       └── Dockerfile
│
├── Dockerfile                       # Docker для основного варианта
├── README.md                        # Основная документация
├── COMPARISON.md                    # Сравнение MassTransit vs Confluent
└── GETTING_STARTED.md               # Этот файл
```

## Troubleshooting

### Порт 8082 занят
```bash
# Проверьте, что запущено
netstat -ano | findstr :8082

# Остановите conflicting service или измените порт в docker-compose.yml
```

### Docker не запускается
```bash
# Проверьте, что Docker Desktop запущен
docker ps

# Если не работает, перезапустите Docker Desktop
```

### Kafka не подключается
```bash
# Проверьте, что Kafka запущен
docker ps | grep kafka

# Проверьте логи Kafka
docker logs cinemaabyss-kafka

# Перезапустите Kafka
docker-compose restart kafka
```

### Consumer не получает сообщения
```bash
# Проверьте логи events-service
docker logs cinemaabyss-events-service -f

# Проверьте Kafka UI
# http://localhost:8090 → Topics → Consumers
```

### Тесты падают
```bash
# Убедитесь, что все сервисы запущены
docker-compose ps

# Подождите 30 секунд после старта
# (Kafka и Postgres инициализируются)

# Запустите тесты снова
cd tests/postman
node run-tests.js --environment=docker
```

## Дальнейшие шаги

1. ✅ Изучите [README.md](./README.md) для деталей реализации
2. ✅ Изучите [COMPARISON.md](./COMPARISON.md) для сравнения подходов
3. ✅ Проверьте API в `api-specification.yaml`
4. ✅ Экспериментируйте с Kafka UI: http://localhost:8090

## Полезные команды

```bash
# Запустить все сервисы
docker-compose up -d

# Остановить все сервисы
docker-compose down

# Перезапустить events-service
docker-compose restart events-service

# Посмотреть логи events-service
docker logs cinemaabyss-events-service -f

# Пересобрать events-service
docker-compose up -d --build events-service

# Запустить только инфраструктуру
docker-compose up -d postgres kafka zookeeper kafka-ui

# Очистить все (включая volumes)
docker-compose down -v
```

## Поддержка

Если возникли проблемы:
1. Проверьте, что Docker Desktop запущен
2. Проверьте логи: `docker logs cinemaabyss-events-service`
3. Проверьте Kafka UI: http://localhost:8090
4. Убедитесь, что все порты свободны (8082, 9092, 5432)

---

**Готово!** 🎉 Микросервис Events создан и готов к работе!

