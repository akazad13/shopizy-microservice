# Technical Implementation Plan: Product Catalog Service (`catalog-service`)

> **Module Slug:** `catalog-service`  
> **Target Framework:** .NET 10 (C# 14)  
> **Architectural Pattern:** Clean Architecture (Hexagonal)  
> **Host Model:** ASP.NET Core Minimal API with .NET Aspire (`Shopizy.ServiceDefaults`)  

---

## 1. Architectural Alignment & Project Structure

The Product Catalog Service follows strict Clean Architecture boundaries and integrates with the existing solution projects:

- **Referenced Projects**:
  - `src/Shopizy.SharedKernel`: DDD primitives (`AggregateRoot`, `Entity`, `ValueObject`), functional `Result<T>`, error models, global exception handler, `IdempotencyMiddleware`, and `IIdempotencyStore`.
  - `src/Shopizy.ServiceDefaults`: OpenTelemetry instrumentation, standard resilience handlers, health check mappings (`/health`, `/alive`).
- **Orchestration**:
  - `src/Shopizy.AppHost`: Registers `catalog-service` as an Aspire microservice resource with reference to PostgreSQL.
- **Projects to Create**:
  - `src/Shopizy.CatalogService/Shopizy.CatalogService.csproj` (Web API)
  - `tests/Shopizy.CatalogService.UnitTests/Shopizy.CatalogService.UnitTests.csproj`
  - `tests/Shopizy.CatalogService.IntegrationTests/Shopizy.CatalogService.IntegrationTests.csproj`
  - `tests/Shopizy.CatalogService.E2ETests/Shopizy.CatalogService.E2ETests.csproj`

---

## 2. Directory Layout

```text
src/Shopizy.CatalogService/
  ├── Domain/
  │   ├── Entities/
  │   │   ├── Category.cs
  │   │   ├── Brand.cs
  │   │   ├── Product.cs
  │   │   ├── ProductVariant.cs
  │   │   └── ProductImage.cs
  │   ├── Enums/
  │   │   └── ProductStatus.cs
  │   ├── ValueObjects/
  │   │   └── Money.cs
  │   ├── Events/
  │   │   ├── ProductCreatedDomainEvent.cs
  │   │   ├── ProductUpdatedDomainEvent.cs
  │   │   └── ProductStockUpdatedDomainEvent.cs
  │   └── Exceptions/
  │       └── ConcurrencyException.cs
  ├── Application/
  │   ├── Contracts/
  │   │   ├── CategoryContracts.cs (CreateCategoryRequest, UpdateCategoryRequest, CategoryResponse)
  │   │   ├── BrandContracts.cs (CreateBrandRequest, UpdateBrandRequest, BrandResponse)
  │   │   ├── ProductContracts.cs (CreateProductRequest, UpdateProductRequest, ProductListResponse, ProductDetailResponse, ProductVariantDto, ProductImageDto)
  │   │   ├── StockAdjustmentRequest.cs
  │   │   └── PagedResult.cs
  │   ├── Interfaces/
  │   │   ├── ICatalogDbContext.cs
  │   │   ├── ICategoryRepository.cs
  │   │   ├── IBrandRepository.cs
  │   │   ├── IProductRepository.cs
  │   │   └── ICatalogService.cs
  │   └── Services/
  │       └── CatalogService.cs
  ├── Infrastructure/
  │   ├── Persistence/
  │   │   ├── CatalogDbContext.cs
  │   │   ├── Configurations/
  │   │   │   ├── CategoryConfiguration.cs
  │   │   │   ├── BrandConfiguration.cs
  │   │   │   ├── ProductConfiguration.cs
  │   │   │   └── ProductVariantConfiguration.cs
  │   │   └── Repositories/
  │   │       ├── CategoryRepository.cs
  │   │       ├── BrandRepository.cs
  │   │       └── ProductRepository.cs
  ├── Endpoints/
  │   ├── CategoryEndpoints.cs
  │   ├── BrandEndpoints.cs
  │   └── ProductEndpoints.cs
  ├── Extensions/
  │   └── ServiceCollectionExtensions.cs
  ├── Program.cs
  ├── appsettings.json
  └── Shopizy.CatalogService.csproj

tests/Shopizy.CatalogService.UnitTests/
  ├── Domain/
  │   ├── CategoryTests.cs
  │   ├── BrandTests.cs
  │   ├── MoneyValueObjectTests.cs
  │   ├── ProductAggregateTests.cs
  │   └── ProductVariantTests.cs
  └── Application/
      └── CatalogServiceTests.cs

tests/Shopizy.CatalogService.IntegrationTests/
  ├── Persistence/
  │   ├── CategoryRepositoryTests.cs
  │   ├── BrandRepositoryTests.cs
  │   └── ProductRepositoryTests.cs
  └── Concurrency/
      └── OptimisticConcurrencyTests.cs

tests/Shopizy.CatalogService.E2ETests/
  ├── Fixtures/
  │   └── CatalogApplicationFactory.cs
  └── Scenarios/
      └── CatalogE2ETests.cs
```

---

## 3. Package Dependencies

### `Shopizy.CatalogService.csproj`
- `Microsoft.AspNetCore.OpenApi`
- `Microsoft.AspNetCore.Authentication.JwtBearer`
- `Microsoft.EntityFrameworkCore`
- `Microsoft.EntityFrameworkCore.InMemory` (for fast unit/integration test runners)
- `Npgsql.EntityFrameworkCore.PostgreSQL`
- `Shopizy.SharedKernel`
- `Shopizy.ServiceDefaults`

### Test Projects
- `Microsoft.NET.Test.Sdk`
- `xunit`
- `xunit.runner.visualstudio`
- `FluentAssertions`
- `Moq`
- `Microsoft.AspNetCore.Mvc.Testing`
- `Microsoft.EntityFrameworkCore.InMemory`

---

## 4. Implementation Strategy

1. **Domain Layer**:
   - Pure domain models without framework dependencies.
   - `Money` value object (amount, currency with default USD, equality comparison, formatting).
   - `Category` with hierarchical navigation (`ParentCategoryId`, `SubCategories`), active flag.
   - `Brand` with slug validation and active flag.
   - `Product` aggregate root controlling `ProductVariant` collection and `ProductImage` collection.
   - `ProductVariant` enforcing positive pricing, non-negative stock quantity, unique SKU, JSON serialized dynamic attributes.
   - `Version` integer property with `[ConcurrencyCheck]` to enforce optimistic concurrency on product modifications.

2. **Application Layer**:
   - `ICatalogService` coordinating repository calls, input validation, and business logic.
   - Filtering & sorting logic for catalog queries (category, brand, price range, stock, search term).
   - Clean result mapping using functional `Result<T>` and `Error` abstractions.

3. **Infrastructure Layer**:
   - `CatalogDbContext` with Fluent API mappings, indices for slugs, SKUs, and category hierarchy.
   - Repositories implementing query specifications and concurrency-safe updates.

4. **Presentation / Minimal APIs**:
   - Minimal API endpoints mapped cleanly under `/api/v1/catalog`.
   - Anonymous access for public queries; `RequireAuthorization("StoreAdminOnly")` for mutations.
   - Idempotency middleware registered for duplicate request prevention on admin endpoints.

5. **Testing Suite**:
   - Unit tests covering domain invariants, calculations, and validation rules.
   - Integration tests verifying EF Core relational mappings, child collection operations, and optimistic concurrency.
   - Automated E2E tests covering all 7 scenarios defined in Section 6.3 of `spec.md`.
