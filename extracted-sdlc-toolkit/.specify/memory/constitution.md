# 📜 Engineering Constitution & Quality Principles

> **Version:** 1.0.0 | **Governance:** Strict Spec-Driven Development (SDD) Compliance

This document defines the non-negotiable architectural principles, engineering standards, and quality gates for all modules and services in the project.

---

## 1. Core Architectural Principles

### Principle I: Clean Architecture & Domain Isolation (NON-NEGOTIABLE)
- Every module or service must adhere to Clean Architecture (Hexagonal / Ports & Adapters) layer boundaries:
  - **Domain**: Pure business entities, value objects, domain events, and domain exceptions. Zero dependencies on external frameworks, ORMs, HTTP abstractions, or infrastructure libraries.
  - **Application**: Use cases, command/query handlers, business workflows, validation rules, and output ports/interfaces.
  - **Infrastructure**: Implementations of database persistence, message broker clients, external APIs, and caching.
  - **Api / Presentation**: Controllers or route handlers, middleware, request/response DTOs, and serialization.
- **Dependency Inversion**: Outer layers depend on inner layers; inner layers NEVER reference outer layers.

### Principle II: Test-First & Quality Gates (NON-NEGOTIABLE)
- Every feature or module specification must define verifiable Unit, Integration, and Automated E2E test criteria before implementation starts.
- Code without automated tests will be rejected by the Review Agent and CI pipeline.
- Quality standards:
  - Domain layer: minimum 85% branch coverage.
  - Endpoints: integration test verifying success (200/201), validation errors (400), unauthorized access (401/403), and missing resources (404).

### Principle III: Asynchronous Event-Driven Decoupling
- Services or modules must not perform synchronous cross-boundary writes during transactional flows.
- Inter-service state synchronization must occur asynchronously via domain events using the **Transactional Outbox Pattern** to prevent dual-write anomalies.

### Principle IV: Zero-Trust Security & Data Isolation
- All authenticated endpoints must validate cryptographic tokens (e.g., JWT Bearer) and enforce role/permission claims.
- Multi-tenant / customer data isolation must be enforced at the repository or query level. Users must never read or modify resources belonging to another tenant or owner.
- Secrets, credentials, or sensitive PII must never be committed to source code or logged in plain text.

### Principle V: Standardized Error Handling (RFC 7807)
- APIs must return standardized RFC 7807 Problem Details for all 4xx and 5xx errors.
- Internal exception stack traces and server internals must never be leaked to public API clients.

### Principle VI: Idempotency & Concurrency Protection
- State-altering operations (creation, checkout, payment processing, status updates) must support idempotency mechanisms (e.g. `Idempotency-Key` headers) to prevent duplicate execution.
- High-concurrency operations must enforce optimistic locking or atomic atomic reservation patterns to eliminate race conditions.

---

## 2. Review Process & CI/CD Gates

1. **SDD Loop Verification**: All tasks must be validated by the autonomous Generator ⟷ Review Agent refinement loop before a PR is opened.
2. **Deterministic Build**: Compiler / linter must pass cleanly with zero warnings (`--warnaserror` or strict linter mode).
3. **Automated Test Suite**: All unit, integration, and E2E tests must execute and pass in CI with 100% pass rate.
4. **Autonomous AI Review Gate**: Every PR is audited by the Google AI PR Review Agent. Merges to `main` are strictly blocked on `❌ CHANGES REQUESTED` until all critical and major findings are resolved.
