# 📘 Shopizy Operations Runbook

Comprehensive operational, local development, deployment, disaster recovery, incident response, and troubleshooting guide for the **Shopizy Microservice Cloud-Native Platform**.

---

## 📑 Table of Contents
1. [System Overview & Architecture](#1-system-overview--architecture)
2. [Local Development & Inner Loop](#2-local-development--inner-loop)
3. [Environment Configuration & Secrets](#3-environment-configuration--secrets)
4. [Database & State Management](#4-database--state-management)
5. [Service Port & Endpoint Directory](#5-service-port--endpoint-directory)
6. [Testing & Quality Verification](#6-testing--quality-verification)
7. [Observability & Health Probes](#7-observability--health-probes)
8. [Common Operational Tasks](#8-common-operational-tasks)
9. [Incident Response & Troubleshooting Playbooks](#9-incident-response--troubleshooting-playbooks)
10. [Disaster Recovery & Data Resilience](#10-disaster-recovery--data-resilience)

---

## 1. System Overview & Architecture

Shopizy is a high-throughput, distributed e-commerce microservices platform built on **.NET 10 / C# 14**, containerized via **Docker**, and orchestrated locally and in cloud environments via **.NET Aspire 10**.

### Core Architecture Highlights
- **Clean Architecture + DDD**: Strict separation of Domain, Application, Infrastructure, and Presentation layers.
- **Microservices Boundary**: 13 business services and background workers operating on a **Database-per-Service** pattern (Principle VII).
- **Asynchronous Decoupling**: Inter-service events driven by RabbitMQ and MassTransit via the Transactional Outbox pattern.
- **Zero-Trust Customer Isolation**: Cryptographic JWT Bearer tokens and repository-level scoping (Principle V).
- **High-Concurrency Inventory**: Atomic reservation with 15-minute expiration windows (Principle II & VIII).

---

## 2. Local Development & Inner Loop

### 2.1 Prerequisites
- **.NET 10 SDK** (v10.0.100 or later)
- **Docker Desktop** (or Podman) for containerized infrastructure
- **PowerShell 7+** / bash
- **Git** & **GitHub CLI (`gh`)**

### 2.2 Bootstrapping the Solution
```bash
# Clone repository
git clone https://github.com/akazad13/shopizy-microservice.git
cd shopizy-microservice

# Restore NuGet packages
dotnet restore Shopizy.sln

# Build entire solution with zero tolerance for warnings
dotnet build Shopizy.sln --warnaserror
```

### 2.3 Running via .NET Aspire Orchestrator
To spin up all infrastructure dependencies (PostgreSQL 17, Redis 7, RabbitMQ) and all 13 services concurrently with live telemetry:

```bash
dotnet run --project src/Shopizy.AppHost
```

- **Aspire Dashboard**: Accessible at `https://localhost:15888` (or port printed in terminal console).
- Provides live logs, distributed trace waterfall charts, metric graphs, and container health.

---

## 3. Environment Configuration & Secrets

### 3.1 Standard Configuration Keys
All microservices consume configuration via `appsettings.json`, environment variables, and Aspire service discovery.

| Variable / Key | Purpose | Default / Example |
|:---|:---|:---|
| `ConnectionStrings:{servicename}db` | PostgreSQL connection string | `Host=localhost;Database={name}db;Username=postgres;Password=...` |
| `ConnectionStrings:redis` | Redis cache & session connection string | `localhost:6379` |
| `ConnectionStrings:rabbitmq` | RabbitMQ event broker connection string | `amqp://guest:guest@localhost:5672` |
| `Jwt:Key` | Symmetric JWT signing key (HMAC-SHA256) | Must be $\ge 256$ bits (32+ chars) |
| `Jwt:Issuer` | Valid JWT Issuer | `Shopizy` |
| `Jwt:Audience` | Valid JWT Audience | `ShopizyClient` |

### 3.2 Secret Management
- **Local Dev**: Use .NET User Secrets:
  ```bash
  dotnet user-secrets set "Jwt:Key" "<secure-random-key>" --project src/Shopizy.IdentityService
  ```
- **CI / GitHub Actions**: Repository secret `GEMINI_API_KEY` for autonomous PR review agent.
- **Production**: Azure Key Vault, AWS Secrets Manager, or HashiCorp Vault mapped via Aspire configuration providers.

---

## 4. Database & State Management

### 4.1 Database-per-Service Mapping
Each microservice maintains its own schema/database. Direct cross-database access is prohibited.

| Microservice | Database Name | Default Storage Engine |
|:---|:---|:---|
| `Shopizy.IdentityService` | `identitydb` | PostgreSQL 17 (EF Core) |
| `Shopizy.CatalogService` | `catalogdb` | PostgreSQL 17 (EF Core) |
| `Shopizy.CartService` | `cartdb` / Redis | Redis 7 (Key-value + TTL) |
| `Shopizy.OrderService` | `orderdb` | PostgreSQL 17 (EF Core) |
| `Shopizy.PaymentService` | `paymentdb` | PostgreSQL 17 (EF Core) |
| `Shopizy.SearchService` | In-memory / Meilisearch | Vector & Inverted Search Index |
| `Shopizy.PromotionService` | `promotiondb` | PostgreSQL 17 (EF Core) |
| `Shopizy.ShippingService` | `shippingdb` | PostgreSQL 17 (EF Core) |
| `Shopizy.NotificationService`| `notificationdb` | PostgreSQL 17 (EF Core) |
| `Shopizy.ReviewService` | `reviewdb` | PostgreSQL 17 (EF Core) |
| `Shopizy.LoyaltyService` | `loyaltydb` | PostgreSQL 17 (EF Core) |
| `Shopizy.CartAbandonmentWorker`| `abandonmentdb`| PostgreSQL 17 (EF Core) |

### 4.2 Applying EF Core Migrations
To generate or apply migrations for any relational service:
```bash
# Example: Adding migration to OrderService
dotnet ef migrations add AddOrderFulfillmentFields \
  --project src/Shopizy.OrderService \
  --startup-project src/Shopizy.OrderService

# Applying migration to database
dotnet ef database update \
  --project src/Shopizy.OrderService \
  --startup-project src/Shopizy.OrderService
```

---

## 5. Service Port & Endpoint Directory

When launched via `Shopizy.AppHost`, dynamic reverse-proxy and service discovery ports are assigned. In standalone execution, services use the following conventions:

| Service | Primary Endpoints | Auth Required |
|:---|:---|:---:|
| **Identity** | `POST /api/v1/identity/register`<br>`POST /api/v1/identity/login`<br>`POST /api/v1/identity/refresh`<br>`GET /api/v1/identity/me` | No (auth for `/me`) |
| **Catalog** | `GET /api/v1/catalog/products`<br>`GET /api/v1/catalog/categories`<br>`POST /api/v1/catalog/products` | Read: No<br>Write: `StoreAdmin` |
| **Cart** | `GET /api/v1/cart/{cartId}`<br>`POST /api/v1/cart/items`<br>`POST /api/v1/cart/merge` | Optional / Token |
| **Order** | `POST /api/v1/orders`<br>`GET /api/v1/orders/{orderId}`<br>`POST /api/v1/orders/{orderId}/cancel` | `Customer` / `StoreAdmin` |
| **Payment** | `POST /api/v1/payments/process`<br>`POST /api/v1/payments/{id}/refund` | `Customer` / `StoreAdmin` |
| **Search** | `GET /api/v1/search?q={term}&category={cat}`<br>`POST /api/v1/search/index` | Query: Public<br>Index: `StoreAdmin` |
| **Promotion**| `POST /api/v1/promotions/validate`<br>`POST /api/v1/promotions` | Validate: Public<br>Create: `StoreAdmin` |
| **Shipping** | `POST /api/v1/shipping/rates`<br>`GET /api/v1/shipping/track/{trackingNumber}` | Public / Customer |
| **Notification**| `POST /api/v1/notifications/email`<br>`GET /api/v1/notifications/my` | Public / Customer |
| **Review** | `POST /api/v1/reviews`<br>`GET /api/v1/reviews/product/{productId}`<br>`POST /api/v1/reviews/{id}/vote` | Submit: `Customer`<br>Read: Public |
| **Loyalty** | `POST /api/v1/loyalty/accrue`<br>`POST /api/v1/loyalty/redeem`<br>`GET /api/v1/loyalty/account` | Accrue: `StoreAdmin`<br>Redeem: `Customer` |
| **Abandonment**| `POST /api/v1/cart-abandonment/sweep`<br>`GET /api/v1/cart-abandonment/restore/{token}` | Sweep: `StoreAdmin`<br>Restore: Public |

---

## 6. Testing & Quality Verification

### 6.1 Running Full Test Suite
The solution contains **38 test projects** covering Unit, Integration, and Automated E2E tiers.

```bash
# Run all tests across the entire solution
dotnet test Shopizy.sln --logger "console;verbosity=normal"

# Expected outcome:
# Total Test Assemblies: 38
# Passed: 336, Failed: 0, Skipped: 0 (100% pass rate)
```

### 6.2 Running Targeted Test Suites
```bash
# Unit tests only for OrderService
dotnet test tests/Shopizy.OrderService.UnitTests/Shopizy.OrderService.UnitTests.csproj

# Integration tests for PaymentService
dotnet test tests/Shopizy.PaymentService.IntegrationTests/Shopizy.PaymentService.IntegrationTests.csproj

# E2E scenarios for CartAbandonmentWorker
dotnet test tests/Shopizy.CartAbandonmentWorker.E2ETests/Shopizy.CartAbandonmentWorker.E2ETests.csproj
```

### 6.3 Code Quality & Strict Compilation
```bash
# Enforce zero compiler warnings or lint issues
dotnet build Shopizy.sln --warnaserror
```

---

## 7. Observability & Health Probes

### 7.1 Health Check Endpoints
Every microservice implements ASP.NET Core Health Checks mapped via `Shopizy.ServiceDefaults`:

- **Liveness Probe**: `GET /alive` (HTTP 200 if process is up)
- **Readiness Probe**: `GET /health` (HTTP 200 if database, Redis, and message broker are reachable)

### 7.2 OpenTelemetry Traces & Metrics
- All HTTP requests and MassTransit event dispatches propagate standard `traceparent` W3C headers.
- Exported OTLP endpoints connect directly to the .NET Aspire dashboard, Prometheus, or Grafana Tempo.
- Default OTLP endpoint: `http://localhost:4317` (gRPC) or `http://localhost:4318` (HTTP).

---

## 8. Common Operational Tasks

### 8.1 Triggering Abandoned Cart Sweep Manually
When running in production or staging, the abandoned cart recovery worker can be triggered on-demand by an administrator:
```bash
curl -X POST "https://<cart-abandonment-url>/api/v1/cart-abandonment/sweep" \
  -H "Authorization: Bearer <StoreAdmin_JWT>" \
  -H "Content-Type: application/json"
```

### 8.2 Applying Promotions & Emergency Coupon Kill Switch
To deactivate an errant promotion or discount rule immediately:
```bash
curl -X DELETE "https://<promotion-service-url>/api/v1/promotions/{promotionId}" \
  -H "Authorization: Bearer <StoreAdmin_JWT>"
```

### 8.3 Re-indexing Search Engine Catalog
To re-index all product listings from catalog into search:
```bash
curl -X POST "https://<search-service-url>/api/v1/search/reindex" \
  -H "Authorization: Bearer <StoreAdmin_JWT>"
```

---

## 9. Incident Response & Troubleshooting Playbooks

### Playbook 1: Order Reservation Deadlock or Flash Sale Starvation
- **Symptoms**: High rate of HTTP 409 Conflict on `/api/v1/orders`, high checkout latency.
- **Root Cause**: Hot-key stock contention on limited-quantity items.
- **Resolution**:
  1. Inspect Redis latency and connection pool metrics in Aspire Dashboard.
  2. Verify that Lua scripts are executing without script timeout errors.
  3. Ensure the 15-minute order reservation expiration job is running (`SELECT * FROM orders WHERE status = 'PendingPayment' AND created_at < NOW() - INTERVAL '15 minutes'`).
  4. Manually trigger stock restock reconciliation if orders were abandoned.

### Playbook 2: MassTransit / RabbitMQ Dead-Letter Queue Spikes
- **Symptoms**: Messages backing up in `_error` queues (e.g. `order-created_error`).
- **Resolution**:
  1. Open RabbitMQ Management Console (`http://localhost:15672`).
  2. Inspect exception headers on failed messages in the dead-letter exchange.
  3. Check database connectivity of downstream consumer service.
  4. Fix transient downstream condition and use Shovel plugin to replay messages from `_error` queue back to primary queue.

### Playbook 3: Unauthorized Data Access or Customer Boundary Breach
- **Symptoms**: Security audit flags or customer reporting viewing another cart/order.
- **Resolution**:
  1. Immediately check application logs for missing `sub` claim or customer ID overrides.
  2. Verify repository query: ensures all queries filter on `WHERE customer_id = @currentUserId`.
  3. Rotate affected JWT signing key if token forgery is suspected.

---

## 10. Disaster Recovery & Data Resilience

### 10.1 Backup & Restore Protocol
- **PostgreSQL**:
  - Run daily automated full snapshots via `pg_dump` or WAL-G continuous archiving.
  - Test restoring into staging environment monthly.
- **Redis**:
  - Configured with RDB snapshotting + AOF persistence for cart session durability.

### 10.2 Service Recovery Order
In case of full cluster reboot or outage, restore services in dependency order:
1. **Tier 1 (Base Infrastructure)**: PostgreSQL, Redis, RabbitMQ.
2. **Tier 2 (Core Auth & Kernel)**: `Shopizy.IdentityService`.
3. **Tier 3 (Primary Data)**: `Shopizy.CatalogService`, `Shopizy.CartService`.
4. **Tier 4 (Transaction Services)**: `Shopizy.OrderService`, `Shopizy.PaymentService`, `Shopizy.ShippingService`.
5. **Tier 5 (Engagement & Peripheral)**: `Shopizy.SearchService`, `Shopizy.PromotionService`, `Shopizy.NotificationService`, `Shopizy.ReviewService`, `Shopizy.LoyaltyService`, `Shopizy.CartAbandonmentWorker`.
