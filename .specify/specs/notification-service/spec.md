# Specification: Notification & Real-Time Push Service (`notification-service`)

## 1. Executive Summary & Objectives
The **Notification & Real-Time Push Service** (`notification-service`) powers real-time bi-directional communications and transactional notification dispatch across the Shopizy microservices ecosystem. It provides shoppers with sub-second order fulfillment push notifications via SignalR WebSocket hubs, delivers a live administrative sales and operational metric feed for store merchants, and orchestrates transactional notifications (order confirmations, shipment dispatches with tracking URLs, password resets, and stock alerts) with durable logging and customer data isolation.

---

## 2. Personas & User Stories

- **US-1 (Shopper Live Tracking Push)**: As an online shopper, I want to receive instantaneous live push updates (<1s latency) on my order tracking screen when my order changes status (Processing -> Shipped -> Delivered), so that I know package progress without manual page reloads.
- **US-2 (Merchant Live Sales Feed)**: As a store administrator, I want to observe a real-time event stream of incoming orders, revenue milestones, and cancellation alerts on the merchant dashboard, so that operations can respond without delay.
- **US-3 (Transactional Email Dispatch)**: As an online shopper, I want to receive transactional emails (Order Confirmation, Shipment Dispatch with carrier tracking URL, Password Reset, and Back-in-Stock alerts), so that I have durable records of my account and order transactions.
- **US-4 (Customer Notification Center & Data Isolation)**: As a registered customer, I want to view my notification history in a secure portal, ensuring no other customer can inspect my alerts.
- **US-5 (Administrative Notification Dispatch & Monitoring)**: As an administrative system, I want to trigger system notifications and inspect delivery audit trails via secured REST endpoints.

---

## 3. Detailed Acceptance Criteria (Given-When-Then)

### AC-1.1: Live Order Tracking Push
- **Given** an active customer connected to `NotificationHub` joined to their order group `order_{orderId}`,
- **When** an order status update event is published (e.g. `Shipped`),
- **Then** the service broadcasts an `OrderStatusUpdated` event containing `orderId`, `status`, `trackingNumber`, and `timestampUtc` directly to the order group within 1000ms.

### AC-2.1: Merchant Dashboard Feed
- **Given** an authenticated user with `StoreAdmin` role connected to `MerchantFeedHub`,
- **When** a merchant event is dispatched (e.g. `NewOrderPlaced` with order total, or `OrderCancelled`),
- **Then** the hub broadcasts a `MerchantEventReceived` payload containing the event type, metric amount, and summary to all active merchant clients.
- **And** non-admin (customer) connections are rejected from joining the merchant hub with 403 Forbidden.

### AC-3.1: Transactional Email Dispatch with Tracking Link
- **Given** an order shipment dispatch event with carrier `"FedEx"` and tracking number `"trk_fedex_123"`,
- **When** the transactional notification is triggered for customer `"alex@example.com"`,
- **Then** the service renders the `ShipmentDispatched` template including tracking link `https://shopizy.com/track/trk_fedex_123`, marks status `Sent`, and persists the log record.

### AC-4.1: Customer Notification History & Zero-Trust Isolation
- **Given** customer A (`userId: 11111111-...`) and customer B (`userId: 22222222-...`),
- **When** customer A queries `GET /api/v1/notifications/user/11111111-...`,
- **Then** only customer A's notifications are returned.
- **And** if customer A attempts to query `GET /api/v1/notifications/user/22222222-...`, the service returns `403 Forbidden` according to Constitution Principle V.

---

## 4. API & Integration Contracts

### REST Endpoints
- `POST /api/v1/notifications/send` (Admin only)
  - Request: `SendNotificationRequest(Guid UserId, string RecipientEmail, NotificationType Type, string Subject, string Body, Dictionary<string, string>? Metadata)`
  - Response: `201 Created` with `NotificationResponse`
- `GET /api/v1/notifications/user/{userId}` (Customer self or Admin)
  - Response: `200 OK` with `IReadOnlyList<NotificationResponse>`
- `POST /api/v1/notifications/push/order-status` (Admin / Internal)
  - Request: `OrderStatusPushRequest(Guid OrderId, Guid CustomerId, string Status, string? TrackingNumber, string? Carrier)`
  - Response: `200 OK` with `{ "broadcasted": true, "timestampUtc": ... }`
- `POST /api/v1/notifications/push/merchant-event` (Admin only)
  - Request: `MerchantEventPushRequest(string EventType, decimal Amount, string Currency, string Description)`
  - Response: `200 OK` with `{ "broadcasted": true, "timestampUtc": ... }`

### SignalR WebSocket Hubs
- `/hubs/notifications` (Bearer auth required)
  - Client methods: `JoinOrderGroup(string orderId)`
  - Server events: `OrderStatusUpdated(OrderStatusPushPayload payload)`
- `/hubs/merchant-feed` (Admin Bearer auth required)
  - Server events: `MerchantEventReceived(MerchantEventPayload payload)`

---

## 5. Data Models & State Transitions

### Notification Entity (`Notification`)
- `Id`: `Guid` (PK)
- `UserId`: `Guid` (Indexed)
- `Recipient`: `string` (Email or identifier)
- `Type`: `NotificationType` (`OrderConfirmation`, `ShipmentDispatched`, `PasswordReset`, `PriceDrop`, `BackInStock`, `MerchantAlert`)
- `Channel`: `NotificationChannel` (`Email`, `Push`, `InApp`)
- `Subject`: `string` (max 200)
- `Body`: `string`
- `Status`: `NotificationStatus` (`Pending`, `Sent`, `Failed`)
- `CreatedAtUtc`: `DateTimeOffset`
- `SentAtUtc`: `DateTimeOffset?`

---

## 6. Automated Test Criteria (MANDATORY)

### 6.1 Unit Test Criteria
1. Template rendering: `ShipmentDispatched` correctly formats tracking link with carrier and tracking number.
2. Email validation: rejects empty or malformed email recipients.
3. Notification state machine: transition from `Pending` to `Sent` with timestamp; transition to `Failed` with reason.
4. Channel validation: ensures channel matches requested delivery format.

### 6.2 Integration Test Criteria
1. EF Core Persistence: notification entity successfully stored and retrieved from `NotificationDbContext`.
2. Customer Query Isolation: queries scoped strictly to `UserId` with descending chronological ordering.

### 6.3 Automated End-to-End (E2E) Test Scenarios
- **Scenario E2E-01**: Admin dispatches transactional order confirmation notification -> status 201 Created and persisted as Sent.
- **Scenario E2E-02**: Customer queries own notification history -> returns 200 OK with expected items.
- **Scenario E2E-03**: Customer attempts to inspect another customer's notifications -> returns 403 Forbidden (Principle V).
- **Scenario E2E-04**: Unauthenticated client attempts to send notification -> returns 401 Unauthorized.
- **Scenario E2E-05**: Admin broadcasts live order status update -> returns 200 OK with broadcasted flag and pushes to SignalR group.
- **Scenario E2E-06**: Admin broadcasts merchant sales event -> returns 200 OK; Customer attempting merchant event push is rejected with 403 Forbidden.

---

## 7. Non-Functional & Security Requirements
- **Latency**: Push broadcasts complete in < 1 second.
- **Security**: JWT authentication on all endpoints and SignalR hubs; RBAC strictly enforces `StoreAdmin` role on merchant streams.
