# MFB Proxy - YARP Reverse Proxy

Проект обратного прокси на базе YARP (Yet Another Reverse Proxy) для маршрутизации и балансировки нагрузки микросервисов.

## 📖 Документация

- [Quick Start Guide](QUICKSTART.md) - Быстрый старт за 5 минут
- [Configuration Examples](docs/configuration-examples.md) - Примеры всех типов конфигураций
- [Custom Load Balancing](docs/custom-load-balancing.md) - Кастомные политики балансировки
- [YARP Official Docs](https://microsoft.github.io/reverse-proxy/) - Официальная документация

## 🏛️ Архитектура

```
┌─────────────┐
│   Client    │
│  (Browser,  │
│   Mobile)   │
└──────┬──────┘
       │ HTTPS
       ▼
┌──────────────────────────────────┐
│   MFB Proxy Gateway (YARP)       │
│   https://localhost:5050         │
│                                  │
│  ┌────────────────────────────┐ │
│  │   Routes & Load Balancing  │ │
│  │   - Weighted Round Robin   │ │
│  │   - Health Checks          │ │
│  │   - Session Affinity       │ │
│  └────────────────────────────┘ │
└─────┬─────────┬──────────┬──────┘
      │         │          │
      ▼         ▼          ▼
┌─────────┐ ┌─────────┐ ┌─────────┐
│Service1 │ │Service2 │ │Service3 │
│:5001    │ │:5002    │ │:7001    │
│Weight:- │ │Weight:- │ │Weight:70│
└─────────┘ └─────────┘ └─────────┘
                         ┌─────────┐
                         │Service4 │
                         │:7002    │
                         │Weight:30│
                         └─────────┘
```

## 🚀 Технологии

- .NET 8.0
- YARP.ReverseProxy 2.3.0
- ASP.NET Core 8.0

## 📋 Возможности

- ✅ Обратное проксирование запросов к backend-сервисам
- ✅ Балансировка нагрузки (Round Robin, Random, etc.)
- ✅ Поддержка множественных destination для одного кластера
- ✅ Конфигурация через appsettings.json
- ✅ CORS support
- ✅ HTTPS/HTTP поддержка

## 🏗️ Структура проекта

```
mfb-proxy/
├── src/
│   └── MfbProxy.Gateway/       # YARP Reverse Proxy Gateway
│       ├── Program.cs           # Конфигурация YARP
│       ├── appsettings.json     # Маршруты и кластеры
│       └── Properties/
│           └── launchSettings.json
├── MfbProxy.sln
└── README.md
```

## ⚙️ Конфигурация

### Порты по умолчанию

- **HTTP**: `http://localhost:5000`
- **HTTPS**: `https://localhost:5050`

### Настройка маршрутов и кластеров

Откройте `src/MfbProxy.Gateway/appsettings.json` для настройки:

```json
{
  "ReverseProxy": {
    "Routes": {
      "service1-route": {
        "ClusterId": "service1-cluster",
        "Match": {
          "Path": "/api/service1/{**catch-all}"
        }
      }
    },
    "Clusters": {
      "service1-cluster": {
        "Destinations": {
          "service1-destination": {
            "Address": "https://localhost:5001/"
          }
        }
      }
    }
  }
}
```

### Основные элементы конфигурации

#### Routes (Маршруты)
- **ClusterId**: Идентификатор кластера, куда направлять запросы
- **Match.Path**: Шаблон пути для сопоставления входящих запросов
  - `{**catch-all}` - захватывает все оставшиеся сегменты пути

#### Clusters (Кластеры)
- **LoadBalancingPolicy**: Политика балансировки
  - `RoundRobin` - Равномерное распределение по кругу
  - `WeightedRoundRobin` - Распределение с учетом весов (приоритетов)
  - `Random` - Случайный выбор
  - `LeastRequests` - Выбор наименее загруженного
  - `PowerOfTwoChoices` - Выбор лучшего из двух случайных
- **Destinations**: Список backend-серверов
  - **Address**: URL целевого сервера
  - **Metadata.Weight**: Вес сервера для `WeightedRoundRobin` (опционально)

## 🚀 Запуск

### Запуск gateway

```bash
cd src/MfbProxy.Gateway
dotnet run
```

Или через профиль HTTPS:

```bash
dotnet run --launch-profile https
```

### Тестирование

После запуска gateway и backend-сервисов:

```bash
# Через прокси к сервису 1
curl https://localhost:5050/api/service1/weatherforecast

# Через прокси к сервису 2 (с обычной Round Robin балансировкой)
curl https://localhost:5050/api/service2/weatherforecast

# Через прокси с взвешенной балансировкой (70% на powerful, 30% на standard)
curl https://localhost:5050/api/weighted/weatherforecast
```

## 📦 Установка дополнительных сервисов

Для тестирования прокси создайте тестовые сервисы:

```bash
# Создать сервис 1
dotnet new webapi -n Service1 -o src/Service1
dotnet sln add src/Service1/Service1.csproj

# Создать сервис 2
dotnet new webapi -n Service2 -o src/Service2
dotnet sln add src/Service2/Service2.csproj
```

Настройте порты в `launchSettings.json` каждого сервиса соответственно (5001, 5002, 5003, и т.д.).

## 🔧 Расширенные возможности YARP

### Weighted Round Robin (Взвешенная балансировка)

Используйте взвешенную балансировку для распределения трафика на основе мощности серверов:

```json
{
  "ReverseProxy": {
    "Routes": {
      "weighted-route": {
        "ClusterId": "weighted-cluster",
        "Match": {
          "Path": "/api/weighted/{**catch-all}"
        }
      }
    },
    "Clusters": {
      "weighted-cluster": {
        "LoadBalancingPolicy": "WeightedRoundRobin",
        "Destinations": {
          "powerful-server": {
            "Address": "https://localhost:7001/",
            "Metadata": {
              "Weight": "70"
            }
          },
          "standard-server": {
            "Address": "https://localhost:7002/",
            "Metadata": {
              "Weight": "30"
            }
          }
        }
      }
    }
  }
}
```

В этом примере:
- **powerful-server** получит ~70% трафика
- **standard-server** получит ~30% трафика

Это полезно когда у вас серверы с разной производительностью.

### Health Checks (Проверки здоровья)

```json
"Clusters": {
  "service1-cluster": {
    "HealthCheck": {
      "Active": {
        "Enabled": true,
        "Interval": "00:00:10",
        "Timeout": "00:00:05",
        "Policy": "ConsecutiveFailures",
        "Path": "/health"
      }
    }
  }
}
```

### Request Headers Transformation

```json
"Routes": {
  "service1-route": {
    "ClusterId": "service1-cluster",
    "Match": {
      "Path": "/api/service1/{**catch-all}"
    },
    "Transforms": [
      {
        "RequestHeader": "X-Forwarded-For",
        "Append": "true"
      },
      {
        "RequestHeader": "X-Original-Path",
        "Set": "{**catch-all}"
      }
    ]
  }
}
```

### Rate Limiting

```json
"Routes": {
  "service1-route": {
    "ClusterId": "service1-cluster",
    "RateLimiterPolicy": "fixed-window",
    "Match": {
      "Path": "/api/service1/{**catch-all}"
    }
  }
}
```

## 📚 Полезные ссылки

- [Официальная документация YARP](https://microsoft.github.io/reverse-proxy/)
- [How To Build a Load Balancer In .NET With YARP](https://medium.com/@michaelmaurice410/how-to-build-a-load-balancer-in-net-with-yarp-reverse-proxy-bf116933afd5)
- [YARP GitHub Repository](https://github.com/microsoft/reverse-proxy)

## 🛠️ Development

### Сборка решения

```bash
dotnet build
```

### Восстановление пакетов

```bash
dotnet restore
```

### Очистка

```bash
dotnet clean
```

## 📄 Лицензия

Этот проект создан для внутреннего использования MFB.

