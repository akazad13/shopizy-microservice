# Implementation Plan: Notification & Real-Time Push Service (`notification-service`)

## 1. Architectural Approach & Layering
- **Clean Architecture** implementation in `src/Shopizy.NotificationService/`:
  - **Domain**: `Notification`, `NotificationType`, `NotificationChannel`, `NotificationStatus`, `NotificationDomainException`.
  - **Application**: DTOs, `INotificationRepository`, `INotificationDispatcher`, `NotificationApplicationService`.
  - **Infrastructure**: `NotificationDbContext`, `NotificationRepository`, `MockEmailDispatcher`, `SignalRPushNotifier`.
  - **Hubs & Endpoints**: `NotificationHub`, `MerchantFeedHub`, Minimal API endpoints in `NotificationEndpoints.cs`.
- **Testing Architecture**:
  - `tests/Shopizy.NotificationService.UnitTests/`
  - `tests/Shopizy.NotificationService.IntegrationTests/`
  - `tests/Shopizy.NotificationService.E2ETests/`

## 2. Aspire Orchestration & Persistence
- Register `notificationdb` (Postgres / In-Memory in test) in `Shopizy.AppHost/Program.cs`.
- Wire `Shopizy.NotificationService` into `Shopizy.AppHost` with references to `notificationdb`, `redis`, and `rabbitmq`.

## 3. Test Suites Plan
- **Unit Tests (4)**: Email format validation, tracking link formatting, notification status transitions, template generation.
- **Integration Tests (2)**: EF Core persistence roundtrip, customer isolation queries.
- **E2E Tests (6)**: 6 scenarios defined in Section 6.3 of `spec.md` with WebApplicationFactory test host.
