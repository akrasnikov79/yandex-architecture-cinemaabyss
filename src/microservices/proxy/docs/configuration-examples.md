# YARP Configuration Examples

## 📋 Примеры конфигураций

### 1. Простое проксирование

```json
{
  "ReverseProxy": {
    "Routes": {
      "simple-route": {
        "ClusterId": "backend-cluster",
        "Match": {
          "Path": "/api/{**catch-all}"
        }
      }
    },
    "Clusters": {
      "backend-cluster": {
        "Destinations": {
          "backend": {
            "Address": "https://backend.example.com/"
          }
        }
      }
    }
  }
}
```

**Результат**: Все запросы к `/api/*` перенаправляются на `https://backend.example.com/api/*`

---

### 2. Round Robin Load Balancing

```json
{
  "ReverseProxy": {
    "Routes": {
      "lb-route": {
        "ClusterId": "lb-cluster",
        "Match": {
          "Path": "/api/{**catch-all}"
        }
      }
    },
    "Clusters": {
      "lb-cluster": {
        "LoadBalancingPolicy": "RoundRobin",
        "Destinations": {
          "server1": {
            "Address": "https://server1.example.com/"
          },
          "server2": {
            "Address": "https://server2.example.com/"
          },
          "server3": {
            "Address": "https://server3.example.com/"
          }
        }
      }
    }
  }
}
```

**Результат**: Запросы распределяются равномерно между тремя серверами по кругу: server1 → server2 → server3 → server1...

---

### 3. Weighted Round Robin (Взвешенная балансировка)

```json
{
  "ReverseProxy": {
    "Routes": {
      "weighted-route": {
        "ClusterId": "weighted-cluster",
        "Match": {
          "Path": "/api/{**catch-all}"
        }
      }
    },
    "Clusters": {
      "weighted-cluster": {
        "LoadBalancingPolicy": "WeightedRoundRobin",
        "Destinations": {
          "powerful-server": {
            "Address": "https://powerful.example.com/",
            "Metadata": {
              "Weight": "80"
            }
          },
          "medium-server": {
            "Address": "https://medium.example.com/",
            "Metadata": {
              "Weight": "15"
            }
          },
          "small-server": {
            "Address": "https://small.example.com/",
            "Metadata": {
              "Weight": "5"
            }
          }
        }
      }
    }
  }
}
```

**Результат**: 
- powerful-server получает 80% трафика
- medium-server получает 15% трафика
- small-server получает 5% трафика

**Применение**: Когда серверы имеют разную производительность (CPU, RAM, пропускная способность)

---

### 4. Маршрутизация по методу HTTP

```json
{
  "ReverseProxy": {
    "Routes": {
      "read-route": {
        "ClusterId": "read-cluster",
        "Match": {
          "Path": "/api/data/{**catch-all}",
          "Methods": [ "GET" ]
        }
      },
      "write-route": {
        "ClusterId": "write-cluster",
        "Match": {
          "Path": "/api/data/{**catch-all}",
          "Methods": [ "POST", "PUT", "DELETE" ]
        }
      }
    },
    "Clusters": {
      "read-cluster": {
        "Destinations": {
          "read-replica": {
            "Address": "https://read-db.example.com/"
          }
        }
      },
      "write-cluster": {
        "Destinations": {
          "master-db": {
            "Address": "https://master-db.example.com/"
          }
        }
      }
    }
  }
}
```

**Результат**: 
- GET запросы идут на read-replica (читающую реплику БД)
- POST/PUT/DELETE идут на master-db (основную БД)

---

### 5. Маршрутизация по заголовкам

```json
{
  "ReverseProxy": {
    "Routes": {
      "beta-route": {
        "ClusterId": "beta-cluster",
        "Match": {
          "Path": "/api/{**catch-all}",
          "Headers": [
            {
              "Name": "X-Version",
              "Values": [ "beta" ],
              "Mode": "ExactHeader"
            }
          ]
        }
      },
      "prod-route": {
        "ClusterId": "prod-cluster",
        "Match": {
          "Path": "/api/{**catch-all}"
        }
      }
    },
    "Clusters": {
      "beta-cluster": {
        "Destinations": {
          "beta-server": {
            "Address": "https://beta.example.com/"
          }
        }
      },
      "prod-cluster": {
        "Destinations": {
          "prod-server": {
            "Address": "https://prod.example.com/"
          }
        }
      }
    }
  }
}
```

**Результат**: Запросы с заголовком `X-Version: beta` идут на бета-сервер, остальные - на продакшн

---

### 6. Health Checks (Проверка здоровья)

```json
{
  "ReverseProxy": {
    "Routes": {
      "api-route": {
        "ClusterId": "api-cluster",
        "Match": {
          "Path": "/api/{**catch-all}"
        }
      }
    },
    "Clusters": {
      "api-cluster": {
        "LoadBalancingPolicy": "RoundRobin",
        "HealthCheck": {
          "Active": {
            "Enabled": true,
            "Interval": "00:00:10",
            "Timeout": "00:00:05",
            "Policy": "ConsecutiveFailures",
            "Path": "/health"
          },
          "Passive": {
            "Enabled": true,
            "Policy": "TransportFailureRate",
            "ReactivationPeriod": "00:01:00"
          }
        },
        "Destinations": {
          "server1": {
            "Address": "https://server1.example.com/",
            "Health": "https://server1.example.com/health"
          },
          "server2": {
            "Address": "https://server2.example.com/",
            "Health": "https://server2.example.com/health"
          }
        }
      }
    }
  }
}
```

**Результат**: 
- Active health checks: каждые 10 секунд проверяется endpoint `/health`
- Passive health checks: автоматическое отключение неработающих серверов
- Переактивация через 1 минуту

---

### 7. Request/Response Transformations

```json
{
  "ReverseProxy": {
    "Routes": {
      "transform-route": {
        "ClusterId": "api-cluster",
        "Match": {
          "Path": "/external/{**catch-all}"
        },
        "Transforms": [
          {
            "PathPattern": "/internal/{**catch-all}"
          },
          {
            "RequestHeader": "X-Forwarded-Proto",
            "Set": "https"
          },
          {
            "RequestHeader": "X-Api-Key",
            "Set": "secret-key-123"
          },
          {
            "ResponseHeader": "X-Powered-By",
            "Set": "YARP"
          }
        ]
      }
    },
    "Clusters": {
      "api-cluster": {
        "Destinations": {
          "api-server": {
            "Address": "https://api.example.com/"
          }
        }
      }
    }
  }
}
```

**Результат**: 
- `/external/users` → `/internal/users` на backend
- Добавляются заголовки запроса: `X-Forwarded-Proto`, `X-Api-Key`
- Добавляется заголовок ответа: `X-Powered-By`

---

### 8. Session Affinity (Sticky Sessions)

```json
{
  "ReverseProxy": {
    "Routes": {
      "sticky-route": {
        "ClusterId": "sticky-cluster",
        "Match": {
          "Path": "/api/{**catch-all}"
        }
      }
    },
    "Clusters": {
      "sticky-cluster": {
        "LoadBalancingPolicy": "RoundRobin",
        "SessionAffinity": {
          "Enabled": true,
          "Policy": "Cookie",
          "FailurePolicy": "Redistribute",
          "Settings": {
            "CustomCookieName": "MyAffinityCookie"
          }
        },
        "Destinations": {
          "server1": {
            "Address": "https://server1.example.com/"
          },
          "server2": {
            "Address": "https://server2.example.com/"
          }
        }
      }
    }
  }
}
```

**Результат**: Пользователь всегда попадает на тот же сервер (через cookie)

---

### 9. Rate Limiting

```json
{
  "ReverseProxy": {
    "Routes": {
      "limited-route": {
        "ClusterId": "api-cluster",
        "RateLimiterPolicy": "fixed-window",
        "Match": {
          "Path": "/api/{**catch-all}"
        }
      }
    },
    "Clusters": {
      "api-cluster": {
        "Destinations": {
          "api-server": {
            "Address": "https://api.example.com/"
          }
        }
      }
    }
  }
}
```

Дополнительно в `Program.cs`:

```csharp
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("fixed-window", limiterOptions =>
    {
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.PermitLimit = 100;
        limiterOptions.QueueLimit = 0;
    });
});

// После app.Build()
app.UseRateLimiter();
```

**Результат**: Максимум 100 запросов в минуту на клиента

---

### 10. Микросервисная архитектура

```json
{
  "ReverseProxy": {
    "Routes": {
      "auth-route": {
        "ClusterId": "auth-service",
        "Match": {
          "Path": "/api/auth/{**catch-all}"
        }
      },
      "users-route": {
        "ClusterId": "users-service",
        "Match": {
          "Path": "/api/users/{**catch-all}"
        }
      },
      "orders-route": {
        "ClusterId": "orders-service",
        "Match": {
          "Path": "/api/orders/{**catch-all}"
        }
      },
      "payments-route": {
        "ClusterId": "payments-service",
        "Match": {
          "Path": "/api/payments/{**catch-all}"
        }
      }
    },
    "Clusters": {
      "auth-service": {
        "Destinations": {
          "auth": {
            "Address": "https://auth-service.internal:5001/"
          }
        }
      },
      "users-service": {
        "LoadBalancingPolicy": "RoundRobin",
        "Destinations": {
          "users1": {
            "Address": "https://users-service-1.internal:5002/"
          },
          "users2": {
            "Address": "https://users-service-2.internal:5002/"
          }
        }
      },
      "orders-service": {
        "LoadBalancingPolicy": "WeightedRoundRobin",
        "Destinations": {
          "orders-primary": {
            "Address": "https://orders-primary.internal:5003/",
            "Metadata": {
              "Weight": "75"
            }
          },
          "orders-secondary": {
            "Address": "https://orders-secondary.internal:5003/",
            "Metadata": {
              "Weight": "25"
            }
          }
        }
      },
      "payments-service": {
        "Destinations": {
          "payments": {
            "Address": "https://payments-service.internal:5004/"
          }
        }
      }
    }
  }
}
```

**Результат**: API Gateway для микросервисной архитектуры с разными стратегиями балансировки для разных сервисов

---

## 🎯 Выбор политики балансировки

| Политика | Когда использовать | Преимущества |
|----------|-------------------|--------------|
| **RoundRobin** | Серверы одинаковой мощности | Простота, равномерность |
| **WeightedRoundRobin** | Серверы разной мощности | Оптимальное использование ресурсов |
| **Random** | Stateless приложения | Минимальные накладные расходы |
| **LeastRequests** | Длинные запросы | Равномерная нагрузка по запросам |
| **PowerOfTwoChoices** | Высоконагруженные системы | Хороший баланс скорости и качества |

---

## 🔧 Полезные команды

```bash
# Проверить конфигурацию
dotnet run --no-build

# Запустить с конкретным appsettings
dotnet run --environment Production

# Логирование YARP
export ASPNETCORE_ENVIRONMENT=Development
dotnet run
```

---

## 📚 Дополнительные ресурсы

- [YARP Documentation](https://microsoft.github.io/reverse-proxy/)
- [Load Balancing Algorithms](https://microsoft.github.io/reverse-proxy/articles/load-balancing.html)
- [Configuration Reference](https://microsoft.github.io/reverse-proxy/articles/config-files.html)



