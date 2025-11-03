# 🧪 Testing Guide - MFB Proxy

Руководство по тестированию YARP Reverse Proxy с использованием тестовых сервисов.

## 📦 Тестовые сервисы

В проекте созданы два тестовых backend-сервиса:

| Сервис | Порт HTTPS | Порт HTTP | Описание |
|--------|------------|-----------|----------|
| **Service1** | 5001 | 5101 | Первый тестовый backend |
| **Service2** | 5002 | 5102 | Второй тестовый backend |
| **Gateway** | 5050 | 5000 | YARP Reverse Proxy |

---

## 🚀 Запуск тестовых сервисов

### Вариант 1: Запуск каждого сервиса в отдельном терминале

#### Терминал 1 - Service1
```bash
cd src/Service1
dotnet run
```

#### Терминал 2 - Service2
```bash
cd src/Service2
dotnet run
```

#### Терминал 3 - Gateway
```bash
cd src/MfbProxy.Gateway
dotnet run
```

### Вариант 2: Запуск через Visual Studio / Rider

1. Щелкните правой кнопкой на Solution
2. Выберите "Configure Startup Projects"
3. Выберите "Multiple startup projects"
4. Установите для всех трех проектов действие "Start"
5. Нажмите F5 для запуска

---

## 🧪 Тестирование endpoints

### 1. Прямое обращение к сервисам (без прокси)

#### Service1
```bash
# Health check
curl https://localhost:5001/health

# Weather forecast
curl https://localhost:5001/weatherforecast

# Test endpoint
curl https://localhost:5001/api/test
```

**Ожидаемый ответ от Service1:**
```json
{
  "message": "Response from Service1",
  "service": "Service1",
  "port": 5001,
  "timestamp": "2025-10-30T...",
  "machineName": "YOUR-PC-NAME"
}
```

#### Service2
```bash
# Health check
curl https://localhost:5002/health

# Weather forecast
curl https://localhost:5002/weatherforecast

# Test endpoint
curl https://localhost:5002/api/test
```

**Ожидаемый ответ от Service2:**
```json
{
  "message": "Response from Service2",
  "service": "Service2",
  "port": 5002,
  "timestamp": "2025-10-30T...",
  "machineName": "YOUR-PC-NAME"
}
```

---

### 2. Тестирование через Gateway (Reverse Proxy)

Убедитесь, что в `src/MfbProxy.Gateway/appsettings.json` настроены правильные маршруты:

```json
{
  "ReverseProxy": {
    "Routes": {
      "service1-route": {
        "ClusterId": "service1-cluster",
        "Match": {
          "Path": "/api/service1/{**catch-all}"
        }
      },
      "service2-route": {
        "ClusterId": "service2-cluster",
        "Match": {
          "Path": "/api/service2/{**catch-all}"
        }
      }
    },
    "Clusters": {
      "service1-cluster": {
        "Destinations": {
          "service1": {
            "Address": "https://localhost:5001/"
          }
        }
      },
      "service2-cluster": {
        "LoadBalancingPolicy": "RoundRobin",
        "Destinations": {
          "service2-1": {
            "Address": "https://localhost:5002/"
          }
        }
      }
    }
  }
}
```

#### Через Gateway к Service1
```bash
# Через прокси
curl https://localhost:5050/api/service1/test
curl https://localhost:5050/api/service1/weatherforecast
curl https://localhost:5050/api/service1/health
```

#### Через Gateway к Service2
```bash
# Через прокси
curl https://localhost:5050/api/service2/test
curl https://localhost:5050/api/service2/weatherforecast
curl https://localhost:5050/api/service2/health
```

---

## 🔄 Тестирование Round Robin балансировки

Для тестирования Round Robin запустите **два экземпляра Service2** на разных портах:

### Шаг 1: Запустить первый экземпляр Service2
```bash
cd src/Service2
dotnet run --launch-profile https
# Работает на https://localhost:5002
```

### Шаг 2: Запустить второй экземпляр Service2 (в другом терминале)
```bash
cd src/Service2
dotnet run --urls "https://localhost:5003;http://localhost:5103"
```

### Шаг 3: Обновить Gateway конфигурацию

В `src/MfbProxy.Gateway/appsettings.json`:

```json
{
  "ReverseProxy": {
    "Clusters": {
      "service2-cluster": {
        "LoadBalancingPolicy": "RoundRobin",
        "Destinations": {
          "service2-1": {
            "Address": "https://localhost:5002/"
          },
          "service2-2": {
            "Address": "https://localhost:5003/"
          }
        }
      }
    }
  }
}
```

### Шаг 4: Тестировать балансировку

Запустите несколько раз:
```bash
curl https://localhost:5050/api/service2/test
```

Вы увидите, что ответы чередуются:
```
1-й запрос: "Response from Service2" (Port: 5002)
2-й запрос: "Response from Service2" (Port: 5003)
3-й запрос: "Response from Service2" (Port: 5002)
4-й запрос: "Response from Service2" (Port: 5003)
...
```

---

## ⚖️ Тестирование Weighted Round Robin

### Шаг 1: Обновить конфигурацию

В `src/MfbProxy.Gateway/appsettings.json` добавьте:

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
          "service1": {
            "Address": "https://localhost:5001/",
            "Metadata": {
              "Weight": "70"
            }
          },
          "service2": {
            "Address": "https://localhost:5002/",
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

### Шаг 2: Тестировать взвешенную балансировку

Запустите 10 раз:
```bash
for i in {1..10}; do curl -s https://localhost:5050/api/weighted/test | jq .service; done
```

**Ожидаемый результат:**
- ~70% запросов пойдут на Service1 (порт 5001)
- ~30% запросов пойдут на Service2 (порт 5002)

Пример вывода:
```
"Service1"
"Service1"
"Service2"
"Service1"
"Service1"
"Service1"
"Service2"
"Service1"
"Service1"
"Service2"
```

---

## 📊 Load Testing с curl

### Простой bash скрипт для нагрузочного тестирования

```bash
#!/bin/bash

echo "Load testing Gateway - 100 requests"
echo "===================================="

for i in {1..100}; do
  response=$(curl -s https://localhost:5050/api/service2/test)
  service=$(echo $response | jq -r .service)
  port=$(echo $response | jq -r .port)
  echo "Request $i: $service (Port: $port)"
done
```

Сохраните как `load-test.sh` и запустите:
```bash
chmod +x load-test.sh
./load-test.sh
```

### PowerShell скрипт для Windows

```powershell
# load-test.ps1
Write-Host "Load testing Gateway - 100 requests" -ForegroundColor Green
Write-Host "====================================" -ForegroundColor Green

$results = @{}

for ($i = 1; $i -le 100; $i++) {
    try {
        $response = Invoke-RestMethod -Uri "https://localhost:5050/api/service2/test" -SkipCertificateCheck
        $service = $response.service
        $port = $response.port
        
        if ($results.ContainsKey($port)) {
            $results[$port]++
        } else {
            $results[$port] = 1
        }
        
        Write-Host "Request $i : $service (Port: $port)"
    } catch {
        Write-Host "Request $i : ERROR" -ForegroundColor Red
    }
}

Write-Host "`nResults Summary:" -ForegroundColor Yellow
Write-Host "================" -ForegroundColor Yellow
foreach ($key in $results.Keys) {
    Write-Host "Port $key : $($results[$key]) requests"
}
```

Запустите:
```powershell
.\load-test.ps1
```

---

## 🐛 Troubleshooting

### Gateway не может подключиться к сервисам

**Проблема**: `502 Bad Gateway` или `503 Service Unavailable`

**Решение**:
1. Убедитесь, что Service1 и Service2 запущены:
   ```bash
   curl https://localhost:5001/health
   curl https://localhost:5002/health
   ```

2. Проверьте логи Gateway:
   ```bash
   cd src/MfbProxy.Gateway
   dotnet run
   # Смотрите на output для ошибок
   ```

3. Проверьте порты в appsettings.json

### SSL/TLS ошибки

**Проблема**: Certificate errors при тестировании

**Решение**:
```bash
# Для curl используйте -k или --insecure
curl -k https://localhost:5050/api/service1/test

# Для PowerShell используйте -SkipCertificateCheck
Invoke-RestMethod -Uri "https://localhost:5050/api/service1/test" -SkipCertificateCheck
```

### Порты уже используются

**Проблема**: `Address already in use`

**Решение**:

Windows:
```powershell
# Найти процесс на порту
netstat -ano | findstr :5001

# Убить процесс
taskkill /PID <PID> /F
```

Linux/Mac:
```bash
# Найти процесс на порту
lsof -i :5001

# Убить процесс
kill -9 <PID>
```

---

## 📈 Monitoring во время тестирования

### Посмотреть логи всех сервисов

В отдельных терминалах держите открытыми логи:

```bash
# Terminal 1
cd src/Service1 && dotnet run

# Terminal 2
cd src/Service2 && dotnet run

# Terminal 3
cd src/MfbProxy.Gateway && dotnet run
```

При каждом запросе вы увидите логи в реальном времени.

---

## ✅ Checklist для тестирования

- [ ] Service1 запущен и отвечает на `https://localhost:5001/health`
- [ ] Service2 запущен и отвечает на `https://localhost:5002/health`
- [ ] Gateway запущен на `https://localhost:5050`
- [ ] Прямое обращение к сервисам работает
- [ ] Обращение через Gateway работает
- [ ] Round Robin балансировка распределяет запросы равномерно
- [ ] Weighted Round Robin соблюдает пропорции (70/30)
- [ ] Health checks возвращают корректный статус

---

## 🎯 Примеры тестовых сценариев

### Сценарий 1: Basic Routing
```bash
# Запросы должны попадать на разные сервисы
curl https://localhost:5050/api/service1/test  # → Service1
curl https://localhost:5050/api/service2/test  # → Service2
```

### Сценарий 2: Load Balancing
```bash
# Запустить 2 экземпляра Service2 на портах 5002 и 5003
# Настроить Round Robin в Gateway
# Запросы должны чередоваться между портами
for i in {1..10}; do 
  curl -s https://localhost:5050/api/service2/test | jq .port
done
```

### Сценарий 3: Weighted Distribution
```bash
# Настроить Weighted Round Robin (70% Service1, 30% Service2)
# Запустить 100 запросов
# Подсчитать распределение
```

### Сценарий 4: Failover
```bash
# Запустить Gateway и оба сервиса
# Отправить запрос - должен работать
curl https://localhost:5050/api/service2/test

# Остановить один из Service2
# Запрос должен все еще работать (переключится на доступный)
curl https://localhost:5050/api/service2/test
```

---

## 📚 Дополнительные инструменты

### Postman Collection

Создайте Postman коллекцию с запросами:

1. **Direct Service1**: `GET https://localhost:5001/api/test`
2. **Direct Service2**: `GET https://localhost:5002/api/test`
3. **Via Gateway Service1**: `GET https://localhost:5050/api/service1/test`
4. **Via Gateway Service2**: `GET https://localhost:5050/api/service2/test`
5. **Weighted**: `GET https://localhost:5050/api/weighted/test`

### k6 Load Testing Script

```javascript
import http from 'k6/http';
import { check, sleep } from 'k6';

export let options = {
  stages: [
    { duration: '30s', target: 20 },
    { duration: '1m', target: 50 },
    { duration: '30s', target: 0 },
  ],
};

export default function () {
  let response = http.get('https://localhost:5050/api/service2/test', {
    insecureSkipVerify: true,
  });
  
  check(response, {
    'status is 200': (r) => r.status === 200,
    'has service field': (r) => r.json('service') !== undefined,
  });
  
  sleep(1);
}
```

Запустите:
```bash
k6 run load-test.js
```

---

**Успешного тестирования!** 🎉



