# Implementation Plan: Abandoned Cart Recovery Worker (`cart-abandonment-worker`)

## 1. Project Structure & Components

```text
src/Shopizy.CartAbandonmentWorker/
├── Domain/
│   ├── Entities/
│   │   └── AbandonedCartRecord.cs
│   ├── Exceptions/
│   │   └── CartAbandonmentDomainException.cs
│   └── Services/
│       └── AbandonmentPolicy.cs
├── Application/
│   ├── Contracts/
│   │   └── CartAbandonmentDtos.cs
│   ├── Interfaces/
│   │   ├── IAbandonedCartRepository.cs
│   │   ├── ICartSnapshotClient.cs
│   │   └── INotificationDispatcherClient.cs
│   └── Services/
│       └── CartAbandonmentApplicationService.cs
├── Infrastructure/
│   ├── Persistence/
│   │   ├── AbandonmentDbContext.cs
│   │   └── Repositories/
│   │       └── AbandonedCartRepository.cs
│   └── Clients/
│       ├── MockCartSnapshotClient.cs
│       └── MockNotificationDispatcherClient.cs
├── Background/
│   └── CartAbandonmentBackgroundService.cs
├── Endpoints/
│   └── CartAbandonmentEndpoints.cs
├── Extensions/
│   └── ServiceExtensions.cs
└── Program.cs
```

## 2. Test Projects
- `tests/Shopizy.CartAbandonmentWorker.UnitTests/` (AbandonmentPolicy, threshold checks, cooldown rules, URL formatting)
- `tests/Shopizy.CartAbandonmentWorker.IntegrationTests/` (EF Core persistence, token queries, customer history)
- `tests/Shopizy.CartAbandonmentWorker.E2ETests/` (6 E2E scenarios via `WebApplicationFactory<Program>`)

## 3. Aspire Wiring
- Register PostgreSQL container `abandonmentdb` in `Shopizy.AppHost`
- Add `cart-abandonment-worker` project reference and service discovery
