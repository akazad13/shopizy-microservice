# Actionable Tasks: Product Catalog Service (`catalog-service`)

## Phase 1: Setup & Domain Contracts
- [x] [P1-01] Create `src/Shopizy.CatalogService/Shopizy.CatalogService.csproj` configured for .NET 10 referencing `Shopizy.SharedKernel` and `Shopizy.ServiceDefaults`.
- [x] [P1-02] Define `ProductStatus` enum (`Draft`, `Published`, `Archived`) and `Money` value object.
- [x] [P1-03] Define `Category` entity with hierarchical self-reference (`ParentCategoryId`, `SubCategories`).
- [x] [P1-04] Define `Brand` entity with slug, website URL, and logo URL properties.
- [x] [P1-05] Define `ProductImage` entity and `ProductVariant` entity with SKU, barcode, price, stock, and attributes dictionary.
- [x] [P1-06] Define `Product` aggregate root with child collections, optimistic concurrency `Version`, and domain events (`ProductCreatedDomainEvent`, `ProductUpdatedDomainEvent`, `ProductStockUpdatedDomainEvent`).
- [x] [P1-07] Define Application DTOs (`CategoryContracts.cs`, `BrandContracts.cs`, `ProductContracts.cs`, `StockAdjustmentRequest.cs`, `PagedResult.cs`).
- [x] [P1-08] Define repository interfaces (`ICategoryRepository`, `IBrandRepository`, `IProductRepository`) and service interface `ICatalogService`.

## Phase 2: Core Domain Logic & Unit Tests
- [x] [P2-01] Implement domain validation rules for Category, Brand, Product, and ProductVariant.
- [x] [P2-02] Implement status transitions (`Draft -> Published -> Archived`) and invariant enforcement in `Product` aggregate.
- [x] [P2-03] Implement variant manipulation and stock adjustments in `Product` aggregate with optimistic concurrency token increment.
- [x] [P2-04] Create `tests/Shopizy.CatalogService.UnitTests` project.
- [x] [P2-05] Write unit tests for `Category` and `Brand` invariants.
- [x] [P2-06] Write unit tests for `Money` value object operations and validations.
- [x] [P2-07] Write unit tests for `Product` and `ProductVariant` aggregate behaviors, variant SKU uniqueness, and stock adjustments.
- [x] [P2-08] Write unit tests for `CatalogService` query filtering, sorting, and pagination logic.

## Phase 3: Infrastructure, Services & Integration Tests
- [x] [P3-01] Implement `CatalogDbContext` with EF Core entity configurations, indexes, and concurrency token mappings.
- [x] [P3-02] Implement repositories (`CategoryRepository`, `BrandRepository`, `ProductRepository`) with filtering and specification queries.
- [x] [P3-03] Implement `CatalogService` implementing all use cases with functional `Result<T>` pattern.
- [x] [P3-04] Implement Minimal API endpoints for Categories (`/api/v1/catalog/categories`), Brands (`/api/v1/catalog/brands`), and Products (`/api/v1/catalog/products`).
- [x] [P3-05] Wire JWT authentication, RBAC authorization (`StoreAdminOnly`), and shared idempotency middleware.
- [x] [P3-06] Create `tests/Shopizy.CatalogService.IntegrationTests` project.
- [x] [P3-07] Write integration tests for database persistence, hierarchical category queries, and optimistic concurrency conflicts.

## Phase 4: Automated E2E Test Suite
- [x] [P4-01] Create `tests/Shopizy.CatalogService.E2ETests` project with `WebApplicationFactory`.
- [x] [P4-02] Implement Scenario E2E-01: StoreAdmin Category Hierarchy & Brand Creation.
- [x] [P4-03] Implement Scenario E2E-02: StoreAdmin Product with Dimensional Variants & Gallery Creation.
- [x] [P4-04] Implement Scenario E2E-03: Customer Public Browsing, Filtering, Sorting & Pagination.
- [x] [P4-05] Implement Scenario E2E-04: Customer Product Detail & Live Variant Stock Inspection.
- [x] [P4-06] Implement Scenario E2E-05: RBAC Security Enforcement (Customer rejected on Admin Mutation endpoints with 403 Forbidden).
- [x] [P4-07] Implement Scenario E2E-06: Optimistic Concurrency Protection on Product Update (409 Conflict).
- [x] [P4-08] Implement Scenario E2E-07: Idempotent Product Creation via `Idempotency-Key` Header.

## Phase 5: Solution Integration, Verification & Multi-Agent Review Gate
- [x] [P5-01] Add new projects to `Shopizy.sln` and update `Shopizy.AppHost`.
- [x] [P5-02] Execute solution-wide `dotnet build --warnaserror` with zero warnings.
- [x] [P5-03] Execute solution-wide `dotnet test` with 100% pass rate.
- [x] [P5-04] Perform adversarial Review Agent audit and generate `review-log.md`.
- [x] [P5-05] Mark all tasks complete upon review sign-off.
