# CinemaAbyss Events Solution

Visual Studio Solution для микросервиса Events, включающий оба варианта реализации.

## Файл Solution

📁 **`CinemaAbyss.Events.sln`** - основной файл решения

## Включенные проекты

### 1. CinemaAbyss.Events (Основной - MassTransit)
**Путь:** `CinemaAbyss.Events/CinemaAbyss.Events.csproj`

**Описание:** Основная реализация микросервиса Events с использованием MassTransit.

**Зависимости:**
- MassTransit 8.5.5
- MassTransit.Kafka 8.5.5
- Swashbuckle.AspNetCore 9.0.6

**Порт:** 8082

### 2. CinemaAbyss.Events.Confluent (Пример - Confluent.Kafka)
**Путь:** `events-confluent-example/CinemaAbyss.Events.Confluent/CinemaAbyss.Events.Confluent.csproj`

**Описание:** Альтернативная реализация с использованием Confluent.Kafka для сравнения подходов.

**Зависимости:**
- Confluent.Kafka 2.12.0
- Swashbuckle.AspNetCore 9.0.6

**Порт:** 8083

## Открытие в IDE

### Visual Studio 2022
```
File → Open → Project/Solution → Выберите CinemaAbyss.Events.sln
```

### Visual Studio Code
```bash
cd src/microservices/events
code .
```

### JetBrains Rider
```bash
rider CinemaAbyss.Events.sln
```

## Команды dotnet CLI

### Сборка всего solution
```bash
cd src/microservices/events
dotnet build
```

### Сборка в Release режиме
```bash
dotnet build -c Release
```

### Запуск конкретного проекта
```bash
# MassTransit вариант
dotnet run --project CinemaAbyss.Events/CinemaAbyss.Events.csproj

# Confluent.Kafka вариант
dotnet run --project events-confluent-example/CinemaAbyss.Events.Confluent/CinemaAbyss.Events.Confluent.csproj
```

### Очистка
```bash
dotnet clean
```

### Восстановление зависимостей
```bash
dotnet restore
```

### Список проектов в solution
```bash
dotnet sln list
```

## Структура Solution

```
CinemaAbyss.Events.sln
│
├── CinemaAbyss.Events/                    # Основной проект (MassTransit)
│   ├── Models/
│   ├── Consumers/
│   ├── Controllers/
│   ├── Program.cs
│   ├── appsettings.json
│   ├── Dockerfile
│   └── CinemaAbyss.Events.csproj
│
└── events-confluent-example/              # Solution Folder
    └── CinemaAbyss.Events.Confluent/      # Пример (Confluent.Kafka)
        ├── Models/
        ├── Services/
        ├── Controllers/
        ├── Program.cs
        ├── appsettings.json
        ├── Dockerfile
        └── CinemaAbyss.Events.Confluent.csproj
```

## Конфигурации сборки

Solution настроен со следующими конфигурациями:

- **Debug|Any CPU** - для разработки и отладки
- **Release|Any CPU** - для production сборки
- **Debug|x64** - 64-битная отладочная сборка
- **Release|x64** - 64-битная production сборка
- **Debug|x86** - 32-битная отладочная сборка
- **Release|x86** - 32-битная production сборка

## Отладка

### Visual Studio
1. Откройте `CinemaAbyss.Events.sln`
2. Выберите стартовый проект: `CinemaAbyss.Events` (правый клик → Set as Startup Project)
3. Нажмите F5 или кнопку Start

### Visual Studio Code
1. Откройте папку `src/microservices/events`
2. Перейдите в Debug панель (Ctrl+Shift+D)
3. Выберите конфигурацию `.NET Core Launch (web)`
4. Нажмите F5

### Отладка с Docker Compose
```bash
# Из корня проекта
docker-compose up -d postgres kafka zookeeper

# Запустите проект локально для отладки
cd src/microservices/events
dotnet run --project CinemaAbyss.Events/CinemaAbyss.Events.csproj
```

## Тестирование

### Запуск тестов для всего solution
```bash
cd src/microservices/events
dotnet test
```

*Примечание: Unit/Integration тесты можно добавить позже в отдельные проекты.*

### Postman тесты (E2E)
```bash
cd tests/postman
node run-tests.js --environment=docker --folder="Events Microservice"
```

## Публикация

### Публикация основного проекта (MassTransit)
```bash
cd CinemaAbyss.Events
dotnet publish -c Release -o ./publish
```

### Публикация Confluent примера
```bash
cd events-confluent-example/CinemaAbyss.Events.Confluent
dotnet publish -c Release -o ./publish
```

## Docker Build

### Основной проект
```bash
# Из папки src/microservices/events
docker build -t cinemaabyss-events:latest -f Dockerfile .
```

### Confluent пример
```bash
# Из папки events-confluent-example/CinemaAbyss.Events.Confluent
docker build -t cinemaabyss-events-confluent:latest -f Dockerfile .
```

## Полезные команды Visual Studio

| Команда | Действие |
|---------|----------|
| `Ctrl+Shift+B` | Собрать решение |
| `F5` | Запустить с отладкой |
| `Ctrl+F5` | Запустить без отладки |
| `Shift+F5` | Остановить отладку |
| `F9` | Установить breakpoint |
| `F10` | Step Over |
| `F11` | Step Into |

## NuGet пакеты

### Обновление всех пакетов
```bash
dotnet list package --outdated
dotnet add package <PackageName> --version <NewVersion>
```

### Восстановление пакетов
```bash
dotnet restore
```

## CI/CD Integration

Solution готов для интеграции с CI/CD:

```yaml
# Пример для GitHub Actions
- name: Build Solution
  run: |
    cd src/microservices/events
    dotnet restore
    dotnet build --no-restore -c Release
    
- name: Run Tests
  run: |
    cd src/microservices/events
    dotnet test --no-build -c Release
```

## Troubleshooting

### "Проект не найден"
```bash
# Проверьте список проектов
dotnet sln list

# Добавьте проект заново
dotnet sln add path/to/project.csproj
```

### "Не удается восстановить пакеты"
```bash
# Очистите кэш NuGet
dotnet nuget locals all --clear

# Восстановите заново
dotnet restore --force
```

### "Конфликты версий пакетов"
```bash
# Проверьте версии
dotnet list package --include-transitive

# Обновите проблемный пакет
dotnet add package PackageName --version x.x.x
```

## Дополнительная документация

- [README.md](./README.md) - Основная документация MassTransit варианта
- [COMPARISON.md](./COMPARISON.md) - Сравнение MassTransit vs Confluent.Kafka
- [GETTING_STARTED.md](./GETTING_STARTED.md) - Быстрый старт и инструкции
- [events-confluent-example/README.md](./events-confluent-example/README.md) - Документация Confluent примера

## Версии и совместимость

- **.NET SDK:** 8.0 или выше
- **Visual Studio:** 2022 или выше
- **Visual Studio Code:** Последняя версия с C# extension
- **JetBrains Rider:** 2023.3 или выше

## Поддержка

При возникновении проблем:
1. Проверьте, что установлен .NET 8.0 SDK
2. Выполните `dotnet restore`
3. Выполните `dotnet clean` и затем `dotnet build`
4. Проверьте логи в Output окне Visual Studio

---

**Solution готов к использованию!** 🎉

Откройте `CinemaAbyss.Events.sln` в вашей любимой IDE и начинайте разработку!

