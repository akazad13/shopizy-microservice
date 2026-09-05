# Implementation Tasks: Shipping & Tracking Service (`shipping-service`)

- [x] 1. Architecture & Domain Model: Define `Shipment`, `ShipmentMilestone`, `ShipmentStatus`, and `CarrierRateCalculator`.
- [x] 2. Rate Estimation Engine: Implement USPS, UPS, FedEx, DHL fee formulas and $75 free ground shipping threshold.
- [x] 3. Database & Repository: Implement `ShippingDbContext` and `ShipmentRepository`.
- [x] 4. Application Service: Implement `ShippingApplicationService` for rates, shipment creation, and tracking lookups.
- [x] 5. Minimal APIs & RBAC: Implement `/api/v1/shipping/rates`, `/api/v1/shipping/shipments`, and milestone update endpoints.
- [x] 6. Aspire Wiring: Register `shippingdb` and `shipping-service` in `Shopizy.AppHost`.
- [x] 7. Automated Unit Tests: Write unit tests covering rate calculation, $75 threshold, and status progressions.
- [x] 8. Automated Integration Tests: Write integration tests for database persistence and milestone tracking history.
- [x] 9. Automated E2E Tests: Write 6 E2E tests validating rate estimation, free shipping, sub-threshold, admin creation, milestone updates, and RBAC.
