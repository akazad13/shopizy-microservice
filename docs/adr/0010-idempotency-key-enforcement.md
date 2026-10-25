# 0010. Idempotency-Key Enforcement for Mutating Endpoints

- **Status:** Accepted
- **Date:** 2026-09-05

## Context

Network instability, browser retries, client timeouts, and automated message broker redeliveries can cause duplicate mutating requests. In financial and commerce operations (such as charging a card, placing an order, decrementing stock, or issuing a refund), executing a request twice causes double-billing, duplicate order creation, or phantom inventory deductions.

## Decision

- Mandate the `Idempotency-Key` header on all mutating and financial API endpoints (`POST /api/v1/orders`, `POST /api/v1/payments`, `POST /api/v1/refunds`):
  - Clients generate a unique v4 UUID and pass it in the `Idempotency-Key` HTTP header.
  - An ASP.NET Core middleware / filter checks Redis for the given key before processing:
    1. If the key is new: Store a lock record in Redis with a 60-second TTL and proceed with the request.
    2. If the key exists with status `InFlight`: Return `409 Conflict` to prevent concurrent execution of the same operation.
    3. If the key exists with status `Completed`: Immediately return the cached HTTP status code and response body without re-executing any business logic or publishing domain events.
- Message consumers (MassTransit) also maintain an idempotency table / deduplication window to safely ignore redelivered event messages.

## Consequences

- Completely protects customers from double-charges and duplicate order placement during mobile reconnects or aggressive button clicking.
- Redis serves as the fast idempotency registry, introducing minimal overhead (~1ms) per mutating request.
- Clients are responsible for generating and preserving UUID idempotency keys across retries.
