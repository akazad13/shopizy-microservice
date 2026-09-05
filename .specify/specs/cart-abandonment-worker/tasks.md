# Implementation Tasks: Abandoned Cart Recovery Worker (`cart-abandonment-worker`)

## Phase 1: Setup & Domain Contracts
- [ ] Task 1.1: Create `Shopizy.CartAbandonmentWorker.csproj` with ASP.NET Core 10 Web SDK, EF Core, SharedKernel, ServiceDefaults references.
- [ ] Task 1.2: Implement Domain Entities (`AbandonedCartRecord`), Services (`AbandonmentPolicy`), and `CartAbandonmentDomainException`.
- [ ] Task 1.3: Implement Application DTO records and repository/client interfaces (`IAbandonedCartRepository`, `ICartSnapshotClient`, `INotificationDispatcherClient`).

## Phase 2: Core Domain Logic & Unit Tests
- [ ] Task 2.1: Implement `AbandonmentPolicy` (2-hour inactivity threshold, 24-hour cooldown logic, restore URL formatting).
- [ ] Task 2.2: Create unit test project `tests/Shopizy.CartAbandonmentWorker.UnitTests` and verify thresholds, cooldowns, and empty cart guards.

## Phase 3: Infrastructure, Persistence & Endpoints
- [ ] Task 3.1: Implement `AbandonmentDbContext`, `AbandonedCartRepository`, and mock clients.
- [ ] Task 3.2: Implement `CartAbandonmentApplicationService` orchestrating detection sweeps and token restorations.
- [ ] Task 3.3: Implement REST endpoints in `CartAbandonmentEndpoints.cs` and `CartAbandonmentBackgroundService`.
- [ ] Task 3.4: Register `abandonmentdb` and `cart-abandonment-worker` in `Shopizy.AppHost`.

## Phase 4: Integration & E2E Automated Tests
- [ ] Task 4.1: Create `tests/Shopizy.CartAbandonmentWorker.IntegrationTests` for database persistence and token uniqueness.
- [ ] Task 4.2: Create `tests/Shopizy.CartAbandonmentWorker.E2ETests` implementing scenarios E2E-01 through E2E-06.
- [ ] Task 4.3: Add all projects to `Shopizy.sln` and verify 100% test pass rate with 0 warnings.
