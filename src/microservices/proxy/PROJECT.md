# MFB Proxy Project Information

## 📦 Версии и зависимости

### .NET SDK
- **Target Framework**: .NET 8.0
- **Language**: C# 12

### NuGet Packages
- **Yarp.ReverseProxy**: 2.3.0
- **Microsoft.AspNetCore.OpenApi**: 8.0.11

## 🗂️ Структура проекта

```
mfb-proxy/
├── src/
│   └── MfbProxy.Gateway/          # YARP Reverse Proxy Gateway
│       ├── Program.cs              # Главная точка входа
│       ├── appsettings.json        # Конфигурация routes & clusters
│       ├── appsettings.Development.json
│       ├── MfbProxy.Gateway.csproj
│       └── Properties/
│           └── launchSettings.json # Порты и профили запуска
├── docs/
│   └── configuration-examples.md   # Примеры конфигураций
├── MfbProxy.sln                    # Solution файл
├── README.md                       # Основная документация
├── QUICKSTART.md                   # Руководство быстрого старта
├── PROJECT.md                      # Информация о проекте (этот файл)
└── .gitignore                      # Git ignore для .NET
```

## 🌐 Endpoints

### Gateway
- **HTTP**: `http://localhost:5000`
- **HTTPS**: `https://localhost:5050`

### Настроенные маршруты

| Маршрут | Назначение | Балансировка |
|---------|------------|--------------|
| `/api/service1/{**catch-all}` | Service1 (localhost:5001) | - |
| `/api/service2/{**catch-all}` | Service2 (localhost:5002, 5003) | Round Robin |
| `/api/weighted/{**catch-all}` | Weighted Cluster (localhost:7001, 7002) | Weighted RR (70/30) |

## 🔧 Конфигурация

### Program.cs

Ключевые компоненты:

```csharp
// YARP Services
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// CORS
builder.Services.AddCors();

// Middleware
app.UseHttpsRedirection();
app.UseCors();
app.MapReverseProxy();
```

### appsettings.json

Структура конфигурации YARP:

```json
{
  "ReverseProxy": {
    "Routes": {
      "route-name": {
        "ClusterId": "cluster-id",
        "Match": {
          "Path": "/path/{**catch-all}"
        }
      }
    },
    "Clusters": {
      "cluster-id": {
        "LoadBalancingPolicy": "RoundRobin | WeightedRoundRobin",
        "Destinations": {
          "destination-name": {
            "Address": "https://backend-url/",
            "Metadata": {
              "Weight": "70"  // Для WeightedRoundRobin
            }
          }
        }
      }
    }
  }
}
```

## 🚀 Политики балансировки

Проект настроен с тремя типами кластеров:

### 1. Single Destination
- **Кластер**: `service1-cluster`
- **Назначение**: Один backend без балансировки
- **Использование**: Stateful сервисы или единичные инстансы

### 2. Round Robin
- **Кластер**: `service2-cluster`
- **Назначения**: 2 равнозначных сервера
- **Алгоритм**: Равномерное распределение по кругу
- **Использование**: Stateless приложения с одинаковыми серверами

### 3. Weighted Round Robin
- **Кластер**: `weighted-cluster`
- **Назначения**: 
  - `powerful-server` (Weight: 70)
  - `standard-server` (Weight: 30)
- **Алгоритм**: Распределение пропорционально весам
- **Использование**: Серверы разной мощности

## 🔐 Безопасность

### Текущая конфигурация
- ✅ HTTPS Redirection включен
- ✅ CORS настроен (AllowAny для dev)
- ⚠️ Без аутентификации (нужно добавить для production)

### Рекомендации для Production
```csharp
// В Program.cs добавить:

// 1. Authentication & Authorization
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => { /* config */ });
builder.Services.AddAuthorization();

// 2. CORS ограничения
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("https://yourdomain.com")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// 3. Rate Limiting
builder.Services.AddRateLimiter(/* config */);

// 4. HSTS
app.UseHsts();
```

## 📊 Мониторинг и логирование

### Логирование YARP

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

### Рекомендуемые метрики для мониторинга
- Request rate (запросов/сек)
- Error rate (% ошибок)
- Response time (p50, p95, p99)
- Backend health status
- Active connections
- Circuit breaker state

## 🧪 Тестирование

### Unit тесты (TODO)
```bash
dotnet new xunit -n MfbProxy.Tests
dotnet sln add MfbProxy.Tests/MfbProxy.Tests.csproj
```

### Integration тесты (TODO)
```bash
dotnet new xunit -n MfbProxy.IntegrationTests
# Использовать WebApplicationFactory<Program>
```

### Load тесты
Рекомендуемые инструменты:
- **k6** - для load testing
- **Apache JMeter** - для функционального тестирования
- **Bombardier** - легкий CLI tool

Пример с k6:
```javascript
import http from 'k6/http';
import { check } from 'k6';

export let options = {
  vus: 100,
  duration: '30s',
};

export default function() {
  let response = http.get('https://localhost:5050/api/service1/test');
  check(response, {
    'status is 200': (r) => r.status === 200,
  });
}
```

## 🐳 Docker Support (TODO)

### Dockerfile для Gateway

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/MfbProxy.Gateway/MfbProxy.Gateway.csproj", "MfbProxy.Gateway/"]
RUN dotnet restore "MfbProxy.Gateway/MfbProxy.Gateway.csproj"
COPY src/MfbProxy.Gateway/ MfbProxy.Gateway/
WORKDIR "/src/MfbProxy.Gateway"
RUN dotnet build "MfbProxy.Gateway.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MfbProxy.Gateway.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MfbProxy.Gateway.dll"]
```

### Docker Compose

```yaml
version: '3.8'
services:
  gateway:
    build: .
    ports:
      - "5050:443"
      - "5000:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ASPNETCORE_URLS=https://+:443;http://+:80
    volumes:
      - ./appsettings.json:/app/appsettings.json:ro
```

## 📝 Roadmap

### Phase 1 ✅ (Completed)
- [x] Базовая настройка YARP
- [x] Конфигурация routes и clusters
- [x] Round Robin балансировка
- [x] Weighted Round Robin балансировка
- [x] Документация

### Phase 2 (Планируется)
- [ ] Health checks для backend сервисов
- [ ] Circuit breaker pattern
- [ ] Rate limiting
- [ ] Request/Response transformations
- [ ] Аутентификация и авторизация

### Phase 3 (Будущее)
- [ ] Metrics и мониторинг (Prometheus)
- [ ] Distributed tracing (OpenTelemetry)
- [ ] Docker/Kubernetes deployment
- [ ] CI/CD pipeline
- [ ] Load testing suite
- [ ] Admin UI для управления маршрутами

## 🤝 Contributing

### Code Style
- Следовать .NET Coding Conventions
- Использовать PascalCase для публичных членов
- Использовать camelCase для локальных переменных
- Добавлять XML комментарии для публичных API

### Pull Request Process
1. Обновить документацию
2. Добавить тесты
3. Убедиться что `dotnet build` проходит
4. Обновить CHANGELOG.md

## 📞 Контакты

- **Проект**: MFB Proxy
- **Организация**: MFB
- **Репозиторий**: f:\project\bank\mfb-proxy

## 📄 Лицензия

Internal use only - MFB

---

**Последнее обновление**: 2025-10-30
**Версия**: 1.0.0


