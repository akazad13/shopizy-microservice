# 0008. 15-Minute Unpaid Order Auto-Expiration and Restocking

- **Status:** Accepted
- **Date:** 2026-09-05

## Context

When an order is created, inventory must be reserved immediately to prevent other shoppers from buying the same stock. However, if a customer navigates away, closes their browser, or abandons payment, reserved stock would remain locked indefinitely unless released. This leads to artificial stock-outs ("phantom out-of-stock") where inventory appears unavailable to prospective buyers even though it was never paid for.

## Decision

- Mandate a strict **15-minute expiration deadline** on all unpaid orders:
  - When an order is placed, it enters the `PendingPayment` state with an explicit `ExpiresAtUtc` timestamp (`CreatedAtUtc + 15 minutes`).
  - `OrderService` schedules an expiration message via MassTransit Scheduled Messages / Quartz scheduler (`SchedulePublish<OrderExpiredEvent>(ExpiresAtUtc)`).
  - When payment succeeds within the 15-minute window, the order transitions to `Processing`, and the expiration message is superseded or ignored by consumer state checks.
  - If the 15-minute window lapses without payment confirmation, the consumer triggers `CancelUnpaidOrderCommand`:
    - Reverts relational reserved stock in PostgreSQL (`UPDATE ProductStock SET Reserved = Reserved - @qty`).
    - Increments in-memory Redis inventory counter via atomic Lua script.
    - Transitions order status to `Cancelled` with cancellation reason `PaymentTimeout`.
    - Publishes `OrderCancelledEvent` to RabbitMQ to notify downstream services.

## Consequences

- Prevents inventory lockup caused by abandoned checkout flows; unsold stock returns to the pool within 15 minutes.
- Checkout UI displays a live countdown timer so customers are aware of the reservation window.
- Consumers of `OrderExpiredEvent` must verify the order has not already transitioned to `Processing` or `Paid` before executing cancellations.
