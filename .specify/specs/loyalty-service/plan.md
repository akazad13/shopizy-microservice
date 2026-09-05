# Implementation Plan: Loyalty Points & Gift Cards (`loyalty-service`)

## 1. Project Structure & Components

```text
src/Shopizy.LoyaltyService/
├── Domain/
│   ├── Entities/
│   │   ├── LoyaltyAccount.cs
│   │   ├── LoyaltyTransaction.cs
│   │   └── GiftCard.cs
│   ├── Enums/
│   │   ├── LoyaltyTransactionType.cs
│   │   └── GiftCardStatus.cs
│   ├── Exceptions/
│   │   └── LoyaltyDomainException.cs
│   └── Services/
│       └── LoyaltyCalculator.cs
├── Application/
│   ├── Contracts/
│   │   └── LoyaltyDtos.cs
│   ├── Interfaces/
│   │   ├── ILoyaltyRepository.cs
│   │   └── IGiftCardRepository.cs
│   └── Services/
│       └── LoyaltyApplicationService.cs
├── Infrastructure/
│   ├── Persistence/
│   │   ├── LoyaltyDbContext.cs
│   │   └── Repositories/
│   │       ├── LoyaltyRepository.cs
│   │       └── GiftCardRepository.cs
├── Endpoints/
│   └── LoyaltyEndpoints.cs
├── Extensions/
│   └── ServiceExtensions.cs
└── Program.cs
```

## 2. Test Projects
- `tests/Shopizy.LoyaltyService.UnitTests/` (Point conversion, over-redemption errors, gift card status transitions)
- `tests/Shopizy.LoyaltyService.IntegrationTests/` (EF Core persistence, ledger transactions, unique gift card codes)
- `tests/Shopizy.LoyaltyService.E2ETests/` (6 E2E scenarios via `WebApplicationFactory<Program>`)

## 3. Aspire Wiring
- Register PostgreSQL container `loyaltydb` in `Shopizy.AppHost`
- Add `loyalty-service` project reference and service discovery
