# Implementation Tasks: Loyalty Points & Gift Cards (`loyalty-service`)

## Phase 1: Setup & Domain Contracts
- [ ] Task 1.1: Create `Shopizy.LoyaltyService.csproj` with ASP.NET Core 10 Web SDK, EF Core, SharedKernel, ServiceDefaults references.
- [ ] Task 1.2: Implement Domain Entities (`LoyaltyAccount`, `LoyaltyTransaction`, `GiftCard`), Enums, and `LoyaltyDomainException`.
- [ ] Task 1.3: Implement Application DTO records and repository interfaces (`ILoyaltyRepository`, `IGiftCardRepository`).

## Phase 2: Core Domain Logic & Unit Tests
- [ ] Task 2.1: Implement `LoyaltyCalculator` service for dollar-to-point and point-to-discount calculations.
- [ ] Task 2.2: Create unit test project `tests/Shopizy.LoyaltyService.UnitTests` and verify accrual, redemption, and gift card logic.

## Phase 3: Infrastructure, Persistence & Endpoints
- [ ] Task 3.1: Implement `LoyaltyDbContext`, `LoyaltyRepository`, and `GiftCardRepository`.
- [ ] Task 3.2: Implement `LoyaltyApplicationService` orchestrating points and gift card workflows.
- [ ] Task 3.3: Implement REST endpoints in `LoyaltyEndpoints.cs` with JWT authentication and customer zero-trust authorization.
- [ ] Task 3.4: Register `loyaltydb` and `loyalty-service` in `Shopizy.AppHost`.

## Phase 4: Integration & E2E Automated Tests
- [ ] Task 4.1: Create `tests/Shopizy.LoyaltyService.IntegrationTests` for database persistence and unique constraints.
- [ ] Task 4.2: Create `tests/Shopizy.LoyaltyService.E2ETests` implementing scenarios E2E-01 through E2E-06.
- [ ] Task 4.3: Add all projects to `Shopizy.sln` and verify 100% test pass rate with 0 warnings.
