# Implementation Plan: Order & Inventory Service (`order-service`)

## 1. Architectural Design & Clean Architecture Layers

```
Shopizy.OrderService/
├── Domain/
│   ├── Entities/
│   │   ├── Order.cs (Aggregate Root)
│   │   ├── OrderItem.cs
│   │   └── InventoryItem.cs
│   ├── Enums/
│   │   └── OrderStatus.cs
│   ├── ValueObjects/
│   │   ├── ShippingAddress.cs
│   │   └── Money.cs
│   └── Exceptions/
│       └── OrderDomainException.cs
├── Application/
│   ├── Contracts/
│   │   └── OrderDtos.cs
│   ├── Interfaces/
│   │   ├── IOrderRepository.cs
│   │   └── IInventoryRepository.cs
│   └── Services/
│       └── OrderService.cs
├── Infrastructure/
│   ├── Persistence/
│   │   ├── OrderDbContext.cs
│   │   ├── Repositories/
│   │   │   ├── OrderRepository.cs
│   │   │   └── InventoryRepository.cs
│   │   └── Configurations/
│   └── BackgroundServices/
│       └── OrderExpirationBackgroundService.cs
├── Endpoints/
│   ├── OrderEndpoints.cs
│   └── InventoryEndpoints.cs
└── Program.cs
```

## 2. Test Architecture

```
tests/
├── Shopizy.OrderService.UnitTests/
│   ├── OrderAggregateTests.cs
│   └── InventoryItemTests.cs
├── Shopizy.OrderService.IntegrationTests/
│   └── Persistence/
│       ├── OrderRepositoryTests.cs
│       └── InventoryConcurrencyTests.cs
└── Shopizy.OrderService.E2ETests/
    ├── Fixtures/
    │   └── OrderWebApplicationFactory.cs
    └── Scenarios/
        └── OrderE2ETests.cs (6 E2E scenarios)
```

## 3. Technology & Dependencies
- **Runtime**: .NET 10 Minimal APIs
- **Database**: PostgreSQL (`orderdb`) via EF Core 10 + InMemory for test runs
- **JWT Authentication**: `Microsoft.AspNetCore.Authentication.JwtBearer`
- **Idempotency**: `Shopizy.SharedKernel.Middleware.Idempotency`
- **Testing**: xUnit, FluentAssertions, `Microsoft.AspNetCore.Mvc.Testing`
- **Aspire Integration**: `Shopizy.AppHost` with PostgreSQL database resource `orderdb`
