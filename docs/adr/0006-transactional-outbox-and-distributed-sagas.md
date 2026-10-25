# 0006. Transactional Outbox and Distributed Sagas

- **Status:** Accepted
- **Date:** 2026-09-05

## Context

When a microservice persists state to its local database and attempts to publish an event to RabbitMQ, a dual-write failure can occur:
1. The database commit succeeds, but the network or broker fails before the message is published (leaving the event lost and downstream services out of sync).
2. The message is published, but the local transaction fails to commit (leaving downstream services acting on a ghost transaction).
Additionally, multi-service workflows (such as order placement, payment capture, and fulfillment) span multiple independent databases and cannot rely on distributed 2-Phase Commit (2PC) transactions due to performance and availability penalties.

## Decision

- Mandate the **Transactional Outbox Pattern** for all state-changing events:
  - Both domain entity state and outbound event envelopes are committed to the microservice's local PostgreSQL database inside a single atomic ACID transaction.
  - MassTransit's native EF Core Outbox integration (`AddEntityFrameworkOutbox<TDbContext>`) publishes messages asynchronously from the database to RabbitMQ once committed.
- Coordinate long-running, multi-service workflows using **Choreographed & Orchestrated MassTransit Sagas**:
  - `OrderService` acts as the saga orchestrator for checkout transactions (`PendingPayment` -> `Processing` -> `Shipped` -> `Delivered`).
  - Compensating actions are registered for failure paths: if `PaymentFailedEvent` or `OrderExpiredEvent` is consumed, the saga triggers compensating commands to revert inventory reservations and notify the customer.

## Consequences

- Guarantees **at-least-once message delivery**; no domain events can be lost due to network or broker glitches during persistence.
- Consumers must be idempotent to handle occasional message redeliveries safely (governed by ADR 0010).
- Eliminates synchronous distributed transaction locks, preserving high transaction throughput and system availability.
