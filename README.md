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

⠀ **Ekstra Değerlendirme (Henüz Yapılmamış):**
- Docker Containerization
- CI/CD Pipeline
- RabbitMQ / Kafka Event Bus
- SAGA Pattern
- Role-Based Authorization
- Advanced Structured Logging (Serilog + ELK)

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
