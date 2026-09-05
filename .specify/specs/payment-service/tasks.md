# Implementation Tasks: Payment & Refund Gateway (`payment-service`)

## Phase 1: Setup & Domain Modeling
- [x] 1.1 Create `src/Shopizy.PaymentService` Web API project.
- [x] 1.2 Add references to `Shopizy.SharedKernel` and `Shopizy.ServiceDefaults`.
- [x] 1.3 Implement `Domain/Enums/PaymentStatus.cs`.
- [x] 1.4 Implement `Domain/ValueObjects/Money.cs` and `Domain/ValueObjects/PaymentMethod.cs`.
- [x] 1.5 Implement `Domain/Entities/PaymentTransaction.cs` and `Domain/Entities/RefundRecord.cs`.
- [x] 1.6 Implement `Domain/Exceptions/PaymentDomainException.cs`.

## Phase 2: Unit Testing (TDD)
- [x] 2.1 Create `tests/Shopizy.PaymentService.UnitTests` project.
- [x] 2.2 Add unit tests for payment state machine (`Initiated` -> `Succeeded` -> `Refunded`).
- [x] 2.3 Add unit tests for refund amount limits and failure handling.

## Phase 3: Application & Infrastructure Implementation
- [x] 3.1 Define `Application/Contracts/PaymentDtos.cs`.
- [x] 3.2 Define `Application/Interfaces/IPaymentRepository.cs` and `IPaymentGatewayProvider.cs`.
- [x] 3.3 Implement `Application/Services/PaymentApplicationService.cs`.
- [x] 3.4 Implement `Infrastructure/Persistence/PaymentDbContext.cs` and repository.
- [x] 3.5 Implement `Infrastructure/Gateway/MockPaymentGatewayProvider.cs`.
- [x] 3.6 Create `tests/Shopizy.PaymentService.IntegrationTests` project and implement tests.

## Phase 4: Minimal APIs, Security & AppHost Wiring
- [x] 4.1 Implement `Endpoints/PaymentEndpoints.cs`.
- [x] 4.2 Enforce JWT authentication, customer data isolation (Principle V), and `IdempotencyMiddleware` (Principle VI).
- [x] 4.3 Wire `payment-service` and PostgreSQL `paymentdb` in `src/Shopizy.AppHost/Program.cs`.

## Phase 5: Automated E2E Test Suite Implementation
- [x] 5.1 Create `tests/Shopizy.PaymentService.E2ETests` project with `WebApplicationFactory`.
- [x] 5.2 Implement Scenario E2E-01: Successful Card Payment.
- [x] 5.3 Implement Scenario E2E-02: Declined Card Payment.
- [x] 5.4 Implement Scenario E2E-03: Automated Post-Payment Refund.
- [x] 5.5 Implement Scenario E2E-04: Duplicate Charge Prevention via Idempotency.
- [x] 5.6 Implement Scenario E2E-05: Customer Multi-Tenant Isolation.
- [x] 5.7 Implement Scenario E2E-06: Double Refund Rejection.

## Phase 6: Review, Verification & Documentation
- [x] 6.1 Run review loop against Clean Architecture and Project Constitution.
- [x] 6.2 Verify 100% test pass rate with zero warnings across entire solution (`dotnet test --warnaserror`).
- [x] 6.3 Update `README.md` (roadmap, test counts, project structure, docs links).
- [x] 6.4 Write `.specify/specs/payment-service/review-log.md` with audit outcomes.
