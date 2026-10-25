# 0004. Database-per-Service and Shared-Nothing Isolation

- **Status:** Accepted
- **Date:** 2026-09-05

## Context

A shared database among microservices creates tight schema coupling, distributed lock contention, cross-service cascading outages, and obscures service domain boundaries. If one service queries or modifies tables owned by another service, independent deployment and database schema evolution become impossible.

## Decision

- Enforce a strict **Database-per-Service** and **Shared-Nothing** architecture across all 13 microservices:
  - Each microservice possesses its own dedicated, isolated PostgreSQL 17 database (`identitydb`, `catalogdb`, `orderdb`, `paymentdb`, `promotiondb`, `shippingdb`, `notificationdb`, `reviewdb`, `loyaltydb`, `abandonmentdb`).
  - **Zero Cross-Database Joins or Foreign Keys**: Foreign key constraints cannot span service database boundaries.
  - Inter-service entity associations (such as `CustomerId` in `Order` or `ProductId` in `Review`) are stored as scalar identifier value objects (`Guid` or `long`), without direct database relationships.
  - Data retrieval across domains must occur via asynchronous event projections (e.g., Catalog syncing data to Search via RabbitMQ) or read-optimized CQRS views, never via cross-database SQL queries.

## Consequences

- Each service can run database migrations, backups, and point-in-time restores completely independently of other services.
- Eliminates noisy-neighbor connection pool exhaustion between unrelated services (e.g., intensive search indexing cannot starve checkout transaction pools).
- Eliminates schema coupling; changing `Catalog` schema has zero impact on `Order` or `Review` databases.
- Requires managing eventual consistency and distributed event choreography for multi-service data aggregation.
