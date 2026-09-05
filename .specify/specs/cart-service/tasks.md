# Implementation Tasks: Shopping Cart Service (`cart-service`)

## Phase 1: Setup & Domain Modeling
- [x] 1.1 Create `src/Shopizy.CartService` ASP.NET Core 10 Web API project.
- [x] 1.2 Add project references to `Shopizy.SharedKernel` and `Shopizy.ServiceDefaults`.
- [x] 1.3 Add NuGet package references (`Microsoft.Extensions.Caching.StackExchangeRedis`, `FluentValidation`).
- [x] 1.4 Implement `Domain/ValueObjects/CartItem.cs` and `Domain/ValueObjects/PriceDiscrepancy.cs`.
- [x] 1.5 Implement `Domain/Entities/Cart.cs` aggregate root with price snapshotting, item mutation, merging, and discrepancy detection.
- [x] 1.6 Implement `Domain/Exceptions/CartDomainException.cs`.

## Phase 2: Unit Testing (TDD)
- [x] 2.1 Create `tests/Shopizy.CartService.UnitTests` project with xUnit and FluentAssertions.
- [x] 2.2 Add unit tests for Cart creation, item validation, and quantity capping.
- [x] 2.3 Add unit tests for price snapshotting and subtotal recalculation.
- [x] 2.4 Add unit tests for guest cart merging (identical variants, disjoint variants).
- [x] 2.5 Add unit tests for live price discrepancy detection.

## Phase 3: Application & Infrastructure Implementation
- [x] 3.1 Define `Application/Common/ICartRepository.cs` and `Application/Services/ICatalogPriceService.cs`.
- [x] 3.2 Define request and response DTOs (`AddToCartRequest`, `UpdateCartItemRequest`, `MergeCartRequest`, `CartResponse`).
- [x] 3.3 Implement `Infrastructure/Redis/RedisCartRepository.cs` with JSON serialization and TTL management.
- [x] 3.4 Implement `Infrastructure/Catalog/CatalogPriceService.cs` (with simulated/mockable provider for testing).
- [x] 3.5 Create `tests/Shopizy.CartService.IntegrationTests` project and implement Redis persistence integration tests.

## Phase 4: Minimal APIs, Security & AppHost Wiring
- [x] 4.1 Implement `Endpoints/CartEndpoints.cs` for GET, POST, PUT, DELETE cart operations and POST /merge.
- [x] 4.2 Enforce JWT Bearer authentication, Customer isolation (Principle V), and IdempotencyMiddleware (Principle VI).
- [x] 4.3 Wire `cart-service` and Redis into `src/Shopizy.AppHost/Program.cs`.

## Phase 5: Automated E2E Test Suite Implementation
- [x] 5.1 Create `tests/Shopizy.CartService.E2ETests` project with `WebApplicationFactory`.
- [x] 5.2 Implement Scenario E2E-01: Guest Cart Lifecycle & Quantity Updates.
- [x] 5.3 Implement Scenario E2E-02: Guest Cart Merging into Customer Cart on Login.
- [x] 5.4 Implement Scenario E2E-03: Price Discrepancy Detection on Cart Review.
- [x] 5.5 Implement Scenario E2E-04: Multi-Tenant Customer Data Isolation (Principle V).
- [x] 5.6 Implement Scenario E2E-05: Idempotency Key Protection (Principle VI).
- [x] 5.7 Implement Scenario E2E-06: Cart Clear Reset.

## Phase 6: Review, Verification & Documentation
- [x] 6.1 Run multi-agent review loop (auditing against Clean Architecture and Constitution).
- [x] 6.2 Verify 100% test pass rate with zero warnings across entire solution (`dotnet test --warnaserror`).
- [x] 6.3 Update `README.md` (roadmap, test counts, project structure, docs links).
- [x] 6.4 Write `.specify/specs/cart-service/review-log.md` with audit outcomes.
