# CinemaAbyss Events Microservice - MassTransit (Основной)

Микросервис обработки событий на **C# ASP.NET Core** с использованием **MassTransit** и **Kafka**.

## Особенности

- **MassTransit** - высокоуровневая абстракция над Kafka
- **Автоматические Consumers** - фоновая обработка событий
- **Kafka Topics**: `movie-events`, `user-events`, `payment-events`
- **API** согласно спецификации `api-specification.yaml`

## Архитектура

### Модели (Models/)
- `MovieEvent` - события фильмов
- `UserEvent` - события пользователей
- `PaymentEvent` - события платежей
- `EventData` - базовая модель события
- `EventResponse` - ответ API с partition/offset

### Consumers (Consumers/)
Автоматически регистрируются как Hosted Services:
- `MovieEventConsumer` - обработка событий фильмов
- `UserEventConsumer` - обработка событий пользователей
- `PaymentEventConsumer` - обработка событий платежей

Каждый consumer логирует:
- Тип события
- ID и основные поля
- Kafka partition и offset
- Timestamp

### Controllers (Controllers/)
**EventsController** с endpoints:
- `GET /api/events/health` → `{"status": true}`
- `POST /api/events/movie` → создание события фильма
- `POST /api/events/user` → создание события пользователя
- `POST /api/events/payment` → создание события платежа

## Конфигурация

### Program.cs
```csharp
// Регистрация MassTransit с Kafka Rider
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<MovieEventConsumer>();
    x.AddConsumer<UserEventConsumer>();
    x.AddConsumer<PaymentEventConsumer>();
    
    x.UsingInMemory((context, cfg) => { ... });
    
    x.AddRider(rider =>
    {
        rider.AddProducer<MovieEvent>("movie-events");
        rider.AddProducer<UserEvent>("user-events");
        rider.AddProducer<PaymentEvent>("payment-events");
        
        rider.UsingKafka((context, k) =>
        {
            k.Host(kafkaBrokers);
            
            k.TopicEndpoint<MovieEvent>("movie-events", "events-service-group", e =>
            {
                e.ConfigureConsumer<MovieEventConsumer>(context);
            });
            // ... другие топики
        });
    });
});
```

### appsettings.json
```json
{
  "Port": "8082",
  "Kafka": {
    "Brokers": "localhost:9092"
  }
}
```

## Запуск

### Локально (без Docker)

1. Запустите Kafka:
```bash
docker-compose up -d kafka zookeeper
```

2. Запустите сервис:
```bash
cd src/microservices/events/CinemaAbyss.Events
dotnet run
```

Сервис будет доступен на `http://localhost:8082`

### С Docker Compose

```bash
# Из корня проекта
docker-compose up -d events-service
```

## Тестирование

### Health Check
```bash
curl http://localhost:8082/api/events/health
```

### Postman тесты
```bash
cd tests/postman
npm install
node run-tests.js --environment=local --folder="Events Microservice"
```

### Swagger UI
Откройте в браузере: `http://localhost:8082/swagger`

## Примеры запросов

### Создание события фильма
```bash
curl -X POST http://localhost:8082/api/events/movie \
  -H "Content-Type: application/json" \
  -d '{
    "movie_id": 1,
    "title": "Inception",
    "action": "viewed",
    "user_id": 10
  }'
```

Ответ:
```json
{
  "status": "success",
  "partition": 0,
  "offset": 0,
  "event": {
    "id": "movie-1-viewed",
    "type": "movie",
    "timestamp": "2025-11-02T13:00:00Z",
    "payload": { ... }
  }
}
```

### Создание события пользователя
```bash
curl -X POST http://localhost:8082/api/events/user \
  -H "Content-Type: application/json" \
  -d '{
    "user_id": 10,
    "username": "john_doe",
    "action": "logged_in",
    "timestamp": "2025-11-02T13:00:00Z"
  }'
```

### Создание события платежа
```bash
curl -X POST http://localhost:8082/api/events/payment \
  -H "Content-Type: application/json" \
  -d '{
    "payment_id": 5,
    "user_id": 10,
    "amount": 9.99,
    "status": "completed",
    "timestamp": "2025-11-02T13:00:00Z"
  }'
```

## Логирование

Consumers автоматически логируют все обработанные события:

```
info: CinemaAbyss.Events.Consumers.MovieEventConsumer[0]
      Received Movie Event: MovieId=1, Title=Inception, Action=viewed, UserId=10, Partition=0, Offset=42
      
info: CinemaAbyss.Events.Consumers.UserEventConsumer[0]
      Received User Event: UserId=10, Username=john_doe, Email=N/A, Action=logged_in, Timestamp=2025-11-02T13:00:00Z, Partition=0, Offset=15
      
info: CinemaAbyss.Events.Consumers.PaymentEventConsumer[0]
      Received Payment Event: PaymentId=5, UserId=10, Amount=9.99, Status=completed, Timestamp=2025-11-02T13:00:00Z, MethodType=credit_card, Partition=0, Offset=23
```

## Зависимости

```xml
<PackageReference Include="MassTransit" Version="8.5.5" />
<PackageReference Include="MassTransit.Kafka" Version="8.5.5" />
<PackageReference Include="MassTransit.AspNetCore" Version="8.5.5" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="9.0.6" />
```

## Сравнение с Confluent.Kafka

Альтернативная реализация с Confluent.Kafka находится в `../events-confluent-example/`. 

См. [../events-confluent-example/README.md](../events-confluent-example/README.md) для сравнения подходов.

**Основные отличия:**
- **MassTransit** (этот вариант): Высокоуровневая абстракция, меньше кода, автоматизация
- **Confluent.Kafka**: Низкоуровневый API, больше контроля, больше кода

## Docker

Dockerfile использует multi-stage build:
- **Build stage**: .NET SDK 8.0
- **Runtime stage**: ASP.NET Core Runtime 8.0
- **Port**: 8082
- **Entry point**: `dotnet CinemaAbyss.Events.dll`

## Мониторинг

Kafka UI доступен на `http://localhost:8090` (если запущен через docker-compose).

Здесь можно:
- Просматривать топики
- Просматривать сообщения
- Мониторить consumer groups
- Просматривать offsets

