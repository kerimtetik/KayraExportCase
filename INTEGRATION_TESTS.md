# KayraExportCase - Final Integration Test Senaryoları

## Ön Koşullar
1. Tüm 4 servis çalışıyor olmalı:
   - Auth.API (5128)
   - Product.API (5164)
   - Log.API (5029)
   - ApiGateway (5109)

2. Redis çalışıyor olmalı:
   ```bash
   docker ps | grep redis
   ```

3. Veritabanları migrate edilmiş olmalı

---

## TEST 1: API Gateway Routing (Ping Tests)

### 1.1: Gateway Ping
```bash
curl -X GET http://localhost:5109/ping
# Expected: {"data":"gateway pong"}
```

### 1.2: Auth Service via Gateway
```bash
curl -X GET http://localhost:5109/auth/ping
# Expected: "auth pong" (text)
```

### 1.3: Product Service via Gateway
```bash
curl -X GET http://localhost:5109/product/ping
# Expected: "product service is running" (text)
```

### 1.4: Log Service via Gateway
```bash
curl -X GET http://localhost:5109/log/ping
# Expected: "log service is running" (text)
```

---

## TEST 2: Auth Flow (Register, Login, Token, Refresh)

### 2.1: Register
```bash
curl -X POST http://localhost:5109/auth/api/auth/register \
  -H "Content-Type: application/json" \
  -d {
    "email": "testuser@example.com",
    "password": "Password123!"
  }
# Expected: 200 OK
# Save userId for later tests
```

### 2.2: Login
```bash
curl -X POST http://localhost:5109/auth/api/auth/login \
  -H "Content-Type: application/json" \
  -d {
    "email": "testuser@example.com",
    "password": "Password123!"
  }
# Expected: 200 OK
# Response:
# {
#   "accessToken": "eyJ..." (JWT token),
#   "refreshToken": "eyJ..." (Refresh token),
#   "expiresIn": 1800
# }
```

Save `accessToken` and `refreshToken` for upcoming tests!

### 2.3: Secure Ping with Token
```bash
# Use accessToken from Login response
curl -X GET http://localhost:5109/auth/api/auth/secure-ping \
  -H "Authorization: Bearer {accessToken}"
# Expected: "auth pong" (with token validation)
```

### 2.4: Refresh Token
```bash
# Use refreshToken from Login response
curl -X POST http://localhost:5109/auth/api/auth/refresh \
  -H "Content-Type: application/json" \
  -d {
    "refreshToken": "{refreshToken}"
  }
# Expected: 200 OK
# New accessToken + refreshToken returned
```

---

## TEST 3: Product Creation & Event Publishing

### 3.1: Create Product
```bash
curl -X POST http://localhost:5109/product/api/product \
  -H "Content-Type: application/json" \
  -d {
    "name": "Test Product 1",
    "price": 99.99,
    "stock": 50
  }
# Expected: 201 Created
# Response includes {id, name, price, stock, createdAtUtc}
# Save productId for PUT test
# Check Product.API console for: "Event published: ProductCreated | ProductId: ..."
# Check Log.API: GET /api/logs should show INFO level log
```

### 3.2: Verify Product Created Log
```bash
curl -X GET http://localhost:5109/log/api/logs
# Expected: 200 OK
# Response includes log entry with:
# {
#   "id": "...",
#   "serviceName": "Product.API",
#   "level": "INFO",
#   "message": "Product created. Id: ..., Name: Test Product 1",
#   "exception": null,
#   "createdAtUtc": "..."
# }
```

---

## TEST 4: Redis Cache Verification

### 4.1: First GET (Database Hit)
```bash
curl -X GET http://localhost:5109/product/api/product
# Expected: 200 OK
# Check Product.API console logs for cache behavior
# Should show: cache MISS → fetching from database
```

### 4.2: Second GET (Immediately After - Cache Hit)
```bash
# Run same command again immediately
curl -X GET http://localhost:5109/product/api/product
# Expected: 200 OK (same data)
# Check Product.API console logs
# Should show: cache HIT → fetching from Redis
# Response time should be faster than first call
```

---

## TEST 5: Product Update with JWT

### 5.1: Update Product (Valid ID)
```bash
# Use accessToken from Auth Login
curl -X PUT http://localhost:5109/product/api/product/{productId} \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {accessToken}" \
  -d {
    "name": "Updated Product",
    "price": 149.99,
    "stock": 100
  }
# Expected: 200 OK
# Response includes updated data
# Check Log.API: GET /api/logs should show new INFO level log
# Cache should be invalidated (next GET will be database hit)
```

### 5.2: Verify Update Log
```bash
curl -X GET http://localhost:5109/log/api/logs
# Expected: New log entry with:
# {
#   "level": "INFO",
#   "message": "Product updated. Id: ..., Name: Updated Product",
#   "serviceName": "Product.API"
# }
```

### 5.3: Verify Cache Invalidation
```bash
# After update, next GET should hit database (and repopulate cache)
curl -X GET http://localhost:5109/product/api/product
# Check console for: cache MISS (after invalidation)
```

---

## TEST 6: Product Update Failure (Not Found) - WARNING Log

### 6.1: Update Non-Existent Product
```bash
curl -X PUT http://localhost:5109/product/api/product/00000000-0000-0000-0000-000000000000 \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {accessToken}" \
  -d {
    "name": "Not Found",
    "price": 50,
    "stock": 10
  }
# Expected: 404 or null response (depends on controller implementation)
# Check Log.API: GET /api/logs should show WARNING level log
```

### 6.2: Verify WARNING Log
```bash
curl -X GET http://localhost:5109/log/api/logs
# Expected: Log entry with:
# {
#   "level": "WARNING",
#   "message": "Product update failed. Product not found. Id: 00000000-0000-0000-0000-000000000000",
#   "serviceName": "Product.API"
# }
```

---

## TEST 7: Log Levels Verification

### 7.1: Get All Logs
```bash
curl -X GET http://localhost:5109/log/api/logs
# Expected: Logs ordered by CreatedAtUtc DESC
# Check for:
#   - "level": "INFO" (creates, successful updates)
#   - "level": "WARNING" (failed updates)
```

### 7.2: Filter by Service
```bash
# Manually inspect response for serviceName: "Product.API"
curl -X GET http://localhost:5109/log/api/logs | jq '.[] | select(.serviceName=="Product.API")'
# Expected: Only Product.API logs
```

---

## TEST 8: Rate Limiting

### 8.1: Trigger Rate Limit
```bash
# Send 6 requests rapidly to same endpoint (limit is 5 per 10 seconds)
for i in {1..6}; do
  curl -X GET http://localhost:5109/product/ping
  echo "Request $i"
done

# Expected:
# Requests 1-5: 200 OK
# Request 6: 429 Too Many Requests
```

### 8.2: Wait and Verify Recovery
```bash
# Wait 11 seconds
sleep 11

curl -X GET http://localhost:5109/product/ping
# Expected: 200 OK (rate limit window reset)
```

---

## TEST 9: Event Publisher Logger Output

### 9.1: Check Console Output
When creating a Product, check **Product.API console** for:

```
[Information] Event published: ProductCreated | ProductId: {guid}, Name: {product_name}, Price: {price}, Stock: {stock}, CreatedAtUtc: {timestamp}
```

This confirms:
- ✅ Event publisher is called
- ✅ Structured logging works
- ✅ Event details are logged

---

## TEST 10: Comprehensive Flow Test

```bash
# Complete end-to-end test:

# 1. Register
curl -X POST http://localhost:5109/auth/api/auth/register ...

# 2. Login (get token)
curl -X POST http://localhost:5109/auth/api/auth/login ...

# 3. Create Product
curl -X POST http://localhost:5109/product/api/product ...

# 4. Get Products (cache miss)
curl -X GET http://localhost:5109/product/api/product

# 5. Get Products (cache hit)
curl -X GET http://localhost:5109/product/api/product

# 6. Update Product (with token)
curl -X PUT http://localhost:5109/product/api/product/{id} ...

# 7. Get Products (cache miss - invalidated)
curl -X GET http://localhost:5109/product/api/product

# 8. Get Logs
curl -X GET http://localhost:5109/log/api/logs

# Expected: All operations complete successfully
# Expected logs:
#   - INFO: Product created
#   - INFO: Product updated
#   - All info/warning logs properly recorded
```

---

## Success Criteria

| Scenario | Success Criteria |
|----------|-----------------|
| **Gateway Routing** | All ping endpoints return 200 OK |
| **Auth Flow** | Login returns valid JWT tokens |
| **Token Validation** | Secure-ping works with token |
| **Product Create** | Event published to logs |
| **Cache Hit/Miss** | MySQL logs show proper cache behavior |
| **Product Update** | Cache invalidates after update |
| **Update Not Found** | WARNING log created |
| **Log Levels** | INFO for success, WARNING for failure |
| **Rate Limiting** | 6th request per 10s returns 429 |
| **Event Logging** | Console shows event published message |

---

## Troubleshooting

| Problem | Solution |
|---------|----------|
| 502 Bad Gateway | Check backend service ports match gateway config |
| 404 Not Found | Verify route paths (check gateway appsettings.json) |
| Connection Refused | Verify Redis running: `docker ps \| grep redis` |
| Token Expired | JWT access token is 30 minutes, refresh if needed |
| Cache Not Working | Check Redis connection in logs |
| Log Service Error | Verify LogServiceBaseUrl in Product.API appsettings.json |

---

## Notes

- All tests use **HTTP** (not HTTPS) for development
- Redis should be running: `docker run -d --name kayra-redis -p 6379:6379 redis:7-alpine`
- Database migrations must be applied beforehand
- JWT tokens expire in 30 minutes (access token)
- Refresh token valid for 7 days
