# 0005. Asynchronous Messaging with RabbitMQ and MassTransit

- **Status:** Accepted
- **Date:** 2026-09-05

## Context

Microservices need to communicate state changes to other services (e.g. when an order is placed, stock must be reserved, payments processed, notifications dispatched, and search indices refreshed). Synchronous REST calls between services introduce tight temporal coupling, latency amplification, vulnerability to cascading network failures, and dual-write data anomalies if one service fails mid-flow.

## Decision

- Adopt **RabbitMQ** as the primary message broker and **MassTransit** as the distributed application bus:
  - Synchronous cross-service writes during transactional business flows are strictly prohibited.
  - State changes are broadcast as immutable integration events (`OrderPlacedEvent`, `PaymentCompletedEvent`, `ShipmentDispatchedEvent`, etc.).
  - Message contracts are defined in `Shopizy.SharedKernel` (or a dedicated integration contracts namespace) to guarantee schema compatibility across services.
  - Exchange naming follows entity-contract conventions (`shopizy.events:order-placed`), and queue names are prefixed by consumer service name (`order-service.order-placed`).
  - MassTransit in-memory test harness (`AddMassTransitTestHarness`) is used for fast unit and component test execution without requiring a live RabbitMQ container.
  - Production consumers configure exponential backoff retry policies and Dead Letter Queues (DLQ) for poisoned messages.

## Consequences

- Producers and consumers are completely decoupled in time and availability; if `NotificationService` is restarting, `OrderService` continues taking orders without interruption.
- Eventual consistency is introduced across service boundaries; user interfaces must accommodate asynchronous processing via live polling or WebSockets.
- MassTransit provides built-in message serialization, telemetry, circuit breakers, and OpenTelemetry activity propagation across message boundaries.
