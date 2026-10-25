# 0012. Real-Time Telemetry via SignalR and Redis Backplane

- **Status:** Accepted
- **Date:** 2026-09-05

## Context

Customers expect immediate live feedback on their order lifecycle (`Pending` -> `Confirmed` -> `Processing` -> `Shipped` -> `Delivered`) without repeatedly refreshing their browser or polling HTTP endpoints. Simultaneously, merchant administrators require a real-time stream of incoming orders, inventory alerts, and revenue metrics on the admin dashboard. Traditional HTTP polling creates unnecessary server load, high database query volume, and delayed UI state updates.

## Decision

- Adopt **ASP.NET Core SignalR** with a **Redis Scale-Out Backplane** in `Shopizy.NotificationService`:
  - `NotificationService` exposes WebSocket endpoints:
    - `/hubs/order-tracking`: Authenticated hub channel scoped to customer ID (`Clients.User(customerId).SendAsync(...)`), streaming live order status and shipment milestone updates.
    - `/hubs/admin-telemetry`: Role-authorized hub channel scoped to store administrators (`Clients.Group("StoreAdmins").SendAsync(...)`), pushing live sales figures, low-stock warnings, and order counts.
  - `NotificationService` subscribes to RabbitMQ integration events (`PaymentCompletedEvent`, `ShipmentDispatchedEvent`, `TrackingUpdatedEvent`) and instantly translates them into targeted SignalR WebSocket frames with latency <1 second.
  - Use **Redis as the distributed backplane** (`AddStackExchangeRedis(...)`) to support horizontal scale-out across multiple container replicas without losing WebSocket connections or broadcast messages.

## Consequences

- Delivers a snappy, modern user experience with sub-second live order tracking and instant admin dashboard metric streams.
- Eliminates recurring polling traffic against the API Gateway and backend services.
- Requires maintaining persistent WebSocket connections through reverse proxies and load balancers (ensuring WebSockets or Server-Sent Events are enabled in gateway configurations).
