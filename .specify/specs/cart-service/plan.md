# Technical Plan: Shopping Cart Service (`cart-service`)

## 1. Architectural Design & Clean Architecture Layers

```
Shopizy.CartService
├── Domain/ (Zero external dependencies)
│   ├── Entities/
│   │   └── Cart.cs (Aggregate Root)
│   ├── ValueObjects/
│   │   ├── CartItem.cs
│   │   └── PriceDiscrepancy.cs
│   └── Exceptions/
│       └── CartDomainException.cs
├── Application/
│   ├── Common/
│   │   └── ICartRepository.cs
│   ├── Services/
│   │   └── ICatalogPriceService.cs
│   ├── DTOs/
│   │   ├── AddToCartRequest.cs
│   │   ├── UpdateCartItemRequest.cs
│   │   ├── MergeCartRequest.cs
│   │   ├── CartResponse.cs
│   │   ├── CartItemResponse.cs
│   │   └── PriceDiscrepancyResponse.cs
│   └── Handlers/
│       ├── CartCommandHandlers.cs
│       └── CartQueryHandlers.cs
├── Infrastructure/
│   ├── Redis/
│   │   └── RedisCartRepository.cs
│   └── Catalog/
│       └── CatalogPriceService.cs
├── Endpoints/
│   └── CartEndpoints.cs
└── Program.cs
```

---

## 2. Test Architecture

### 1. `tests/Shopizy.CartService.UnitTests`
- Domain Aggregate unit tests (`CartTests.cs`):
  - Item addition, quantity capping, updating, removal.
  - Price snapshot recording and integrity.
  - Subtotal calculation with multi-currency validations.
  - Cart merging edge cases (identical variants, disjoint variants, quantity overflow).
  - Price discrepancy detection algorithms.

### 2. `tests/Shopizy.CartService.IntegrationTests`
- Redis persistence repository tests (`RedisCartRepositoryTests.cs`):
  - Save and retrieve complex carts with variant attributes.
  - TTL expiration verification.
  - Deletion and concurrent update handling.

### 3. `tests/Shopizy.CartService.E2ETests`
- ASP.NET Core `WebApplicationFactory` E2E scenarios (`CartE2ETests.cs`):
  - Scenario E2E-01: Guest Cart Lifecycle & Quantity Updates.
  - Scenario E2E-02: Guest Cart Merging into Customer Cart on Login.
  - Scenario E2E-03: Price Discrepancy Detection on Cart Review.
  - Scenario E2E-04: Multi-Tenant Customer Data Isolation (Principle V).
  - Scenario E2E-05: Idempotency Key Protection (Principle VI).
  - Scenario E2E-06: Cart Clear Reset.

---

## 3. Technology Choices & Configuration
- **Runtime**: .NET 10 / C# 14 Minimal APIs.
- **Cache Provider**: StackExchange.Redis / `IDistributedCache` with JSON serializer.
- **Resilience**: Polly resilience pipelines via `Shopizy.ServiceDefaults`.
- **Security**: JWT Bearer token authentication with Role-based claims.
- **Aspire Integration**: `.AddRedis("redis")` connection string from `Shopizy.AppHost`.
