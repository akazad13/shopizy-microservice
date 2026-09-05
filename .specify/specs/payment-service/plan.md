# Implementation Plan: Payment & Refund Gateway (`payment-service`)

## 1. Architectural Design & Clean Architecture Layers

```
Shopizy.PaymentService/
├── Domain/
│   ├── Entities/
│   │   ├── PaymentTransaction.cs (Aggregate Root)
│   │   └── RefundRecord.cs
│   ├── Enums/
│   │   └── PaymentStatus.cs (Initiated, Succeeded, Failed, Refunded)
│   ├── ValueObjects/
│   │   ├── Money.cs
│   │   └── PaymentMethod.cs (Token, Brand, Last4)
│   └── Exceptions/
│       └── PaymentDomainException.cs
├── Application/
│   ├── Contracts/
│   │   └── PaymentDtos.cs
│   ├── Interfaces/
│   │   ├── IPaymentRepository.cs
│   │   └── IPaymentGatewayProvider.cs
│   └── Services/
│       └── PaymentApplicationService.cs
├── Infrastructure/
│   ├── Persistence/
│   │   ├── PaymentDbContext.cs
│   │   └── Repositories/
│   │       └── PaymentRepository.cs
│   └── Gateway/
│       └── MockPaymentGatewayProvider.cs
├── Endpoints/
│   └── PaymentEndpoints.cs
└── Program.cs
```

## 2. Test Architecture
```
tests/
├── Shopizy.PaymentService.UnitTests/
│   └── PaymentTransactionTests.cs
├── Shopizy.PaymentService.IntegrationTests/
│   └── PaymentPersistenceTests.cs
└── Shopizy.PaymentService.E2ETests/
    └── PaymentE2ETests.cs (6 E2E scenarios)
```

## 3. Technology & Dependencies
- .NET 10 Minimal APIs
- EF Core 10 (`PaymentDbContext`) with In-Memory testing support
- JWT Bearer Authentication & IdempotencyMiddleware
- Mockable `IPaymentGatewayProvider` supporting simulation of declines, card brands, and gateway transaction IDs.
