# Implementation Tasks: Promotion & Coupon Service (`promotion-service`)

- [x] 1. Architecture & Domain Model: Define `PromotionCampaign`, `DiscountType`, and domain invariants.
- [x] 2. Calculation Engine: Implement discount strategies (percentage, fixed amount, BOGO, minimum spend, category filters, and safety cap ceilings).
- [x] 3. Database & Repository: Implement `PromotionDbContext` (Postgres / InMemory for tests) with concurrency and usage counters.
- [x] 4. Application Service: Implement `PromotionApplicationService` orchestrating campaign evaluation and usage tracking.
- [x] 5. Minimal APIs & RBAC: Implement `/api/v1/promotions/apply` and StoreAdmin-secured `/api/v1/promotions/campaigns`.
- [x] 6. Aspire Wiring: Register `promotiondb` and `promotion-service` in `Shopizy.AppHost`.
- [x] 7. Automated Unit Tests: Write unit tests covering percentage calculation, caps, minimum spend, BOGO, and expiration.
- [x] 8. Automated Integration Tests: Write integration tests for persistence and campaign lifecycle.
- [x] 9. Automated E2E Tests: Write 6 E2E tests validating capped percentage, fixed discount, minimum spend rejection, category restrictions, BOGO, and usage exhaustion.
