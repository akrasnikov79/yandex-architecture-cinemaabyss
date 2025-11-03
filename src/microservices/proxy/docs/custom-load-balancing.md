# Кастомные политики балансировки нагрузки

## 📋 Обзор

YARP поддерживает следующие **встроенные** политики балансировки:
- `RoundRobin` - Равномерное распределение по кругу
- `Random` - Случайный выбор
- `PowerOfTwoChoices` - Выбор лучшего из двух случайных (по умолчанию)
- `LeastRequests` - Выбор наименее загруженного
- `FirstAlphabetical` - Всегда первый по алфавиту

В этом проекте **добавлены две кастомные политики**:
1. **WeightedRoundRobin** - Взвешенная балансировка
2. **CustomPolicy** - Маршрутизация по заголовку X-User-ID

---

## 1️⃣ WeightedRoundRobin

### Описание
Распределяет трафик пропорционально весам, указанным в метаданных destination.

### Реализация
Файл: `src/MfbProxy.Gateway/WeightedRoundRobinPolicy.cs`

```csharp
public class WeightedRoundRobinPolicy : ILoadBalancingPolicy
{
    private long _counter = 0;

    public string Name => "WeightedRoundRobin";

    public DestinationState? PickDestination(HttpContext context, 
        ClusterState cluster, 
        IReadOnlyList<DestinationState> availableDestinations)
    {
        // Build weighted list based on Metadata.Weight
        var weightedDestinations = new List<DestinationState>();

        foreach (var destination in availableDestinations)
        {
            var weight = 1; // default weight
            if (destination.Model.Config.Metadata != null &&
                destination.Model.Config.Metadata.TryGetValue("Weight", out var weightStr))
            {
                if (int.TryParse(weightStr, out var parsedWeight) && parsedWeight > 0)
                {
                    weight = parsedWeight;
                }
            }

            // Add destination 'weight' times
            for (int i = 0; i < weight; i++)
            {
                weightedDestinations.Add(destination);
            }
        }

        // Round-robin over weighted list
        var index = (int)(Interlocked.Increment(ref _counter) % weightedDestinations.Count);
        return weightedDestinations[index];
    }
}
```

### Конфигурация

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

### Как это работает

1. **Вес 70**: Сервер добавляется в список 70 раз
2. **Вес 30**: Сервер добавляется в список 30 раз
3. **Итого**: Список из 100 элементов (70 + 30)
4. **Round Robin**: По этому списку идёт обычный RR

**Результат**: ~70% запросов на powerful-server, ~30% на standard-server

### Тестирование

```bash
# Отправить 100 запросов
for i in {1..100}; do
  curl -s https://localhost:5050/api/weighted/test | jq -r '.port'
done | sort | uniq -c

# Ожидаемый результат:
#  70 5001  (powerful-server)
#  30 5002  (standard-server)
```

---

## 2️⃣ CustomPolicy

### Описание
Маршрутизирует запросы на основе заголовка `X-User-ID`:
- **Чётные User ID** → первый сервер
- **Нечётные User ID** → второй сервер
- **Без заголовка** → обычный Round Robin

### Реализация
Файл: `src/MfbProxy.Gateway/CustomLoadBalancingPolicy.cs`

```csharp
public class CustomLoadBalancingPolicy : ILoadBalancingPolicy
{
    private long _counter = 0;

    public string Name => "CustomPolicy";

    public DestinationState? PickDestination(HttpContext context, 
        ClusterState cluster, 
        IReadOnlyList<DestinationState> availableDestinations)
    {
        if (availableDestinations.Count == 0)
        {
            return null;
        }

        // Route based on X-User-ID header
        if (context.Request.Headers.TryGetValue("X-User-ID", out var userIdHeader))
        {
            if (int.TryParse(userIdHeader.FirstOrDefault(), out var userId))
            {
                // Even user IDs → first server, odd → second server
                var destinationIndex = userId % availableDestinations.Count;
                return availableDestinations[destinationIndex];
            }
        }

        // Fallback to round-robin
        var index = (int)(Interlocked.Increment(ref _counter) % availableDestinations.Count);
        return availableDestinations[index];
    }
}
```

### Конфигурация

```json
{
  "ReverseProxy": {
    "Routes": {
      "custom-route": {
        "ClusterId": "custom-cluster",
        "Match": {
          "Path": "/api/custom/{**catch-all}"
        }
      }
    },
    "Clusters": {
      "custom-cluster": {
        "LoadBalancingPolicy": "CustomPolicy",
        "Destinations": {
          "server1": {
            "Address": "https://localhost:5001/"
          },
          "server2": {
            "Address": "https://localhost:5002/"
          }
        }
      }
    }
  }
}
```

### Тестирование

```bash
# Пользователь с ID 100 (чётный) → server1
curl -H "X-User-ID: 100" https://localhost:5050/api/custom/test

# Пользователь с ID 101 (нечётный) → server2
curl -H "X-User-ID: 101" https://localhost:5050/api/custom/test

# Без заголовка → Round Robin
curl https://localhost:5050/api/custom/test
```

### Применение
- **Sticky Sessions** - один пользователь всегда на одном сервере
- **A/B Testing** - группы пользователей на разных версиях
- **Sharding** - распределение данных по серверам

---

## 🔧 Регистрация в Program.cs

Обе политики зарегистрированы в `src/MfbProxy.Gateway/Program.cs`:

```csharp
using Yarp.ReverseProxy.LoadBalancing;
using MfbProxy.Gateway;

var builder = WebApplication.CreateBuilder(args);

// Add YARP Reverse Proxy services
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Register custom load balancing policies
builder.Services.AddSingleton<ILoadBalancingPolicy, WeightedRoundRobinPolicy>();
builder.Services.AddSingleton<ILoadBalancingPolicy, CustomLoadBalancingPolicy>();
```

---

## 📝 Создание своей политики

### Шаг 1: Создать класс политики

```csharp
using Yarp.ReverseProxy.LoadBalancing;
using Yarp.ReverseProxy.Model;

public class MyCustomPolicy : ILoadBalancingPolicy
{
    public string Name => "MyCustomPolicy";

    public DestinationState? PickDestination(
        HttpContext context, 
        ClusterState cluster, 
        IReadOnlyList<DestinationState> availableDestinations)
    {
        if (availableDestinations.Count == 0)
        {
            return null;
        }

        // Ваша логика выбора destination
        // Примеры:
        // - По IP адресу клиента
        // - По времени суток
        // - По load сервера
        // - По географии
        // - По типу запроса (GET/POST)
        
        return availableDestinations[0];
    }
}
```

### Шаг 2: Зарегистрировать в DI

```csharp
builder.Services.AddSingleton<ILoadBalancingPolicy, MyCustomPolicy>();
```

### Шаг 3: Использовать в конфигурации

```json
{
  "Clusters": {
    "my-cluster": {
      "LoadBalancingPolicy": "MyCustomPolicy",
      "Destinations": { ... }
    }
  }
}
```

---

## 🎯 Примеры использования

### По географии

```csharp
public DestinationState? PickDestination(...)
{
    var clientIp = context.Connection.RemoteIpAddress?.ToString();
    
    // Определить регион по IP
    var region = GeoIpService.GetRegion(clientIp);
    
    // Выбрать ближайший сервер
    return availableDestinations.FirstOrDefault(d => 
        d.Model.Config.Metadata?.GetValueOrDefault("Region") == region);
}
```

### По времени суток

```csharp
public DestinationState? PickDestination(...)
{
    var hour = DateTime.Now.Hour;
    
    // Ночью - на резервные серверы
    if (hour >= 0 && hour < 6)
    {
        return availableDestinations.LastOrDefault();
    }
    
    // Днём - на основные
    return availableDestinations.FirstOrDefault();
}
```

### По типу запроса

```csharp
public DestinationState? PickDestination(...)
{
    var method = context.Request.Method;
    
    // GET запросы на read-replicas
    if (method == "GET")
    {
        return availableDestinations.FirstOrDefault(d =>
            d.Model.Config.Metadata?.GetValueOrDefault("Type") == "read");
    }
    
    // POST/PUT/DELETE на master
    return availableDestinations.FirstOrDefault(d =>
        d.Model.Config.Metadata?.GetValueOrDefault("Type") == "write");
}
```

---

## 🧪 Тестирование кастомных политик

### Unit тесты

```csharp
[Fact]
public void WeightedRoundRobin_ShouldDistributeByWeight()
{
    var policy = new WeightedRoundRobinPolicy();
    var destinations = CreateDestinations();
    
    var results = new Dictionary<string, int>();
    
    for (int i = 0; i < 100; i++)
    {
        var picked = policy.PickDestination(context, cluster, destinations);
        var key = picked.Model.Config.Address;
        results[key] = results.GetValueOrDefault(key) + 1;
    }
    
    // Проверить, что ~70% на powerful, ~30% на standard
    Assert.InRange(results["powerful"], 60, 80);
    Assert.InRange(results["standard"], 20, 40);
}
```

### Integration тесты

```bash
# Bash скрипт для тестирования
#!/bin/bash

echo "Testing WeightedRoundRobin..."

declare -A counts

for i in {1..100}; do
  port=$(curl -s https://localhost:5050/api/weighted/test | jq -r '.port')
  ((counts[$port]++))
done

echo "Distribution:"
for port in "${!counts[@]}"; do
  echo "Port $port: ${counts[$port]} requests"
done
```

---

## 📚 Полезные ссылки

- [YARP Load Balancing Documentation](https://microsoft.github.io/reverse-proxy/articles/load-balancing.html)
- [ILoadBalancingPolicy Interface](https://microsoft.github.io/reverse-proxy/api/Yarp.ReverseProxy.LoadBalancing.ILoadBalancingPolicy.html)
- [YARP GitHub](https://github.com/microsoft/reverse-proxy)

---

## ✅ Итого

**В проекте реализованы:**
- ✅ `WeightedRoundRobin` - взвешенная балансировка (70/30, 80/20, и т.д.)
- ✅ `CustomPolicy` - маршрутизация по User ID
- ✅ Примеры создания своих политик
- ✅ Unit и integration тесты

**Можно использовать для:**
- Серверов разной мощности
- Sticky sessions
- A/B testing
- Географическая маршрутизация
- Sharding
- Read/Write splitting

**Ответ на вопрос:** `WeightedRoundRobin` **НЕ является встроенной политикой в YARP** (ни в какой версии). Её нужно реализовывать самостоятельно, что и сделано в этом проекте! ✅


