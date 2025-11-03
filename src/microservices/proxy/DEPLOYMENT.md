# Deployment Guide

Руководство по развертыванию MFB Proxy в различных окружениях.

## 📋 Содержание

- [Локальная разработка](#локальная-разработка)
- [Windows Server](#windows-server)
- [Linux Server](#linux-server)
- [Docker](#docker)
- [Kubernetes](#kubernetes)
- [Azure App Service](#azure-app-service)
- [IIS](#iis)

---

## 🖥️ Локальная разработка

### Требования
- .NET 8.0 SDK или новее
- Visual Studio 2022 / VS Code / Rider

### Запуск

```bash
# Клонировать проект
cd f:\project\bank\mfb-proxy

# Восстановить зависимости
dotnet restore

# Запустить в Development режиме
cd src/MfbProxy.Gateway
dotnet run
```

Gateway доступен на:
- HTTP: http://localhost:5000
- HTTPS: https://localhost:5050

---

## 🪟 Windows Server

### Требования
- Windows Server 2019/2022
- .NET 8.0 Runtime (ASP.NET Core)

### 1. Установка .NET Runtime

```powershell
# Скачать и установить .NET 8.0 Runtime
# https://dotnet.microsoft.com/download/dotnet/8.0

# Проверить установку
dotnet --list-runtimes
```

### 2. Сборка для Production

```powershell
# В корне проекта
dotnet publish src/MfbProxy.Gateway/MfbProxy.Gateway.csproj `
  -c Release `
  -o C:\publish\mfb-proxy `
  --self-contained false
```

### 3. Конфигурация

Отредактировать `C:\publish\mfb-proxy\appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning",
      "Yarp": "Information"
    }
  },
  "AllowedHosts": "*",
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
          "service1": {
            "Address": "https://your-backend-server.com/"
          }
        }
      }
    }
  }
}
```

### 4. Запуск как Windows Service

Установить службу с помощью `sc`:

```powershell
# Создать службу
sc.exe create MfbProxyGateway `
  binPath="C:\publish\mfb-proxy\MfbProxy.Gateway.exe" `
  DisplayName="MFB Proxy Gateway" `
  start=auto

# Запустить службу
sc.exe start MfbProxyGateway

# Проверить статус
sc.exe query MfbProxyGateway
```

Или использовать `NSSM` (Non-Sucking Service Manager):

```powershell
# Установить NSSM
# https://nssm.cc/download

# Установить службу
nssm install MfbProxyGateway "C:\publish\mfb-proxy\MfbProxy.Gateway.exe"

# Настроить порты через переменные окружения
nssm set MfbProxyGateway AppEnvironmentExtra ASPNETCORE_URLS=https://+:5050;http://+:5000

# Запустить
nssm start MfbProxyGateway
```

### 5. Настройка Firewall

```powershell
# Открыть порт 5050 (HTTPS)
New-NetFirewallRule -DisplayName "MFB Proxy HTTPS" `
  -Direction Inbound `
  -Protocol TCP `
  -LocalPort 5050 `
  -Action Allow

# Открыть порт 5000 (HTTP) - опционально
New-NetFirewallRule -DisplayName "MFB Proxy HTTP" `
  -Direction Inbound `
  -Protocol TCP `
  -LocalPort 5000 `
  -Action Allow
```

---

## 🐧 Linux Server

### Требования
- Ubuntu 20.04/22.04 или RHEL 8/9
- .NET 8.0 Runtime

### 1. Установка .NET Runtime

#### Ubuntu/Debian:
```bash
# Добавить репозиторий Microsoft
wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb

# Установить .NET Runtime
sudo apt-get update
sudo apt-get install -y aspnetcore-runtime-8.0
```

#### RHEL/CentOS:
```bash
# Добавить репозиторий Microsoft
sudo dnf install -y https://packages.microsoft.com/config/rhel/9/packages-microsoft-prod.rpm

# Установить .NET Runtime
sudo dnf install -y aspnetcore-runtime-8.0
```

### 2. Сборка и публикация

```bash
# На машине разработки
dotnet publish src/MfbProxy.Gateway/MfbProxy.Gateway.csproj \
  -c Release \
  -o ./publish \
  --self-contained false

# Скопировать на сервер
scp -r ./publish user@server:/opt/mfb-proxy
```

### 3. Создание systemd service

Создать файл `/etc/systemd/system/mfb-proxy.service`:

```ini
[Unit]
Description=MFB Proxy Gateway
After=network.target

[Service]
Type=notify
WorkingDirectory=/opt/mfb-proxy
ExecStart=/usr/bin/dotnet /opt/mfb-proxy/MfbProxy.Gateway.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=mfb-proxy
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=https://+:5050;http://+:5000
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false

[Install]
WantedBy=multi-user.target
```

### 4. Запуск службы

```bash
# Перезагрузить systemd
sudo systemctl daemon-reload

# Включить автозапуск
sudo systemctl enable mfb-proxy

# Запустить службу
sudo systemctl start mfb-proxy

# Проверить статус
sudo systemctl status mfb-proxy

# Просмотр логов
sudo journalctl -u mfb-proxy -f
```

### 5. Настройка Nginx как reverse proxy (опционально)

Создать `/etc/nginx/sites-available/mfb-proxy`:

```nginx
server {
    listen 80;
    server_name gateway.example.com;
    
    location / {
        proxy_pass https://localhost:5050;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

```bash
# Активировать конфигурацию
sudo ln -s /etc/nginx/sites-available/mfb-proxy /etc/nginx/sites-enabled/
sudo nginx -t
sudo systemctl reload nginx
```

---

## 🐳 Docker

### Dockerfile

Создать `Dockerfile` в корне проекта:

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/MfbProxy.Gateway/MfbProxy.Gateway.csproj", "MfbProxy.Gateway/"]
RUN dotnet restore "MfbProxy.Gateway/MfbProxy.Gateway.csproj"
COPY src/MfbProxy.Gateway/ MfbProxy.Gateway/
WORKDIR "/src/MfbProxy.Gateway"
RUN dotnet build "MfbProxy.Gateway.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MfbProxy.Gateway.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MfbProxy.Gateway.dll"]
```

### Docker Compose

Создать `docker-compose.yml`:

```yaml
version: '3.8'

services:
  gateway:
    build: .
    container_name: mfb-proxy-gateway
    ports:
      - "5050:443"
      - "5000:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ASPNETCORE_URLS=https://+:443;http://+:80
      - ASPNETCORE_Kestrel__Certificates__Default__Path=/https/aspnetapp.pfx
      - ASPNETCORE_Kestrel__Certificates__Default__Password=YourPassword
    volumes:
      - ./appsettings.Production.json:/app/appsettings.json:ro
      - ./certs:/https:ro
    restart: unless-stopped
    networks:
      - mfb-network

networks:
  mfb-network:
    driver: bridge
```

### Сборка и запуск

```bash
# Сборка образа
docker build -t mfb-proxy:latest .

# Запуск контейнера
docker run -d \
  --name mfb-proxy \
  -p 5050:443 \
  -p 5000:80 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  mfb-proxy:latest

# Или через docker-compose
docker-compose up -d

# Просмотр логов
docker logs -f mfb-proxy

# Остановка
docker-compose down
```

---

## ☸️ Kubernetes

### Deployment

Создать `k8s/deployment.yaml`:

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: mfb-proxy-gateway
  namespace: default
spec:
  replicas: 3
  selector:
    matchLabels:
      app: mfb-proxy
  template:
    metadata:
      labels:
        app: mfb-proxy
    spec:
      containers:
      - name: gateway
        image: your-registry/mfb-proxy:latest
        ports:
        - containerPort: 80
          name: http
        - containerPort: 443
          name: https
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: "Production"
        - name: ASPNETCORE_URLS
          value: "https://+:443;http://+:80"
        resources:
          requests:
            memory: "256Mi"
            cpu: "250m"
          limits:
            memory: "512Mi"
            cpu: "500m"
        livenessProbe:
          httpGet:
            path: /health
            port: 80
          initialDelaySeconds: 30
          periodSeconds: 10
        readinessProbe:
          httpGet:
            path: /health
            port: 80
          initialDelaySeconds: 5
          periodSeconds: 5
        volumeMounts:
        - name: config
          mountPath: /app/appsettings.json
          subPath: appsettings.json
          readOnly: true
      volumes:
      - name: config
        configMap:
          name: mfb-proxy-config
```

### Service

Создать `k8s/service.yaml`:

```yaml
apiVersion: v1
kind: Service
metadata:
  name: mfb-proxy-gateway
  namespace: default
spec:
  type: LoadBalancer
  selector:
    app: mfb-proxy
  ports:
  - name: http
    port: 80
    targetPort: 80
  - name: https
    port: 443
    targetPort: 443
```

### ConfigMap

Создать `k8s/configmap.yaml`:

```yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: mfb-proxy-config
  namespace: default
data:
  appsettings.json: |
    {
      "Logging": {
        "LogLevel": {
          "Default": "Information",
          "Microsoft.AspNetCore": "Warning"
        }
      },
      "AllowedHosts": "*",
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
              "service1": {
                "Address": "http://backend-service.default.svc.cluster.local/"
              }
            }
          }
        }
      }
    }
```

### Развертывание

```bash
# Применить конфигурацию
kubectl apply -f k8s/configmap.yaml
kubectl apply -f k8s/deployment.yaml
kubectl apply -f k8s/service.yaml

# Проверить статус
kubectl get pods -l app=mfb-proxy
kubectl get svc mfb-proxy-gateway

# Просмотр логов
kubectl logs -f deployment/mfb-proxy-gateway

# Масштабирование
kubectl scale deployment/mfb-proxy-gateway --replicas=5
```

---

## ☁️ Azure App Service

### Развертывание через Azure CLI

```bash
# Войти в Azure
az login

# Создать Resource Group
az group create --name mfb-proxy-rg --location eastus

# Создать App Service Plan
az appservice plan create \
  --name mfb-proxy-plan \
  --resource-group mfb-proxy-rg \
  --sku B1 \
  --is-linux

# Создать Web App
az webapp create \
  --name mfb-proxy-app \
  --resource-group mfb-proxy-rg \
  --plan mfb-proxy-plan \
  --runtime "DOTNETCORE:8.0"

# Настроить переменные окружения
az webapp config appsettings set \
  --name mfb-proxy-app \
  --resource-group mfb-proxy-rg \
  --settings ASPNETCORE_ENVIRONMENT=Production

# Развернуть из локальной папки
cd src/MfbProxy.Gateway
az webapp up \
  --name mfb-proxy-app \
  --resource-group mfb-proxy-rg
```

---

## 🌐 IIS

### Требования
- IIS 10+
- .NET 8.0 Hosting Bundle

### 1. Установка Hosting Bundle

```powershell
# Скачать и установить .NET Hosting Bundle
# https://dotnet.microsoft.com/download/dotnet/8.0

# Перезапустить IIS после установки
net stop was /y
net start w3svc
```

### 2. Публикация приложения

```powershell
dotnet publish src/MfbProxy.Gateway/MfbProxy.Gateway.csproj `
  -c Release `
  -o C:\inetpub\mfb-proxy
```

### 3. Создание Application Pool

```powershell
# Через PowerShell
Import-Module WebAdministration

New-WebAppPool -Name "MfbProxyAppPool"
Set-ItemProperty "IIS:\AppPools\MfbProxyAppPool" -Name "managedRuntimeVersion" -Value ""
Set-ItemProperty "IIS:\AppPools\MfbProxyAppPool" -Name "enable32BitAppOnWin64" -Value $false
```

### 4. Создание веб-сайта

```powershell
New-Website -Name "MfbProxy" `
  -PhysicalPath "C:\inetpub\mfb-proxy" `
  -ApplicationPool "MfbProxyAppPool" `
  -Port 5050 `
  -Ssl
```

---

## 🔒 SSL/TLS Certificates

### Для Development (самоподписанный сертификат)

```bash
# Создать сертификат
dotnet dev-certs https --trust
```

### Для Production (Let's Encrypt)

```bash
# Установить Certbot
sudo apt-get install certbot

# Получить сертификат
sudo certbot certonly --standalone -d gateway.example.com

# Сертификаты будут в /etc/letsencrypt/live/gateway.example.com/
```

### Настройка в appsettings.json

```json
{
  "Kestrel": {
    "Endpoints": {
      "Https": {
        "Url": "https://+:5050",
        "Certificate": {
          "Path": "/path/to/cert.pfx",
          "Password": "your-password"
        }
      }
    }
  }
}
```

---

## 📊 Мониторинг Production

### Health Checks

Добавить в `Program.cs`:

```csharp
builder.Services.AddHealthChecks();
app.MapHealthChecks("/health");
```

Проверка:
```bash
curl https://gateway.example.com/health
```

### Логирование

Использовать Serilog для структурированного логирования:

```bash
dotnet add package Serilog.AspNetCore
dotnet add package Serilog.Sinks.File
dotnet add package Serilog.Sinks.Console
```

---

## 🚨 Troubleshooting

### Gateway не запускается

```bash
# Проверить порты
netstat -an | findstr :5050

# Проверить логи
journalctl -u mfb-proxy -n 50
```

### Backend недоступен

```bash
# Проверить подключение
curl -k https://backend-server.com/

# Проверить DNS
nslookup backend-server.com
```

### Ошибки SSL

```bash
# Проверить сертификат
openssl s_client -connect localhost:5050 -showcerts
```

---

## 📝 Checklist перед Production

- [ ] Обновить `appsettings.json` с реальными backend адресами
- [ ] Настроить SSL/TLS сертификаты
- [ ] Ограничить CORS политику
- [ ] Добавить аутентификацию (если требуется)
- [ ] Настроить Health Checks
- [ ] Настроить логирование
- [ ] Настроить мониторинг
- [ ] Протестировать failover
- [ ] Настроить автоматические бэкапы конфигурации
- [ ] Документировать процедуры восстановления

---

**Дата обновления**: 2025-10-30


