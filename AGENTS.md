# AGENTS.md — AI Agent Guidance & Repository Rules

This document provides mandatory guidance for autonomous AI coding agents (Antigravity, Claude Code, GitHub Copilot, Cursor, Windsurf, Devin) operating in `akazad13/shopizy-microservice`.

---

## 1. System Identity & Architecture

**Product**: `Shopizy Microservices Platform` — Enterprise e-commerce microservices ecosystem built with .NET 10, Clean Architecture, DDD, MassTransit, and .NET Aspire.
- **Microservices**: Catalog, Basket, Ordering, Identity, Discount, Payment, Notification, Shipping, Inventory, Customer.
- **Framework**: .NET 10, C# 14, ASP.NET Core Minimal APIs.
- **Messaging**: RabbitMQ + MassTransit with Transactional Outbox pattern.
- **Persistence**: Database-per-Service (PostgreSQL 17), Redis caching, Redis Lua stock locks.
- **Orchestration**: .NET Aspire (`Shopizy.AppHost`).

---

## 2. Inviolable Architectural Invariants

1. **Database-Per-Service Isolation**:
   - Microservices MUST maintain isolated database contexts.
   - Cross-service table joins and direct foreign keys are strictly prohibited. Inter-service data sharing must occur via asynchronous domain events or synchronous API calls.
2. **Transactional Outbox for Events**:
   - All events published to RabbitMQ must use MassTransit's Entity Framework Core Outbox to ensure transactional consistency with local database commits.
3. **Inventory Reservation Atomicity**:
   - Stock deduction must use atomic Redis Lua scripts or database row-level versioning to eliminate race conditions during high-concurrency flash sales.
4. **Banned Packages Policy**:
   - `Moq` is prohibited (use `NSubstitute`).
   - `AutoMapper` is prohibited (use explicit mapping or compile-time generators).

---

## 3. Commands Cheat Sheet

```bash
# Build entire microservice solution
dotnet build Shopizy.sln

# Run all unit and integration tests
dotnet test Shopizy.sln

# Run Aspire orchestrator (starts all microservices + infra)
dotnet run --project src/Shopizy.AppHost/Shopizy.AppHost.csproj
```

---

## 4. Agentic Testing Paradigm & Verification (ADR-0015)

All agents MUST practice disciplined Test-Driven Development (TDD) adhering to [ADR-0015](docs/adr/0015-agentic-testing-paradigm-and-verification-loop.md):

1. **Red-Green-Refactor Loop**:
   - **RED**: Author a failing test before writing any production implementation. Verify failure diagnostic.
   - **GREEN**: Write minimal production code to satisfy the test.
   - **REFACTOR**: Clean code while keeping all tests green. Never submit unverified code.
2. **Prove-It Bug Reproduction Pattern**:
   - Never apply ad-hoc bug fixes. First write an isolated test that reproduces the defect and fails (RED). Fix the bug until the test passes (GREEN).
3. **Resource Sizing Model**:
   - **Small (Milliseconds)**: Pure in-memory unit tests, value objects, aggregate roots, domain rules, state machines, handlers. Zero I/O or containers.
   - **Medium (Seconds)**: Testcontainers for PostgreSQL, RabbitMQ, Redis. Ephemeral Docker instances cleaned via `Respawn`.
   - **Large (Minutes)**: `WebApplicationFactory<Program>` and Aspire distributed flow E2E tests for multi-service checkout/fulfillment.
4. **Testing Invariants**:
   - **State Over Interactions**: Assert observable state, output, and domain events. Do not mock internal method calls.
   - **DAMP Naming & AAA**: Structure tests with Arrange-Act-Assert. Use descriptive naming: `[UnitOfWork]_[Condition]_[ExpectedResult]`.
   - **Zero Moq & Zero AutoMapper**: Banned unconditionally. Use real fakes, stubs, or `NSubstitute` only at 3rd-party external boundaries.

### Exact Verification Commands

```bash
# Fast inner loop: run a single focused unit test
dotnet test tests/Shopizy.CatalogService.UnitTests --filter "FullyQualifiedName~CreateProduct"

# Run a specific test project (Small)
dotnet test tests/Shopizy.CatalogService.UnitTests/Shopizy.CatalogService.UnitTests.csproj

# Run an integration test suite with Testcontainers (Medium)
dotnet test tests/Shopizy.CatalogService.IntegrationTests/Shopizy.CatalogService.IntegrationTests.csproj

# Run all unit tests across the solution (fast feedback)
dotnet test Shopizy.sln --filter "Category!=Integration&Category!=E2E"

# Pre-merge gate: run all 38 test suites across the solution
dotnet test Shopizy.sln
```

---

## 5. Verification Gates for AI Agents

Before submitting code changes or finishing a turn:
1. Run `dotnet build Shopizy.sln` — must compile with **0 errors**.
2. Run `dotnet test Shopizy.sln` (or affected service test suite) — all tests must pass.
3. Verify newly implemented logic has corresponding RED-to-GREEN tests.
4. Ensure no cross-database foreign key relationships were introduced.
