# 0013. Testing Pyramid and Testcontainers for Integration Verification

- **Status:** Accepted
- **Date:** 2026-09-05

## Context

In an enterprise microservices ecosystem with 13 services, automated tests are the only reliable safeguard against regressions. However, testing strategies frequently fail when:
1. Over-relying on in-memory fake databases (like EF Core InMemory provider) which do not behave like real PostgreSQL (missing constraint checks, transaction behaviors, SQL syntax differences).
2. Relying on shared persistent testing databases that suffer from test pollution, data collision, and order-dependent test failures.
3. Writing slow, brittle E2E tests for everything instead of balanced testing across the test pyramid.

## Decision

- Enforce a strict **Testing Pyramid** supported by **Testcontainers** across every microservice:
  1. **Unit Tests (`*.UnitTests`)**:
     - Test pure domain models, aggregate roots, value objects, domain rules, and command/query handlers.
     - Fast execution (milliseconds) with zero external I/O. Use Moq and FluentAssertions.
     - Minimum 85% branch coverage required for domain logic.
  2. **Integration Tests (`*.IntegrationTests`)**:
     - Test EF Core repositories, database migrations, MassTransit consumers, and external adapters against real containerized infrastructure.
     - Use **Testcontainers for .NET** (`Testcontainers.PostgreSql`, `Testcontainers.RabbitMq`, `Testcontainers.Redis`) to spin up disposable, ephemeral Docker containers per test run.
     - Database state is cleaned up between test cases using `Respawn` to guarantee 100% test independence and zero data contamination.
  3. **End-to-End & API Contract Tests (`*.E2ETests`)**:
     - Use `WebApplicationFactory<Program>` to test full HTTP request-response pipelines, status codes (200, 201, 400, 401, 403, 404), serialization contracts, and authentication filters.
     - Multi-persona user journeys (Customer checkout, StoreAdmin fulfillment) verified with automated test suites.

## Consequences

- Tests execute against production-identical PostgreSQL and RabbitMQ instances, catching constraint violations, concurrency issues, and migration bugs before deployment.
- Tests can run on any local developer workstation or CI runner with Docker installed without manual database seeding or configuration.
- Integration tests take longer to start (~2-5 seconds container initialization), which is mitigated by running unit test suites first in fast feedback loops.
