# Changelog

Все заметные изменения в проекте MFB Proxy будут документированы в этом файле.

Формат основан на [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
и проект придерживается [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2025-10-30

### 🎉 Первый релиз

Начальная настройка проекта YARP Reverse Proxy для MFB.

### ✨ Added

#### Инфраструктура
- Создан solution `MfbProxy.sln`
- Добавлен проект `MfbProxy.Gateway` на базе ASP.NET Core Web API
- Установлен пакет `Yarp.ReverseProxy 2.3.0`
- Настроен `.gitignore` для .NET проектов

#### Функциональность
- **Базовое обратное проксирование**
  - Конфигурация routes через `appsettings.json`
  - Динамическая перезагрузка конфигурации
  
- **Балансировка нагрузки**
  - Round Robin для равномерного распределения
  - Weighted Round Robin для серверов разной мощности
  
- **CORS Support**
  - Настроен AllowAny policy для development
  
- **HTTPS Support**
  - HTTPS redirection включен
  - Настроены порты: HTTP (5000), HTTPS (5050)

#### Маршруты
- `/api/service1/{**catch-all}` → Single destination (localhost:5001)
- `/api/service2/{**catch-all}` → Round Robin (localhost:5002, 5003)
- `/api/weighted/{**catch-all}` → Weighted RR 70/30 (localhost:7001, 7002)

#### Документация
- **README.md** - Основная документация проекта
  - Описание технологий
  - Инструкции по конфигурации
  - Примеры расширенных возможностей
  - Архитектурная диаграмма
  
- **QUICKSTART.md** - Руководство быстрого старта
  - Пошаговая инструкция для начинающих
  - 5 типовых сценариев использования
  - Примеры тестирования
  - FAQ раздел
  
- **docs/configuration-examples.md** - Детальные примеры
  - 10 различных сценариев конфигурации
  - Примеры Health Checks, Rate Limiting, Session Affinity
  - Сравнительная таблица политик балансировки
  - Полезные команды
  
- **PROJECT.md** - Техническая информация
  - Структура проекта
  - Версии зависимостей
  - Roadmap
  - Рекомендации для production

- **CHANGELOG.md** - История изменений (этот файл)

### 🔧 Configuration

#### Program.cs
```csharp
- Настроен AddReverseProxy() с загрузкой из конфигурации
- Добавлен CORS middleware
- Настроен MapReverseProxy()
```

#### appsettings.json
```json
- Конфигурация 3 маршрутов
- Конфигурация 3 кластеров с разными политиками балансировки
- Weighted Round Robin с весами 70/30
```

#### launchSettings.json
```json
- HTTP profile: http://localhost:5000
- HTTPS profile: https://localhost:5050
- LaunchBrowser: false
```

### 📚 Examples Provided

1. Простое проксирование
2. Round Robin Load Balancing
3. Weighted Round Robin
4. Маршрутизация по HTTP методу
5. Маршрутизация по заголовкам
6. Health Checks
7. Request/Response Transformations
8. Session Affinity (Sticky Sessions)
9. Rate Limiting
10. Микросервисная архитектура

### 🎯 Target Audience

- Backend разработчики
- DevOps инженеры
- Архитекторы решений

---

## [Unreleased]

### 🔜 Планируется

#### Features
- [ ] Active Health Checks для автоматического failover
- [ ] Passive Health Checks для обнаружения проблем
- [ ] Rate Limiting на уровне Gateway
- [ ] Request/Response transformations
- [ ] JWT Authentication middleware
- [ ] API Key authentication
- [ ] Request logging middleware
- [ ] Response caching

#### Infrastructure
- [ ] Docker support (Dockerfile + docker-compose.yml)
- [ ] Kubernetes manifests (deployment, service, ingress)
- [ ] Helm chart для deployment
- [ ] CI/CD pipeline (GitHub Actions / Azure DevOps)

#### Monitoring
- [ ] Prometheus metrics endpoint
- [ ] Grafana dashboards
- [ ] OpenTelemetry distributed tracing
- [ ] Structured logging (Serilog)
- [ ] Health check endpoint для Gateway

#### Testing
- [ ] Unit tests с xUnit
- [ ] Integration tests с WebApplicationFactory
- [ ] Load tests с k6
- [ ] E2E tests

#### Documentation
- [ ] API documentation (Swagger/OpenAPI)
- [ ] Architecture Decision Records (ADR)
- [ ] Deployment guide
- [ ] Operations runbook
- [ ] Performance tuning guide

#### Security
- [ ] HTTPS certificate configuration guide
- [ ] OAuth 2.0 / OpenID Connect integration
- [ ] IP whitelisting
- [ ] DDoS protection strategies
- [ ] Security headers configuration

---

## Типы изменений

- **Added** - новые функции
- **Changed** - изменения в существующей функциональности
- **Deprecated** - функции, которые скоро будут удалены
- **Removed** - удаленные функции
- **Fixed** - исправления багов
- **Security** - исправления уязвимостей

---

**Версии**:
- `[Unreleased]` - Нерелизные изменения в разработке
- `[X.Y.Z]` - Релизная версия с датой

**Формат дат**: YYYY-MM-DD (ISO 8601)



