# Architecture Decision Records (ADRs)

This directory maintains the **Architecture Decision Records (ADRs)** for the **Shopizy Microservices Platform**. 

Architecture Decision Records capture significant, architectural choices made during the design and evolution of the platform. While specifications (`.specify/specs/`) define **what** is built and how it behaves, ADRs document **why** architectural tradeoffs were selected.

---

## Index of Architectural Decisions

| ADR | Title | Status | Date | Area |
| :---: | :--- | :---: | :---: | :--- |
| [0001](0001-record-architecture-decisions.md) | Record Architecture Decisions | Accepted | 2026-09-05 | Governance |
| [0002](0002-orchestration-and-observability-with-dotnet-aspire.md) | Orchestration and Observability with .NET Aspire | Accepted | 2026-09-05 | Infrastructure |
| [0003](0003-clean-architecture-and-domain-driven-design.md) | Clean Architecture and Domain-Driven Design per Service | Accepted | 2026-09-05 | Architecture |
| [0004](0004-database-per-service-and-data-isolation.md) | Database-per-Service and Shared-Nothing Isolation | Accepted | 2026-09-05 | Persistence |
| [0005](0005-asynchronous-messaging-with-rabbitmq-and-masstransit.md) | Asynchronous Messaging with RabbitMQ and MassTransit | Accepted | 2026-09-05 | Messaging |
| [0006](0006-transactional-outbox-and-distributed-sagas.md) | Transactional Outbox and Distributed Sagas | Accepted | 2026-09-05 | Consistency |
| [0007](0007-two-tier-inventory-reservation-and-flash-sale-protection.md) | Two-Tier Inventory Reservation and Flash-Sale Hot-Key Protection | Accepted | 2026-09-05 | Concurrency |
| [0008](0008-fifteen-minute-unpaid-order-expiration.md) | 15-Minute Unpaid Order Auto-Expiration and Restocking | Accepted | 2026-09-05 | Lifecycle |
| [0009](0009-stateless-jwt-auth-and-customer-data-isolation.md) | Stateless Asymmetric JWT Authentication and Query-Level Isolation | Accepted | 2026-09-05 | Security |
| [0010](0010-idempotency-key-enforcement.md) | Idempotency-Key Enforcement for Mutating Endpoints | Accepted | 2026-09-05 | Reliability |
| [0011](0011-read-optimization-and-search-engine-offloading.md) | Read Scalability via CQRS and Search Engine Offloading | Accepted | 2026-09-05 | Performance |
| [0012](0012-real-time-telemetry-via-signalr-and-redis-backplane.md) | Real-Time Telemetry via SignalR and Redis Backplane | Accepted | 2026-09-05 | Real-Time |
| [0013](0013-testing-pyramid-and-testcontainers-integration.md) | Testing Pyramid and Testcontainers for Integration Verification | Accepted | 2026-09-05 | Quality |

---

## Anatomy of an ADR

Every ADR in this directory adheres to the standard format:
1. **Title**: Numbered sequence and concise decision name.
2. **Status**: `Draft`, `Accepted`, `Rejected`, or `Superseded by NNNN`.
3. **Date**: Date of decision ratification.
4. **Context**: Technical forces, constraints, business motivations, and problem statement.
5. **Decision**: Concrete architectural choice and enforcement invariants.
6. **Consequences**: Positive tradeoffs, operational overhead, and accepted risks.
