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
- **SQL Server** (LocalDB for dev, Container for Docker)
- **Redis** (Caching)
- **YARP** (Reverse Proxy / API Gateway)
- **Swagger/OpenAPI**
- **Docker** & **Docker Compose** (Containerization with SQL Server + Redis)

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

### 4. Veritabanlarını Oluştur (LOCAL DEVELOPMENT)

**Package Manager Console'de şu komutları çalıştır:**

> 📌 **Note:** Bu adımlar LOCAL development için. Docker kullananlar için migration otomatik çalışır.

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
POST   /api/auth/assign-role       - Rol atama (Sadece Admin)
GET    /api/auth/secure-ping       - Token kontrol (Auth gerekli)
GET    /api/auth/ping              - Basit ping
```

### PRODUCT.API
```
POST   /api/product                - Ürün oluştur (ProductWritePolicy)
PUT    /api/product/{id}           - Ürün güncelle (ProductWritePolicy)
GET    /api/product                - Ürün listele (Redis cache)
GET    /api/product/cursor         - Cursor-based pagination (ayrı endpoint, Redis cache yok)
GET    /api/product/ping           - Basit ping
```

### LOG.API
```
POST   /api/logs                   - Log kaydı ekle
GET    /api/logs                   - Logları listele (LogsReadPolicy)
GET    /api/logs/ping              - Basit ping
```

### Authorization Özeti
```
Roller:
- Admin
- ProductManager
- User

Policy'ler:
- AdminOnlyPolicy    -> sadece Admin
- ProductWritePolicy -> Admin veya ProductManager
- LogsReadPolicy     -> sadece Admin

Notlar:
- Register olan kullanıcıya varsayılan User rolü atanır.
- Roller uygulama başlangıcında seed edilir.
- JWT access token içine role claim'leri eklenir.
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
GET  http://localhost:5109/product/api/product/cursor?limit=10
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
2. Yeni kullanıcının varsayılan rolü User olur
3. POST /auth/api/auth/login → Token al (accessToken)
4. GET /auth/api/auth/secure-ping + Token → Doğrula
5. Admin token ile POST /auth/api/auth/assign-role → ProductManager rolü ata
```

### 2. Product Test
```
1. ProductManager veya Admin token ile POST /product/api/product → Ürün oluştur
2. User token ile POST /product/api/product → 403 dönmeli
3. GET /product/api/product → Listele (DB'den)
4. GET /product/api/product (2. kez) → Listele (Redis cache'den)
5. ProductManager veya Admin token ile PUT /product/api/product/{id} → Güncelle
6. Cache invalidate olur, yeniden GET → Güncel veri
```

### 3. Log Test
```
1. Product'ta işlem yap (create/update)
2. Admin token ile GET /log/api/logs → Merkezi log kaydını kontrol et
3. User token ile GET /log/api/logs → 403 dönmeli
```

### 4. Gateway Routing Test
```
Gateway URL'leri (5109) üstünden yukarıdaki testleri tekrarla
Tüm request'ler doğru servisine yönlendirilmeli
```

---

## Konfigürasyon

### SQL Server Bağlantı Stringi

#### Local Development (appsettings.json)

Her servisin `appsettings.json`'ında LocalDB kullanılır:

```json
"ConnectionStrings": {
  "AuthDb": "Server=(localdb)\\mssqllocaldb;Database=KayraAuthDb;Trusted_Connection=true;TrustServerCertificate=true;"
}
```

Veritabanı adları:
- **Auth:** `KayraAuthDb`
- **Product:** `KayraProductDb`
- **Log:** `KayraLogDb`

#### Docker Environment (appsettings.Docker.json)

`ASPNETCORE_ENVIRONMENT=Docker` olduğunda SQL Server container kullanılır:

```json
"ConnectionStrings": {
  "AuthDb": "Server=sqlserver;Database=KayraAuthDb;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;"
}
```

**Fark:**
- Local: `(localdb)\\mssqllocaldb` + Windows Auth
- Docker: `sqlserver` container name + SQL Auth (sa user)

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

### Mimari Özet

Tüm servisler (Auth, Product, Log, Gateway), Redis ve SQL Server Docker containerlarında çalışabilir.

**✨ Yeni:** Veritabanlar artık SQL Server container'da çalışıyor (LocalDB yerine). Otomatik migration desteği.

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

# Output örneği:
# NAME                  STATUS          PORTS
# api-gateway          Up 2 minutes    0.0.0.0:5109->80/tcp
# auth-api             Up 2 minutes    0.0.0.0:5128->80/tcp
# product-api          Up 2 minutes    0.0.0.0:5164->80/tcp
# log-api              Up 2 minutes    0.0.0.0:5029->80/tcp
# redis                Up 2 minutes    0.0.0.0:6379->6379/tcp
# sqlserver            Up 2 minutes    0.0.0.0:1433->1433/tcp
```

#### Containerları Durdur:

```bash
docker-compose down

# Tüm volume'leri sil (clean state):
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
│   ├── → auth-api (container host: auth-api, port 80)
│   ├── → product-api (container host: product-api, port 80)
│   └── → log-api (container host: log-api, port 80)
├── redis (port 6379)
├── sqlserver (port 1433)
│   ├── KayraAuthDb
│   ├── KayraProductDb
│   └── KayraLogDb
```

### Service Discovery (Container Network)

Container'lar kendi aralarında container adı/host ile iletişim kurar:

- **SQL Server**: `Server=sqlserver;User Id=sa;Password=YourStrong!Passw0rd;`
- **Product → Log API**: `http://log-api:80` (appsettings.Docker.json)
- **Product → Redis**: `redis:6379`
- **Gateway → Backend Services**: auth-api, product-api, log-api (DNS names)

### Database Configuration (SQL Server Container)

#### appsettings.Docker.json Pattern

Her servis, `ASPNETCORE_ENVIRONMENT=Docker` olduğunda appsettings.Docker.json dosyasını yükler:

**Auth.API/appsettings.Docker.json:**
```json
{
  "ConnectionStrings": {
    "AuthDb": "Server=sqlserver;Database=KayraAuthDb;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;"
  },
  "Jwt": { ... },
  "AllowedHosts": "*"
}
```

**Product.API/appsettings.Docker.json:**
```json
{
  "ConnectionStrings": {
    "ProductDb": "Server=sqlserver;Database=KayraProductDb;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;"
  },
  "Redis": {
    "Connection": "redis:6379"
  },
  "Services": {
    "LogServiceBaseUrl": "http://log-api:80"
  },
  "Jwt": { ... },
  "AllowedHosts": "*"
}
```

**Log.API/appsettings.Docker.json:**
```json
{
  "ConnectionStrings": {
    "LogDb": "Server=sqlserver;Database=KayraLogDb;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;"
  },
  "Logging": { ... },
  "AllowedHosts": "*"
}
```

#### Automatic Database Migration

Her container başladıktan sonra **otomatik olarak** Database.Migrate() çalışır:

```csharp
// Program.cs içindeki kod
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
    dbContext.Database.Migrate();
}
```

Bu sayede:
✅ Migration'lar docker-compose up sırasında otomatik çalışır
✅ Veritabanı şemaları otomatik oluşturulur
✅ Manuel migration komutuna gerek yok

### Docker Compose Setup

**Servisler:**
1. **redis** - Cache layer (redis:7-alpine)
2. **sqlserver** - SQL Server container (Microsoft SQL Server)
3. **auth-api** - Authentication service
4. **product-api** - Product management service
5. **log-api** - Centralized logging service
6. **api-gateway** - YARP reverse proxy

**Veritabanlar** (SQL Server container'ında):
- `KayraAuthDb` - Auth tokens ve user data
- `KayraProductDb` - Product catalog
- `KayraLogDb` - Centralized logs

**Volume'ler:**
- `sqlserver_data` - SQL Server verisi persistent storage
- `redis_data` - Redis cache persistence (opsiyonel)

### Logs Kontrol

```bash
# Tüm container loglarını göster
docker-compose logs -f

# Spesifik servis logları
docker-compose logs -f api-gateway
docker-compose logs -f product-api
docker-compose logs -f sqlserver

# Son 100 satır (without follow)
docker-compose logs --tail=100 product-api
```

### Performance Notes

- **Build Süresi**: ~2-3 dakika (ilk build, network hızına bağlı)
- **Startup Süresi**: ~45-90 saniye (SQL Server + migration + health checks)
- **Memory**: ~4-5GB RAM (tüm containerlar)
- **Disk**: ~8-10GB (images + SQL Server data volume)

### Known Limitations

1. **SQL Server Container**: Developer edition kullanılıyor
2. **Migration Otomasyonu**: Program.cs'de Database.Migrate() otomatik çalışır
3. **SSL/HTTPS**: Docker ortamında HTTP kullanılıyor (HTTPS için sertifikat setup gerekli)
4. **Password**: appsettings.Docker.json içinde SA password (production'da environment variable kullan)

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

# SQL Server startup kontrol
docker-compose logs sqlserver
```

**Database Connection Error**
```bash
# SQL Server hazır mı kontrol et
docker exec kayra_sqlserver sqlcmd -S localhost -U sa -P YourStrong!Passw0rd -Q "SELECT @@VERSION"

# Database var mı kontrol et
docker exec kayra_sqlserver sqlcmd -S localhost -U sa -P YourStrong!Passw0rd -Q "SELECT DB_NAME() FROM sys.databases WHERE name='KayraProductDb'"
```

**SQL Server SA Password Reset**
```bash
# docker-compose.yml SA_PASSWORD environment variable bölümünü kontrol et
# docker-compose down -v ile baştan başla
docker-compose down -v
docker-compose up --build
```

---

## Continuous Integration (CI/CD)

### GitHub Actions Workflow

Repositoryyde otomatik doğrulama için GitHub Actions CI pipeline'ı kullanılmaktadır.

#### Workflow: `.github/workflows/ci.yml`

**Tetikleme:**
- ✅ Push events (test/v1.0.0, prod/v1.0.0, main)
- ✅ Pull Request events (test/v1.0.0, prod/v1.0.0, main)

**Adımlar:**
1. **Checkout** - Kod indirilir
2. **.NET 8 SDK Setup** - Runtime ve tools kurulur
3. **Restore** - NuGet bağımlılıkları yüklenir
4. **Build** - Release konfigürasyonunda proje build'lenir
5. **Test** - xUnit unit tests çalıştırılır (Product.Application.Tests)
6. **Docker Verification** - Tüm Dockerfile'lar build'lenir (registry push YOK)

#### Docker Doğrulaması

```bash
docker build -t kayra-auth-api:verify Auth.API/
docker build -t kayra-product-api:verify Product.API/
docker build -t kayra-log-api:verify Log.API/
docker build -t kayra-api-gateway:verify ApiGateway/
```

**Scope:**
- ✅ Dockerfile syntax doğruluğu
- ✅ .NET 8 SDK build başarısı
- ✅ Multi-stage build adımları
- ✅ Dependency resolution

**Dışında (yapılmayan):**
- ❌ Deployment yok
- ❌ Registry push yok (DockerHub vb.)
- ❌ Cloud environment deploy yok
- ❌ Kubernetes deploy yok
- ❌ SQL Server container image pull yok (build-only verification)

#### Workflow Durumunu Kontrol Et

GitHub repository sayfasında **"Actions"** sekmesinden recent workflow runs'ları görebilirsin.

```
Repo → Actions → ci.yml → Recent runs
```

Workflow başarılı ise tüm adımlar yeşil checkpoint gösterir:
- ✅ Checkout code
- ✅ Setup .NET 8 SDK
- ✅ Restore dependencies
- ✅ Build solution
- ✅ Run unit tests
- ✅ Verify Docker image builds

#### Local Test (CI Öncesi Doğrulama)

Workflow çalıştırılmadan önce local'inde test edebilirsin:

```bash
# Solution restore
dotnet restore KayraExportCase.slnx

# Build (Release)
dotnet build KayraExportCase.slnx --configuration Release

# Tests çalıştır
dotnet test KayraExportCase.slnx --configuration Release

# Docker image build'lerini doğrula
docker build -t kayra-auth-api:verify Auth.API/
docker build -t kayra-product-api:verify Product.API/
docker build -t kayra-log-api:verify Log.API/
docker build -t kayra-api-gateway:verify ApiGateway/
```

#### Workflow Failure Troubleshooting

**Build hatası**
```bash
# Local'de debug et
dotnet build KayraExportCase.slnx --configuration Release --verbosity diagnostic
```

**Test hatası**
```bash
# Test projesi çalıştır
dotnet test Product.Application.Tests --configuration Release -v normal
```

**Docker build hatası**
```bash
# Specific dockerfile debug
docker build --no-cache -t test Auth.API/
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
- Role-Based ve Policy-Based Authorization
- Onion Architecture
- Entity Framework Core Migrations
- **Docker Containerization** (SQL Server + Redis + API Services)
  - Multi-stage builds (SDK 8.0 → aspnet:8.0)
  - Automatic Database Migration (Program.cs)
  - SQL Server Container Integration
  - Container service discovery (sqlserver, redis, service-to-service)
  - appsettings.Docker.json configuration
- **Unit Tests** (xUnit + Moq, 13 tests)
- **Event Publishing** (ProductCreatedEvent)
  - Multi-branch trigger (push/PR on test/v1.0.0, prod/v1.0.0, main)
  - Build & test validation
 Auth mikroservisi (Register, Login, Refresh Token)
- Product CQRS (Create, Update, GetAll)
- Redis Cache + Cache Invalidation
- Cursor-Based Pagination (`GET /api/product/cursor`)
- Centralized Log Service

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
