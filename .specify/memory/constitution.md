# 📜 Shopizy Microservices Platform — Constitution

> **Version:** 1.0.0 | **Ratified:** 2026-09-05 | **Governance:** Strict SDD Compliance

This document defines the non-negotiable architectural principles, engineering standards, and quality gates for all microservices in the Shopizy platform.

---

## 1. Core Principles

### Principle I: Clean Architecture & Domain Isolation (NON-NEGOTIABLE)
- Every microservice must adhere to Clean Architecture (or Hexagonal Architecture) layer boundaries:
  - **Domain**: Pure business entities, value objects, domain events, and domain exceptions. Zero dependencies on external frameworks, ORMs, or ASP.NET Core.
  - **Application**: Use cases, CQRS commands/queries (MediatR), validation rules (FluentValidation), and interfaces/ports.
  - **Infrastructure**: Implementations of database persistence (EF Core), message publishing (MassTransit), external APIs, and caching.
  - **Api / Presentation**: Controllers or Minimal API endpoints, middleware, filters, and DTO contracts.
- **Dependency Inversion**: Outer layers depend on inner layers; inner layers NEVER reference outer layers.

### Principle II: Zero Overselling & Atomic Inventory Protection
- Stock reservation must occur atomically at the exact moment of order placement.
- Unpaid orders must be subject to an automatic 15-minute expiration deadline.
- Stock restock/release logic must be idempotent and triggered automatically when an order cancels or expires.

### Principle III: Asynchronous Event-Driven Decoupling
- Services must not perform synchronous cross-service writes during transactional business flows.
- State synchronization between services (e.g. Catalog -> Search, Order -> Shipping, Payment -> Order) must occur asynchronously via **RabbitMQ domain events** using the **Transactional Outbox Pattern** to prevent dual-write anomalies.

### Principle IV: Strict Test-First & Quality Gates (NON-NEGOTIABLE)
- Every feature or module specification must define verifiable Unit, Integration, and Automated E2E test criteria before implementation starts.
- Code without automated tests will be rejected by the Review Agent and CI pipeline.
- Quality standards:
  - Domain layer: minimum 85% branch coverage.
  - Endpoints: integration test verifying success (200/201), validation errors (400), unauthorized access (401/403), and missing resources (404).

### Principle V: Customer Data Isolation & Zero Trust Security
- All authenticated requests must carry a valid cryptographic JWT Bearer token.
- Multi-customer data isolation must be enforced at the repository/query level. Customers can never read, modify, or infer carts, addresses, or orders belonging to another user.
- PCI Compliance: Sensitive credit card numbers and CVVs must never touch or be stored on Shopizy servers or databases.

### Principle VI: Idempotency & Duplicate Prevention
- Financial and state-altering endpoints (order placement, payment capture, refunds) must require and validate an `Idempotency-Key` header.
- Repeated requests with the same key must return the recorded response without executing side effects twice.

### Principle VII: Database-per-Service & Shared-Nothing Isolation
- Each microservice possesses an isolated PostgreSQL database. Cross-database queries and foreign keys across service boundaries are strictly forbidden.
- Data synchronization must occur asynchronously through MassTransit domain events with the Transactional Outbox pattern.

### Principle VIII: High-Throughput Hot-Key Inventory Protection
- High-concurrency inventory reservation must leverage atomic Redis Lua scripts to prevent relational table/row locking under flash-sale spikes, backed by durable PostgreSQL Outbox reconciliation.

---

## 2. Technology & Language Standards

- **Runtime**: .NET 10 / C# 14
- **Distributed Orchestration**: .NET Aspire 10 (`Shopizy.AppHost` for local inner loop, container provisioning, and service discovery; `Shopizy.ServiceDefaults` for standard OTel resilience and health checks).
- **ORM & Database**: Entity Framework Core 10 on PostgreSQL 17 with snake_case naming conventions and code-first migrations.
- **Messaging**: MassTransit with RabbitMQ transport. Message contracts stored in a shared contracts package or namespace.
- **Logging & Tracing**: Serilog with structured JSON output, OpenTelemetry metrics and activity tracing, correlation ID propagation via HTTP headers and message headers.
- **Real-Time**: ASP.NET Core SignalR with Redis backplane.

---

## 3. Review Process & Quality Gates

1. **SDD Loop Verification**: All tasks must be validated by the autonomous Generator ⟷ Review Agent refinement loop before a PR is opened.
2. **Deterministic Build**: `dotnet build --warnaserror` must pass cleanly without warnings.
3. **Automated Test Suite**: All unit, integration, and contract tests must execute and pass in CI with zero flaky tests.

---

**Version**: 1.0.0 | **Ratified**: 2026-09-05 | **Last Amended**: 2026-09-05
