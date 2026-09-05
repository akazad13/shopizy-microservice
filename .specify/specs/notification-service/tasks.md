# Tasks: Notification & Real-Time Push Service (`notification-service`)

- [ ] **Task 1: Setup & Domain Model**
  - Define `Notification` aggregate, `NotificationType`, `NotificationChannel`, `NotificationStatus`, and `NotificationDomainException`.
- [ ] **Task 2: Application Contracts & Service**
  - Define DTOs, `INotificationRepository`, and `NotificationApplicationService` with dispatch and push broadcasting logic.
- [ ] **Task 3: Infrastructure & Persistence**
  - Implement `NotificationDbContext`, `NotificationRepository`, and `Shopizy.NotificationService.csproj`.
- [ ] **Task 4: Real-Time SignalR Hubs & Endpoints**
  - Implement `NotificationHub`, `MerchantFeedHub`, and Minimal API endpoints in `NotificationEndpoints.cs`.
- [ ] **Task 5: Wire Aspire Orchestration**
  - Update `Shopizy.AppHost` and `Shopizy.sln` to include `Shopizy.NotificationService`.
- [ ] **Task 6: Unit Test Suite**
  - Implement `Shopizy.NotificationService.UnitTests` (4 tests).
- [ ] **Task 7: Integration Test Suite**
  - Implement `Shopizy.NotificationService.IntegrationTests` (2 tests).
- [ ] **Task 8: Automated E2E Test Suite**
  - Implement `Shopizy.NotificationService.E2ETests` (6 tests fulfilling Section 6.3).
- [ ] **Task 9: Verification & Documentation**
  - Verify solution build with `--warnaserror`, run all tests, update `README.md`, and produce `review-log.md`.
