# Implementation Tasks: Order & Inventory Service (`order-service`)

## Phase 1: Setup & Domain Modeling
- [x] 1.1 Create `src/Shopizy.OrderService` ASP.NET Core 10 Web API project.
- [x] 1.2 Add project references to `Shopizy.SharedKernel` and `Shopizy.ServiceDefaults`.
- [x] 1.3 Add NuGet package references (`Npgsql.EntityFrameworkCore.PostgreSQL`, `Microsoft.EntityFrameworkCore.InMemory`, `FluentValidation`).
- [x] 1.4 Implement `Domain/Enums/OrderStatus.cs`.
- [x] 1.5 Implement `Domain/ValueObjects/ShippingAddress.cs` and `Domain/ValueObjects/Money.cs`.
- [x] 1.6 Implement `Domain/Entities/OrderItem.cs` and `Domain/Entities/InventoryItem.cs`.
- [x] 1.7 Implement `Domain/Entities/Order.cs` aggregate root with status state machine, expiration logic, and stock restock calculation.

## Phase 2: Unit Testing (TDD)
- [x] 2.1 Create `tests/Shopizy.OrderService.UnitTests` project.
- [x] 2.2 Add unit tests for `Order` aggregate creation, address validation, and expiration calculation.
- [x] 2.3 Add unit tests for state machine transitions (`PendingPayment` -> `Processing` -> `Shipping` -> `Delivered`).
- [x] 2.4 Add unit tests for invalid transitions (e.g. cancelling shipped order).
- [x] 2.5 Add unit tests for `InventoryItem` stock reservation, overselling rejection, and restocking.

## Phase 3: Application & Infrastructure Implementation
- [x] 3.1 Define `Application/Contracts/OrderDtos.cs`.
- [x] 3.2 Define `Application/Interfaces/IOrderRepository.cs` and `IInventoryRepository.cs`.
- [x] 3.3 Implement `Application/Services/OrderService.cs` orchestrating atomic reservations and orders.
- [x] 3.4 Implement `Infrastructure/Persistence/OrderDbContext.cs` and entity type configurations.
- [x] 3.5 Implement `Infrastructure/Persistence/Repositories/OrderRepository.cs` and `InventoryRepository.cs`.
- [x] 3.6 Create `tests/Shopizy.OrderService.IntegrationTests` project and implement persistence and concurrency tests.

## Phase 4: Minimal APIs, Security & AppHost Wiring
- [x] 4.1 Implement `Endpoints/OrderEndpoints.cs` and `Endpoints/InventoryEndpoints.cs`.
- [x] 4.2 Enforce JWT authentication, customer data isolation (Principle V), and `IdempotencyMiddleware` (Principle VI).
- [x] 4.3 Wire `order-service` and PostgreSQL `orderdb` in `src/Shopizy.AppHost/Program.cs`.

## Phase 5: Automated E2E Test Suite Implementation
- [x] 5.1 Create `tests/Shopizy.OrderService.E2ETests` project with `WebApplicationFactory`.
- [x] 5.2 Implement Scenario E2E-01: Successful Order Checkout & Stock Reservation.
- [x] 5.3 Implement Scenario E2E-02: Zero-Overselling Stock Depletion Rejection.
- [x] 5.4 Implement Scenario E2E-03: 15-Minute Unpaid Expiration & Auto-Restock.
- [x] 5.5 Implement Scenario E2E-04: Order Cancellation & Restocking Prior to Shipment.
- [x] 5.6 Implement Scenario E2E-05: Customer Multi-Tenant Isolation.
- [x] 5.7 Implement Scenario E2E-06: Idempotent Checkout Protection.

## Phase 6: Review, Verification & Documentation
- [x] 6.1 Run review loop against Clean Architecture and Project Constitution.
- [x] 6.2 Verify 100% test pass rate with zero warnings across entire solution (`dotnet test --warnaserror`).
- [x] 6.3 Update `README.md` (roadmap, test counts, project structure, docs links).
- [x] 6.4 Write `.specify/specs/order-service/review-log.md` with audit outcomes.
