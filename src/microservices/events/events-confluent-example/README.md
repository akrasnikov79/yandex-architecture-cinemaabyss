# CinemaAbyss Events Service - Confluent.Kafka Example

Это пример реализации микросервиса Events с использованием **Confluent.Kafka** для демонстрации низкоуровневого подхода к работе с Kafka.

## Отличия от MassTransit варианта

### Confluent.Kafka (этот пример)
- **Прямая работа с Kafka**: Полный контроль над producer и consumer
- **Ручное управление**: Явное создание producers и consumers
- **Больше кода**: Больше boilerplate кода для настройки
- **Гибкость**: Доступ ко всем возможностям Kafka
- **Partition и Offset**: Реальные значения из Kafka

### MassTransit (основной вариант)
- **Абстракция**: Высокоуровневый API поверх Kafka
- **Автоматизация**: Автоматическая регистрация consumers
- **Меньше кода**: Меньше настроек, больше conventions
- **DI интеграция**: Естественная работа с ASP.NET Core DI
- **Retry policies**: Встроенная обработка ошибок

## Компоненты

### Models
- `MovieEvent`, `UserEvent`, `PaymentEvent` - модели событий
- `Event`, `EventResponse` - модели ответов

### Services
- **KafkaProducerService** - отправка событий в Kafka с использованием `IProducer<TKey, TValue>`
  - `ProduceMovieEventAsync()` - отправка событий фильмов
  - `ProduceUserEventAsync()` - отправка событий пользователей
  - `ProducePaymentEventAsync()` - отправка событий платежей
  
- **KafkaConsumerService** - фоновая служба для чтения событий
  - Реализует `BackgroundService`
  - Подписывается на все три топика
  - Логирует обработанные события

### Controllers
- **EventsController** - API endpoints
  - `GET /api/events/health` - health check
  - `POST /api/events/movie` - создание события фильма
  - `POST /api/events/user` - создание события пользователя
  - `POST /api/events/payment` - создание события платежа

## Запуск

```bash
cd src/microservices/events/events-confluent-example/CinemaAbyss.Events.Confluent
dotnet run
```

Сервис будет доступен на `http://localhost:8083`

## Примечание

Этот пример НЕ добавлен в `docker-compose.yml` - он предназначен только для демонстрации и сравнения подходов.
Для production использования рекомендуется MassTransit вариант в папке `src/microservices/events/`.


