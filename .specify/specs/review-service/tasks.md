# Implementation Tasks: Reviews, Ratings & Wishlists (`review-service`)

## Phase 1: Setup & Domain Contracts
- [ ] Task 1.1: Create `Shopizy.ReviewService.csproj` with ASP.NET Core 10 Web SDK, EF Core, SharedKernel, ServiceDefaults references.
- [ ] Task 1.2: Implement Domain Entities (`Review`, `ReviewVote`, `Wishlist`, `WishlistItem`), Enums, and `ReviewDomainException`.
- [ ] Task 1.3: Implement Application DTO records and repository interfaces (`IReviewRepository`, `IWishlistRepository`, `IOrderVerificationClient`).

## Phase 2: Core Domain Logic & Unit Tests
- [ ] Task 2.1: Implement `RatingCalculator` service for calculating weighted average and star breakdown.
- [ ] Task 2.2: Create unit test project `tests/Shopizy.ReviewService.UnitTests` and verify rating bounds, validation, and voting math.

## Phase 3: Infrastructure, Persistence & Endpoints
- [ ] Task 3.1: Implement `ReviewDbContext`, `ReviewRepository`, and `WishlistRepository`.
- [ ] Task 3.2: Implement `MockOrderVerificationClient` to check verified buyer order status.
- [ ] Task 3.3: Implement `ReviewApplicationService` orchestrating business workflows.
- [ ] Task 3.4: Implement REST endpoints in `ReviewEndpoints.cs` with JWT authentication and customer zero-trust authorization.
- [ ] Task 3.5: Register `reviewdb` and `review-service` in `Shopizy.AppHost`.

## Phase 4: Integration & E2E Automated Tests
- [ ] Task 4.1: Create `tests/Shopizy.ReviewService.IntegrationTests` for database persistence and customer isolation.
- [ ] Task 4.2: Create `tests/Shopizy.ReviewService.E2ETests` implementing scenarios E2E-01 through E2E-06.
- [ ] Task 4.3: Add all projects to `Shopizy.sln` and verify 100% test pass rate with 0 warnings.
