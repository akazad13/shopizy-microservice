# 0014. Solution Package Modernization and Runtime Dependency Alignment

- **Status:** Accepted
- **Date:** 2026-11-07
- **Area:** Infrastructure & Quality

## Context

The Shopizy Microservices platform comprises distributed services (.NET Aspire, RabbitMQ, MassTransit, EF Core, Redis, ElasticSearch, and Testcontainers). To maintain maximum security posture, performance, and tooling compatibility across the distributed microservices ecosystem, dependencies were upgraded across all project files.

## Decision

1. **Modernize Distributed Stack Packages**:
   - Upgraded .NET Aspire orchestration and service defaults packages.
   - Upgraded MassTransit messaging, outbox, and RabbitMQ transports.
   - Upgraded Entity Framework Core (PostgreSQL Npgsql provider) and Redis cache multiplexers.
   - Upgraded Testcontainers and xUnit/FluentAssertions testing harnesses.
2. **Verify System-Wide Integrity**:
   - Verify that distributed sagas, outbox publishers, and idempotency key middleware behave consistently without contract breaks.
   - Confirm Docker Testcontainers integration tests continue to run cleanly against containerized PostgreSQL, RabbitMQ, and Redis dependencies.

## Consequences

- Services benefit from runtime performance enhancements and security patches in .NET, Aspire, and MassTransit.
- No changes required to external HTTP/REST or messaging event schema contracts.
