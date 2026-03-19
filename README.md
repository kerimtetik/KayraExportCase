# KayraExportCase - Microservices Architecture

Üç mikroservisten (Auth, Product, Log) ve API Gateway katmanından oluşan .NET 8.0 tabanlı bir proje.

## Mimari

```
┌─────────────────────────────────────────┐
│         API Gateway (YARP)              │
│         http://localhost:5109           │
└────────────┬────────────┬────────────────┘
             │            │
    ┌────────▼──┐  ┌──────▼────┐  ┌─────────────┐
    │  Auth     │  │  Product  │  │    Log      │
    │  (CQRS)   │  │  (CQRS)   │  │ (Logging)   │
    │ 5128      │  │   5164    │  │    5029     │
    └───────────┘  └───────────┘  └─────────────┘
```

## Teknolojiler

- **.NET 8.0** / C#
- **ASP.NET Core Web API**
- **Entity Framework Core** (ORM)
- **ASP.NET Core Identity** (Auth)
- **SQL Server LocalDB**
- **Redis** (Caching)
- **YARP** (Reverse Proxy / API Gateway)
- **Swagger/OpenAPI**
- **Docker** & **Docker Compose** (Containerization)

## Mimari Yaklaşımlar

- **Microservice Architecture**
- **Onion Architecture** (layered structure)
- **CQRS** (Command Query Responsibility Segregation)
- **JWT Authentication** + Refresh Token
- **Redis Cache**
- **Centralized Logging**
- **Rate Limiting**

---

## Kurulum

### 1. Repository Klonla
```bash
git clone https://github.com/kerimtetik/KayraExportCase.git
cd KayraExportCase
git checkout test/v1.0.0
```

### 2. Gerekli Araçlar
- Visual Studio 2022+
- .NET 8 SDK
- SQL Server (LocalDB veya Express)
- Docker (Redis için)

### 3. Redis Başlat
```bash
docker run -d --name kayra-redis -p 6379:6379 redis:7-alpine
```

### 4. Veritabanlarını Oluştur

**Package Manager Console'de şu komutları çalıştır:**

Auth veritabanı:
```powershell
Add-Migration InitialAuthSchema -Project Auth.Infrastructure -StartupProject Auth.API
Update-Database -Project Auth.Infrastructure -StartupProject Auth.API
```

Product veritabanı:
```powershell
Add-Migration InitialProductSchema -Project Product.Infrastructure -StartupProject Product.API
Update-Database -Project Product.Infrastructure -StartupProject Product.API
```

Log veritabanı:
```powershell
Add-Migration InitialLogSchema -Project Log.Infrastructure -StartupProject Log.API
Update-Database -Project Log.Infrastructure -StartupProject Log.API
```

### 5. Uygulamaları Çalıştır

Visual Studio'da Solution'a sağ tık → **Set Startup Projects** → **Multiple startup projects** seçip şunları Start olarak ayarla:
- Auth.API
- Product.API
- Log.API
- ApiGateway

Or command line:
```bash
dotnet run --project Auth.API
dotnet run --project Product.API
dotnet run --project Log.API
dotnet run --project ApiGateway
```

---

## Servis Portları

| Servis | HTTP | HTTPS |
|--------|------|-------|
| **API Gateway** | 5109 | 7205 |
| **Auth.API** | 5128 | 7057 |
| **Product.API** | 5164 | 7053 |
| **Log.API** | 5029 | 7177 |

> Development'te API Gateway HTTP portları (5128, 5164, 5029) kullanır.

---

## Veritabanı Şemaları

### Auth Database (KayraAuthDb)
```sql
-- ASP.NET Core Identity tables auto-generated

-- Özel tablo
RefreshTokens
├── Id (PK)
├── Token (string)
├── UserId (FK → AspNetUsers)
├── ExpiresAt (DateTime)
└── IsRevoked (bool)
```

### Product Database (KayraProductDb)
```sql
Products
├── Id (Guid, PK)
├── Name (varchar(200))
├── Price (decimal 18,2)
├── Stock (int)
├── CreatedAtUtc (DateTime)
└── UpdatedAtUtc (DateTime, nullable)
```

### Log Database (KayraLogDb)
```sql
Logs
├── Id (Guid, PK)
├── ServiceName (varchar(100))
├── Level (varchar(20)) -- INFO, WARNING, ERROR, CRITICAL
├── Message (varchar(1000))
├── Exception (text, nullable)
└── CreatedAtUtc (DateTime)
```

---

## API Endpoints

### Gateway Routing: `/prefix/* → Service`

```
/auth/*    → Auth.API (5128)
/product/* → Product.API (5164)
/log/*     → Log.API (5029)
```

### AUTH.API
```
POST   /api/auth/register          - Kullanıcı kayıt
POST   /api/auth/login             - Giriş (access + refresh token döner)
POST   /api/auth/refresh           - Token yenile
GET    /api/auth/secure-ping       - Token kontrol (Auth gerekli)
GET    /api/auth/ping              - Basit ping
```

### PRODUCT.API
```
POST   /api/product                - Ürün oluştur
PUT    /api/product/{id}           - Ürün güncelle (Auth gerekli)
GET    /api/product                - Ürün listele (Redis cache)
GET    /api/product/ping           - Basit ping
```

### LOG.API
```
POST   /api/logs                   - Log kaydı ekle
GET    /api/logs                   - Logları listele
GET    /api/logs/ping              - Basit ping
```

---

## Gateway Üzerinden Test URL'leri

### Ping Testleri
```
GET http://localhost:5109/ping
GET http://localhost:5109/auth/ping
GET http://localhost:5109/product/ping
GET http://localhost:5109/log/ping
```

### Auth Flow
```
POST http://localhost:5109/auth/api/auth/register
POST http://localhost:5109/auth/api/auth/login
POST http://localhost:5109/auth/api/auth/refresh
GET  http://localhost:5109/auth/api/auth/secure-ping (Token gerekli)
```

### Product
```
GET  http://localhost:5109/product/api/product
POST http://localhost:5109/product/api/product
PUT  http://localhost:5109/product/api/product/{id}
```

### Log
```
GET  http://localhost:5109/log/api/logs
POST http://localhost:5109/log/api/logs
```

---

## Test Akışı

### 1. Auth Test
```
1. POST /auth/api/auth/register → Kullanıcı oluştur
2. POST /auth/api/auth/login → Token al (accessToken)
3. GET /auth/api/auth/secure-ping + Token → Doğrula
```

### 2. Product Test
```
1. POST /product/api/product → Ürün oluştur
2. GET /product/api/product → Listele (DB'den)
3. GET /product/api/product (2. kez) → Listele (Redis cache'den)
4. PUT /product/api/product/{id} + Token → Güncelle
5. Cache invalidate olur, yeniden GET → Güncel veri
```

### 3. Log Test
```
1. Product'ta işlem yap (create/update)
2. GET /log/api/logs → Merkezi log kaydını kontrol et
```

### 4. Gateway Routing Test
```
Gateway URL'leri (5109) üstünden yukarıdaki testleri tekrarla
Tüm request'ler doğru servisine yönlendirilmeli
```

---

## Konfigürasyon

### SQL Server Bağlantı Stringi

Her servisin `appsettings.json`'ında:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=KayraAuthDb;Trusted_Connection=true;TrustServerCertificate=true;"
}
```

Veritabanı adları:
- **Auth:** `KayraAuthDb`
- **Product:** `KayraProductDb`
- **Log:** `KayraLogDb`

### JWT Konfigürasyonu

```json
"Jwt": {
  "SecretKey": "your-super-secret-key-min-32-chars-long",
  "Issuer": "KayraExportCase",
  "Audience": "KayraExportCaseAPI",
  "AccessTokenExpirationMinutes": 30,
  "RefreshTokenExpirationDays": 7
}
```

### Redis Konfigürasyonu

```json
"Redis": {
  "Host": "localhost",
  "Port": 6379,
  "Database": 0
}
```

### Rate Limiting

API Gateway: 5 request / 10 saniye (IP başına)

---

## Docker Containerization

### Docker ile Çalıştırma

Tüm servisler (Auth, Product, Log, Gateway, Redis, SQL Server) Docker containerlarında çalışabilir.

#### Ön Koşullar:
- Docker Desktop yüklü ve çalışıyor
- Docker Compose v3.8+
- Minimum 4GB RAM ve 20GB disk alanı (SQL Server için)

#### Başlatma:

```bash
# Root dizinden komut çalıştır
docker-compose up --build

# Arka planda çalıştır:
docker-compose up --build -d
```

#### Container Durumunu Kontrol Et:

```bash
docker-compose ps

# Output:
# NAME                    STATUS          PORTS
# kayra-api-gateway       Up 2 minutes    0.0.0.0:5109->80/tcp
# kayra-auth-api          Up 2 minutes    0.0.0.0:5128->80/tcp
# kayra-product-api       Up 2 minutes    0.0.0.0:5164->80/tcp
# kayra-log-api           Up 2 minutes    0.0.0.0:5029->80/tcp
# kayra-redis             Up 2 minutes    0.0.0.0:6379->6379/tcp
# kayra-auth-db           Up 2 minutes    0.0.0.0:11433->1433/tcp
# kayra-product-db        Up 2 minutes    0.0.0.0:21433->1433/tcp
# kayra-log-db            Up 2 minutes    0.0.0.0:31433->1433/tcp
```

#### Containerları Durdur:

```bash
docker-compose down

# Tüm volume'leri sil:
docker-compose down -v
```

### Docker Ortamında API Erişimi

Gateway başarıyla başladıktan sonra aynı curl/Postman komutlarını kullanabilirsiniz:

```bash
# Gateway ping
curl http://localhost:5109/ping

# Auth register
curl -X POST http://localhost:5109/auth/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"email":"test@test.com","password":"Pass123!"}'

# Product oluştur
curl -X POST http://localhost:5109/product/api/product \
  -H "Content-Type: application/json" \
  -d '{"name":"Test Product","price":99.99,"stock":50}'

# Logları listele
curl http://localhost:5109/log/api/logs
```

### Docker Mimarisi

```
Docker Network: kayra-network
├── api-gateway (port 5109)
│   ├── → auth-api (container name: kayra-auth-api, port 5128)
│   ├── → product-api (container name: kayra-product-api, port 5164)
│   └── → log-api (container name: kayra-log-api, port 5029)
├── redis (port 6379)
│
⚠️  Veritabanları: LocalDB (host.docker.internal ile container'lardan erişilebilir)
    - KayraAuthDb
    - KayraProductDb
    - KayraLogDb
```

### Service Discovery (Container Network)

Container'lar kendi aralarında container adı ile iletişim kurar:

- **Product → Log**: `http://log-api:80` (`appsettings.Docker.json`)
- **Product → Redis**: `redis:6379`
- **Gateway → Backend Services**: Container isimleri (auth-api, product-api, log-api)

### Docker Compose Setup

**Servisler:**
1. **redis** - Cache layer (redis:7-alpine)
2. **auth-api** - Authentication service
3. **product-api** - Product management service
4. **log-api** - Centralized logging service
5. **api-gateway** - YARP reverse proxy

**Veritabanı Notası:**

Containerlar SQL Server yerine LocalDB kullanıyor. Veritabanlar host machine'de LocalDB'de yer alıyor.

```bash
# Container başlamadan ÖNCE veritabanları migrate etmelisin:
Add-Migration InitialAuthSchema -Project Auth.Infrastructure -StartupProject Auth.API
Update-Database -Project Auth.Infrastructure -StartupProject Auth.API
# ... (Product ve Log için de)
```

Container'lar başladıktan sonra, appsettings.Docker.json aracılığıyla `host.docker.internal` üzerinden LocalDB'ye bağlanırlar.

### Database Setup (LocalDB with Docker Containers)

**ÖNEMLI:** Container'lar başlamadan ÖNCE hassas veritabanı migration'larını çalıştırmalısınız:

```bash
# Host machine'de (Docker başlamadan):

# Auth DB
Add-Migration InitialAuthSchema -Project Auth.Infrastructure -StartupProject Auth.API
Update-Database -Project Auth.Infrastructure -StartupProject Auth.API

# Product DB
Add-Migration InitialProductSchema -Project Product.Infrastructure -StartupProject Product.API
Update-Database -Project Product.Infrastructure -StartupProject Product.API

# Log DB
Add-Migration InitialLogSchema -Project Log.Infrastructure -StartupProject Log.API
Update-Database -Project Log.Infrastructure -StartupProject Log.API
```

**Nasıl çalışır:**
- Veritabanları host machine'de LocalDB'de yaşıyor
- Container'lar `System.Data.SqlClient` Trusted Connection kullanıyor
- Docker Desktop'ın `host.docker.internal` special hostname'i üzerinden LocalDB'ye erişim sağlanıyor
- Windows Authentication (Trusted_Connection=True) kullanılıyor

**Avantajlar:**
- Hızlı startup (SQL Server container gerekmiyor)
- Development esnekliği (VS'de dbsını kontrol edebilirsin)
- Düşük resource kullanım

**Dezavantajlar:**
- Yalnızca Windows'ta çalışır (Linux/Mac'te docker-compose.override.yml ile SQL Server eklemek gerekli)

### Environment Variables (Docker Compose)

`docker-compose.yml` içinde her servis için environment değişkenleri tanımlanmıştır:

```yaml
environment:
  - ASPNETCORE_ENVIRONMENT=Docker
  - ConnectionStrings__AuthDb=Server=auth-db,1433;...
  - Redis__Connection=redis:6379
  - Services__LogServiceBaseUrl=http://log-api
```

Yerel değişiklikler için `docker-compose.override.yml` oluşturabilirsiniz:

```yaml
version: '3.8'
services:
  product-api:
    environment:
      - Services__LogServiceBaseUrl=http://custom-log-service
```

### Logs Kontrol

```bash
# Tüm container loglarını göster
docker-compose logs -f

# Spesifik servis logları
docker-compose logs -f api-gateway
docker-compose logs -f product-api

# Son 100 satır (without follow)
docker-compose logs --tail=100 product-api
```

### Performance Notes

- **Build Süresi**: ~2-3 dakika (ilk build, network hızına bağlı)
- **Startup Süresi**: ~30-60 saniye (SQL Server health checks)
- **Memory**: ~3-4GB RAM (tüm container'lar)
- **Disk**: ~5-6GB (image'lar + data volumes)

### Known Limitations

1. **SQL Server Licensing**: Developer edition kullanılıyor (production değil)
2. **LocalDB ile Dev**: Local development için LocalDB + Redis daha hafif
3. **Database Migrations**: Container'da manuel migration gerekli
4. **SSL/HTTPS**: Docker ortamında HTTP kullanılıyor (HTTPS için sertifikat setup gerekli)

### Troubleshooting

**Port Already in Use**
```bash
# Port 5109'u kullanan process'i bul
lsof -i :5109  # Linux/Mac
netstat -ano | findstr :5109  # Windows
```

**Container Crash'i**
```bash
# Log detayları gör
docker-compose logs api-gateway

# Health check başarısız
docker-compose logs auth-db
```

**Database Connection Error**
```bash
# SQL Server hazır mı kontrol et
docker exec kayra-auth-db sqlcmd -S localhost -U sa -P KayraPassword123! -Q "SELECT @@VERSION"
```

---

## Özellikler

✅ **Tamamlanan:**
- Auth mikroservisi (Register, Login, Refresh Token)
- Product CQRS (Create, Update, GetAll)
- Redis Cache + Cache Invalidation
- Centralized Log Service
- API Gateway Routing
- Rate Limiting
- JWT Authentication
- Onion Architecture
- Entity Framework Core Migrations
- **Docker Containerization** (LocalDB + Redis + API Services)
- **Unit Tests** (xUnit + Moq, 13 tests)
- **Event Publishing** (ProductCreatedEvent)

⠀ **Ekstra Değerlendirme (Henüz Yapılmamış):**
- CI/CD Pipeline
- RabbitMQ / Kafka Event Bus
- SAGA Pattern
- Role-Based Authorization
- Advanced Structured Logging (Serilog + ELK)
- SQL Server Container (Alternative DB setup)

---

## Troubleshooting

### 502 Bad Gateway
**Çözüm:** Backend servislerinin çalıştığını kontrol et. Portlar doğru mu?
```bash
# Her servisi test et
curl http://localhost:5128/api/auth/ping
curl http://localhost:5164/api/product/ping
curl http://localhost:5029/api/logs/ping
```

### 404 Endpoint Not Found
**Çözüm:** Gateway route'ları ve prefix'leri kontrol et (`appsettings.json`).

### Connection Refused
**Çözüm:** Redis çalışıyor mu?
```bash
docker ps | grep redis
```

### Database Migration Error
**Çözüm:** SQL Server'ın açık olduğundan emin ol ve connection string'i kontrol et.

### SSL Certificate Error (HTTPS)
Development'te HTTP kullan. HTTPS portları self-signed certificate gerektirir.

---

## Branch Yapısı

- **test/v1.0.0** - Geliştirme branch'i (şu anki)
- **prod/v1.0.0** - Production branch'i

Commit mesajları:
```
feat: ...      (yeni özellik)
fix: ...       (hata düzeltme)
refactor: ...  (kod iyileştirme)
docs: ...      (dokümantasyon)
```

---

## Kaynaklar

- [.NET 8 Docs](https://learn.microsoft.com/en-us/dotnet/)
- [EF Core](https://learn.microsoft.com/en-us/ef/core/)
- [YARP Documentation](https://microsoft.github.io/reverse-proxy/)
- [Redis](https://redis.io/)

---

## Lisans

Private / Internal Use
