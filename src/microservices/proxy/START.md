# 🚀 Быстрый старт MFB Proxy

## ✅ Проекты созданы и настроены

Все три проекта успешно созданы и адаптированы для **.NET 8.0**:

| Проект | Порт HTTPS | Порт HTTP | Статус |
|--------|------------|-----------|--------|
| **MfbProxy.Gateway** | 5050 | 5000 | ✅ Собран |
| **Service1** | 5001 | 5101 | ✅ Собран |
| **Service2** | 5002 | 5102 | ✅ Собран |

---

## 🏃 Запуск для тестирования

### Вариант 1: Запуск в 3 терминалах (рекомендуется)

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

### Вариант 2: Через Visual Studio / Rider

1. Открыть `MfbProxy.sln`
2. Щелкнуть правой кнопкой на Solution → **Set Startup Projects**
3. Выбрать **Multiple startup projects**
4. Установить для всех трех проектов **Action: Start**
5. Нажать **F5**

---

## 🧪 Проверка работы

После запуска всех трех проектов:

### 1. Проверить сервисы напрямую

```bash
# Service1
curl https://localhost:5001/health

# Service2  
curl https://localhost:5002/health
```

### 2. Проверить работу через Gateway (YARP)

```bash
# Через прокси к Service1
curl https://localhost:5050/api/service1/test

# Через прокси к Service2
curl https://localhost:5050/api/service2/test

# Weather forecast через прокси
curl https://localhost:5050/api/service1/weatherforecast
```

**Ожидаемый результат:** Вы увидите ответ от Service1 или Service2 с информацией о сервисе, порте и времени.

---

## 📊 Доступные endpoints

### Service1 (https://localhost:5001)
- `/health` - Health check
- `/weatherforecast` - Прогноз погоды
- `/api/test` - Тестовый endpoint

### Service2 (https://localhost:5002)
- `/health` - Health check
- `/weatherforecast` - Прогноз погоды
- `/api/test` - Тестовый endpoint

### Gateway (https://localhost:5050)
- `/api/service1/*` → Service1
- `/api/service2/*` → Service2
- `/api/weighted/*` → Weighted балансировка (70% Service1, 30% Service2)

---

## 🔧 Настройка маршрутов

Отредактируйте `src/MfbProxy.Gateway/appsettings.json` для изменения:
- Портов backend-сервисов
- Путей маршрутов
- Политик балансировки
- Весов для Weighted Round Robin

**Важно:** Gateway автоматически перезагружает конфигурацию при изменении `appsettings.json`!

---

## 📚 Документация

| Файл | Описание |
|------|----------|
| [README.md](README.md) | Основная документация проекта |
| [QUICKSTART.md](QUICKSTART.md) | Подробный quick start guide |
| [TESTING.md](TESTING.md) | Руководство по тестированию |
| [docs/custom-load-balancing.md](docs/custom-load-balancing.md) | Кастомные политики балансировки |
| [DEPLOYMENT.md](DEPLOYMENT.md) | Deployment на 7 платформах |
| [PROJECT.md](PROJECT.md) | Техническая информация |
| [SUMMARY.md](SUMMARY.md) | Полное резюме проекта |

---

## ⚡ Команды для разработки

```bash
# Сборка всех проектов
dotnet build

# Очистка
dotnet clean

# Сборка Release
dotnet build -c Release

# Запуск конкретного проекта
dotnet run --project src/MfbProxy.Gateway/MfbProxy.Gateway.csproj
```

---

## 🎯 Что дальше?

1. ✅ **Запустите сервисы** (см. выше)
2. ✅ **Протестируйте endpoints** (см. [TESTING.md](TESTING.md))
3. ✅ **Настройте маршруты** под свои backend-сервисы
4. ✅ **Добавьте Health Checks** (примеры в [configuration-examples.md](docs/configuration-examples.md))
5. ✅ **Настройте Rate Limiting** для production
6. ✅ **Разверните** на сервере (см. [DEPLOYMENT.md](DEPLOYMENT.md))

---

## 🐛 Troubleshooting

### Порт уже используется
```bash
# Windows
netstat -ano | findstr :5050
taskkill /PID <PID> /F

# Linux/Mac
lsof -i :5050
kill -9 <PID>
```

### Certificate errors
```bash
# Для curl
curl -k https://localhost:5050/api/service1/test

# Для PowerShell
Invoke-RestMethod -Uri "https://localhost:5050/api/service1/test" -SkipCertificateCheck
```

### Gateway не видит сервисы (502 Bad Gateway)
1. Убедитесь, что Service1 и Service2 запущены
2. Проверьте порты в `appsettings.json`
3. Проверьте логи Gateway

---

## 💡 Полезные советы

- 🔄 **Конфигурация перезагружается автоматически** - не нужно перезапускать Gateway
- 📊 **Логи в реальном времени** - смотрите на output всех трех терминалов
- 🧪 **Используйте Postman** для удобного тестирования
- 📝 **Читайте логи YARP** для отладки маршрутизации

---

**Готово! Успешного тестирования!** 🎉

Если нужна помощь, см. [TESTING.md](TESTING.md) для детального руководства по тестированию.

