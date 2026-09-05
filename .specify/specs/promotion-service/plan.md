# Implementation Plan: Promotion & Coupon Service (`promotion-service`)

## 1. Architectural Approach
`promotion-service` is an autonomous ASP.NET Core 10 Minimal API service built following Clean Architecture:
- Dedicated PostgreSQL database resource `promotiondb` registered in `Shopizy.AppHost`.
- Pure Domain entities (`PromotionCampaign`, `CouponRule`, `DiscountType`) without infrastructure dependencies.
- Strategy-based calculation engine (`IPromotionCalculationStrategy`) executing percentage, fixed, and BOGO rules with safety limits.
- Admin-secured campaign authoring endpoints and high-speed public promotion evaluation endpoints.

## 2. Solution Structure
- `src/Shopizy.PromotionService/`
  - `Domain/`:
    - `Entities/PromotionCampaign.cs`, `Enums/DiscountType.cs`, `Exceptions/PromotionDomainException.cs`
  - `Application/`:
    - `Contracts/PromotionDtos.cs`
    - `Interfaces/IPromotionRepository.cs`, `Interfaces/IPromotionCalculator.cs`
    - `Services/PromotionApplicationService.cs`
  - `Infrastructure/`:
    - `Persistence/PromotionDbContext.cs`, `Persistence/Repositories/PromotionRepository.cs`
    - `Calculators/DefaultPromotionCalculator.cs`
  - `Endpoints/PromotionEndpoints.cs`
  - `Extensions/ServiceExtensions.cs`
  - `Program.cs`
- `tests/Shopizy.PromotionService.UnitTests/`
- `tests/Shopizy.PromotionService.IntegrationTests/`
- `tests/Shopizy.PromotionService.E2ETests/`
