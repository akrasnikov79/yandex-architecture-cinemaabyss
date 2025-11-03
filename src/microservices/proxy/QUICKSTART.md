# 🚀 Quick Start Guide

Быстрый старт для работы с MFB Proxy на базе YARP.

## ⚡ Быстрый запуск

### 1. Клонирование и сборка

```bash
# Перейти в директорию проекта
cd mfb-proxy

# Восстановить пакеты и собрать
dotnet restore
dotnet build
```

### 2. Запуск Gateway

```bash
cd src/MfbProxy.Gateway
dotnet run
```

Gateway запустится на:
- **HTTP**: `http://localhost:5000`
- **HTTPS**: `https://localhost:5050`

### 3. Проверка работы

```bash
# Проверить, что Gateway запущен
curl https://localhost:5050/
```

## 📝 Настройка первого маршрута

### Шаг 1: Откройте `appsettings.json`

```bash
code src/MfbProxy.Gateway/appsettings.json
```

### Шаг 2: Добавьте свой маршрут

```json
{
  "ReverseProxy": {
    "Routes": {
      "my-api-route": {
        "ClusterId": "my-api-cluster",
        "Match": {
          "Path": "/my-api/{**catch-all}"
        }
      }
    },
    "Clusters": {
      "my-api-cluster": {
        "Destinations": {
          "my-backend": {
            "Address": "https://your-backend-url.com/"
          }
        }
      }
    }
  }
}
```

### Шаг 3: Перезапустите Gateway

```bash
# Ctrl+C для остановки
dotnet run
```

### Шаг 4: Тестирование

```bash
# Запрос через прокси
curl https://localhost:5050/my-api/endpoint

# Будет перенаправлен на
# https://your-backend-url.com/my-api/endpoint
```

## 🔧 Типовые сценарии

### Сценарий 1: Единый API Gateway для микросервисов

**Задача**: Объединить несколько микросервисов под одним доменом

```json
{
  "ReverseProxy": {
    "Routes": {
      "auth-route": {
        "ClusterId": "auth-cluster",
        "Match": { "Path": "/api/auth/{**catch-all}" }
      },
      "users-route": {
        "ClusterId": "users-cluster",
        "Match": { "Path": "/api/users/{**catch-all}" }
      },
      "orders-route": {
        "ClusterId": "orders-cluster",
        "Match": { "Path": "/api/orders/{**catch-all}" }
      }
    },
    "Clusters": {
      "auth-cluster": {
        "Destinations": {
          "auth": { "Address": "https://auth-service:5001/" }
        }
      },
      "users-cluster": {
        "Destinations": {
          "users": { "Address": "https://users-service:5002/" }
        }
      },
      "orders-cluster": {
        "Destinations": {
          "orders": { "Address": "https://orders-service:5003/" }
        }
      }
    }
  }
}
```

**Использование**:
```bash
curl https://gateway.com/api/auth/login
curl https://gateway.com/api/users/123
curl https://gateway.com/api/orders/456
```

---

### Сценарий 2: Load Balancer с несколькими серверами

**Задача**: Распределить нагрузку между несколькими идентичными серверами

```json
{
  "ReverseProxy": {
    "Routes": {
      "api-route": {
        "ClusterId": "api-cluster",
        "Match": { "Path": "/api/{**catch-all}" }
      }
    },
    "Clusters": {
      "api-cluster": {
        "LoadBalancingPolicy": "RoundRobin",
        "Destinations": {
          "server1": { "Address": "https://api-server-1.com/" },
          "server2": { "Address": "https://api-server-2.com/" },
          "server3": { "Address": "https://api-server-3.com/" }
        }
      }
    }
  }
}
```

**Результат**: Запросы распределяются равномерно 1→2→3→1...

---

### Сценарий 3: Взвешенная балансировка (разные мощности серверов)

**Задача**: Мощный сервер должен обрабатывать больше запросов

```json
{
  "ReverseProxy": {
    "Routes": {
      "api-route": {
        "ClusterId": "weighted-cluster",
        "Match": { "Path": "/api/{**catch-all}" }
      }
    },
    "Clusters": {
      "weighted-cluster": {
        "LoadBalancingPolicy": "WeightedRoundRobin",
        "Destinations": {
          "powerful": {
            "Address": "https://powerful-server.com/",
            "Metadata": { "Weight": "70" }
          },
          "standard": {
            "Address": "https://standard-server.com/",
            "Metadata": { "Weight": "30" }
          }
        }
      }
    }
  }
}
```

**Результат**: 70% трафика → powerful, 30% → standard

---

### Сценарий 4: A/B Testing (Beta vs Production)

**Задача**: Направить бета-тестеров на новую версию

```json
{
  "ReverseProxy": {
    "Routes": {
      "beta-route": {
        "ClusterId": "beta-cluster",
        "Order": 1,
        "Match": {
          "Path": "/api/{**catch-all}",
          "Headers": [
            {
              "Name": "X-Beta-User",
              "Values": ["true"]
            }
          ]
        }
      },
      "prod-route": {
        "ClusterId": "prod-cluster",
        "Order": 2,
        "Match": { "Path": "/api/{**catch-all}" }
      }
    },
    "Clusters": {
      "beta-cluster": {
        "Destinations": {
          "beta": { "Address": "https://beta.example.com/" }
        }
      },
      "prod-cluster": {
        "Destinations": {
          "prod": { "Address": "https://prod.example.com/" }
        }
      }
    }
  }
}
```

**Использование**:
```bash
# Обычный пользователь → production
curl https://gateway.com/api/users

# Бета-тестер → beta
curl -H "X-Beta-User: true" https://gateway.com/api/users
```

---

### Сценарий 5: Health Checks (автоматическое отключение неработающих серверов)

**Задача**: Автоматически исключать недоступные серверы

```json
{
  "ReverseProxy": {
    "Routes": {
      "api-route": {
        "ClusterId": "ha-cluster",
        "Match": { "Path": "/api/{**catch-all}" }
      }
    },
    "Clusters": {
      "ha-cluster": {
        "LoadBalancingPolicy": "RoundRobin",
        "HealthCheck": {
          "Active": {
            "Enabled": true,
            "Interval": "00:00:10",
            "Timeout": "00:00:05",
            "Policy": "ConsecutiveFailures",
            "Path": "/health"
          }
        },
        "Destinations": {
          "server1": {
            "Address": "https://server1.com/",
            "Health": "https://server1.com/health"
          },
          "server2": {
            "Address": "https://server2.com/",
            "Health": "https://server2.com/health"
          }
        }
      }
    }
  }
}
```

**Результат**: Каждые 10 секунд проверяется `/health`, недоступные серверы исключаются

---

## 🧪 Тестирование локально

### Создание тестовых backend-сервисов

```bash
# Создать Service1
dotnet new webapi -n Service1 -o test-services/Service1
cd test-services/Service1

# Изменить порт на 5001 в launchSettings.json
# Запустить
dotnet run
```

```bash
# Создать Service2 (в другом терминале)
dotnet new webapi -n Service2 -o test-services/Service2
cd test-services/Service2

# Изменить порт на 5002 в launchSettings.json
# Запустить
dotnet run
```

### Настроить маршруты в Gateway

```json
{
  "ReverseProxy": {
    "Routes": {
      "service1-route": {
        "ClusterId": "service1-cluster",
        "Match": { "Path": "/service1/{**catch-all}" }
      },
      "service2-route": {
        "ClusterId": "service2-cluster",
        "Match": { "Path": "/service2/{**catch-all}" }
      }
    },
    "Clusters": {
      "service1-cluster": {
        "Destinations": {
          "service1": { "Address": "https://localhost:5001/" }
        }
      },
      "service2-cluster": {
        "Destinations": {
          "service2": { "Address": "https://localhost:5002/" }
        }
      }
    }
  }
}
```

### Тестирование

```bash
# Через Gateway
curl https://localhost:5050/service1/weatherforecast
curl https://localhost:5050/service2/weatherforecast

# Напрямую (для сравнения)
curl https://localhost:5001/weatherforecast
curl https://localhost:5002/weatherforecast
```

---

## 🐛 Отладка

### Включить подробное логирование

В `appsettings.Development.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Yarp": "Debug"
    }
  }
}
```

### Просмотр логов

```bash
dotnet run --environment Development
```

Вы увидите детальные логи YARP:
```
[DBG] YARP: ProxyInvoker: Proxying request to https://backend.com/api/users
[DBG] YARP: LoadBalancer: Selected destination: backend-1
```

---

## 📊 Мониторинг

### Добавить health check endpoint

В `Program.cs`:

```csharp
builder.Services.AddHealthChecks();

// После app.Build()
app.MapHealthChecks("/health");
```

### Проверить health

```bash
curl https://localhost:5050/health
# Ответ: Healthy
```

---

## 🔐 Production checklist

- [ ] Настроить HTTPS сертификаты
- [ ] Настроить CORS политики
- [ ] Добавить Health Checks для backend-сервисов
- [ ] Настроить Rate Limiting
- [ ] Включить Request/Response Logging
- [ ] Настроить мониторинг (Prometheus/Grafana)
- [ ] Добавить аутентификацию/авторизацию
- [ ] Протестировать failover сценарии
- [ ] Настроить retry policies
- [ ] Документировать API routes

---

## 📚 Дальнейшее изучение

- [Подробные примеры конфигураций](docs/configuration-examples.md)
- [Официальная документация YARP](https://microsoft.github.io/reverse-proxy/)
- [README проекта](README.md)

---

## ❓ FAQ

**Q: Как изменить порт Gateway?**  
A: В `Properties/launchSettings.json` измените `applicationUrl`

**Q: Можно ли перезагрузить конфигурацию без перезапуска?**  
A: Да, YARP автоматически перезагружает `appsettings.json` при изменении

**Q: Как добавить custom заголовки?**  
A: Используйте `Transforms` в маршруте (см. [configuration-examples.md](docs/configuration-examples.md))

**Q: Поддерживается ли WebSocket?**  
A: Да, YARP поддерживает WebSocket из коробки

**Q: Как отключить HTTPS редирект?**  
A: Закомментируйте `app.UseHttpsRedirection();` в `Program.cs`



