# Implementation Plan: Reviews, Ratings & Wishlists (`review-service`)

## 1. Project Structure & Components

```text
src/Shopizy.ReviewService/
├── Domain/
│   ├── Entities/
│   │   ├── Review.cs
│   │   ├── ReviewVote.cs
│   │   ├── Wishlist.cs
│   │   └── WishlistItem.cs
│   ├── Exceptions/
│   │   └── ReviewDomainException.cs
│   └── Services/
│       └── RatingCalculator.cs
├── Application/
│   ├── Contracts/
│   │   └── ReviewDtos.cs
│   ├── Interfaces/
│   │   ├── IReviewRepository.cs
│   │   ├── IWishlistRepository.cs
│   │   └── IOrderVerificationClient.cs
│   └── Services/
│       └── ReviewApplicationService.cs
├── Infrastructure/
│   ├── Persistence/
│   │   ├── ReviewDbContext.cs
│   │   └── Repositories/
│   │       ├── ReviewRepository.cs
│   │       └── WishlistRepository.cs
│   └── Clients/
│       └── MockOrderVerificationClient.cs
├── Endpoints/
│   └── ReviewEndpoints.cs
├── Extensions/
│   └── ServiceExtensions.cs
└── Program.cs
```

## 2. Test Projects
- `tests/Shopizy.ReviewService.UnitTests/` (Domain logic, rating validation, voting calculation)
- `tests/Shopizy.ReviewService.IntegrationTests/` (EF Core persistence, wishlist operations, unique constraints)
- `tests/Shopizy.ReviewService.E2ETests/` (6 E2E scenarios via `WebApplicationFactory<Program>`)

## 3. Aspire Wiring
- Register PostgreSQL container `reviewdb` in `Shopizy.AppHost`
- Add `review-service` project reference and service discovery
