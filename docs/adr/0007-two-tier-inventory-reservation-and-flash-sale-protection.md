# 0007. Two-Tier Inventory Reservation and Flash-Sale Hot-Key Protection

- **Status:** Accepted
- **Date:** 2026-09-05

## Context

In high-volume e-commerce platforms, flash-sale events (e.g. 10,000 users attempting to buy 100 units of a discounted product simultaneously) cause severe hot-key contention. If each checkout attempt issues a relational database lock (`SELECT ... FOR UPDATE` or conditional update against PostgreSQL), connection pools exhaust immediately, database CPU spikes to 100%, and deadlocks cause catastrophic request drops. Conversely, relying solely on in-memory counters risks inventory overselling if an in-memory node restarts before persisting to disk.

## Decision

- Implement a **Two-Tier Inventory Reservation Architecture**:
  1. **Hot In-Memory Tier (Redis Lua Scripts)**:
     - High-contention inventory availability is cached in Redis with an atomic Lua script:
       ```lua
       local available = redis.call('get', KEYS[1])
       if not available or tonumber(available) < tonumber(ARGV[1]) then
           return 0
       else
           redis.call('decrby', KEYS[1], ARGV[1])
           return 1
       end
       ```
     - If the Lua script returns `0`, the request is immediately rejected at the API boundary in sub-millisecond latency without touching PostgreSQL.
  2. **Durable Relational Tier (PostgreSQL Outbox)**:
     - Upon successful Redis decrement, `OrderService` commits an atomic reservation row in PostgreSQL within an ACID transaction (`UPDATE ProductStock SET Reserved = Reserved + @qty WHERE Available >= @qty`).
     - Publishes `OrderPlacedEvent` via the Transactional Outbox.
  3. **Automated Compensation & Restock**:
     - If the relational commit fails or times out, a compensating Redis Lua script increments the in-memory counter back (`incrby`).

## Consequences

- 100% protection against overselling; available stock cannot drop below zero.
- PostgreSQL database is shielded from thousands of failing requests per second during flash sales; only valid purchases hit the database.
- Requires keeping Redis and PostgreSQL inventory numbers reconciled via a periodic background audit job.
