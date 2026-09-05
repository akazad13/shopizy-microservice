# Implementation Plan: Shipping & Tracking Service (`shipping-service`)

## 1. Architectural Approach
`shipping-service` is an autonomous ASP.NET Core 10 Minimal API service built following Clean Architecture:
- Dedicated PostgreSQL database resource `shippingdb` registered in `Shopizy.AppHost`.
- Pure Domain entities: `Shipment`, `ShipmentMilestone`, `ShipmentStatus`, `CarrierRateCalculator`.
- Application service orchestrating rate lookups, shipment lifecycle, and milestone tracking.
- EF Core database-per-service isolation with PostgreSQL / InMemory test provider.

## 2. Solution Structure
- `src/Shopizy.ShippingService/`
  - `Domain/`:
    - `Entities/Shipment.cs`, `Entities/ShipmentMilestone.cs`, `Enums/ShipmentStatus.cs`, `Services/CarrierRateCalculator.cs`
  - `Application/`:
    - `Contracts/ShippingDtos.cs`
    - `Interfaces/IShipmentRepository.cs`
    - `Services/ShippingApplicationService.cs`
  - `Infrastructure/`:
    - `Persistence/ShippingDbContext.cs`, `Persistence/Repositories/ShipmentRepository.cs`
  - `Endpoints/ShippingEndpoints.cs`
  - `Extensions/ServiceExtensions.cs`
  - `Program.cs`
- `tests/Shopizy.ShippingService.UnitTests/`
- `tests/Shopizy.ShippingService.IntegrationTests/`
- `tests/Shopizy.ShippingService.E2ETests/`
