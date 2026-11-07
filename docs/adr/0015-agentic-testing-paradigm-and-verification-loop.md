# 0015. Agentic Testing Paradigm and Continuous Verification Loop

- **Status:** Accepted
- **Date:** 2026-11-07
- **Area:** Quality & Engineering Standards

## Context

In an enterprise distributed microservices platform spanning 13 services (.NET 10, Aspire, RabbitMQ/MassTransit, PostgreSQL, Redis), development is increasingly driven by autonomous AI coding agents (Antigravity, Claude Code, GitHub Copilot) alongside human engineers.

While ADR-0013 defined the foundational Testing Pyramid and Testcontainers infrastructure, agentic development introduces unique risks:
1. **Hallucinated or False-Confidence Tests:** Agents frequently declare victory based on unverified code changes, write assertions that pass unconditionally, or write tests *after* implementation that only assert implementation details rather than domain behavior.
2. **Untracked Regressions & Shotgun Debugging:** When bugs are reported, agents often guess fixes and iterate randomly rather than creating deterministic reproductions.
3. **Over-Mocking & Interaction Coupling:** Relying heavily on mocking frameworks (e.g., verifying `mock.Verify(x => x.SaveAsync(), Times.Once)`) creates brittle tests that break during refactoring even when state behavior is correct. Furthermore, `Moq` historically introduced supply-chain and telemetry concerns.
4. **Indiscriminate Test Execution:** Running the entire 38-project suite for single-line domain edits creates prohibitive latency in agent feedback loops, while running no integration tests risks missing schema and messaging breaks.

A formal **Agentic Testing Paradigm and Verification Loop** is required to codify how human and AI agents write, verify, and maintain tests across the codebase.

---

## Decision

We establish the **Agentic Testing Paradigm** grounded in Test-Driven Development (TDD), the Prove-It bug reproduction pattern, strict resource-tiered execution, and state-focused invariants.

### 1. The Agentic TDD Workflow (Red-Green-Refactor)

All behavioral modifications and new feature implementations MUST adhere to the strict Red-Green-Refactor cadence:

```
    ┌──────────────┐         ┌──────────────┐         ┌──────────────┐
    │     RED      │         │    GREEN     │         │   REFACTOR   │
    │ Write failing│ ──────> │Write minimal │ ──────> │Clean up code │
    │     test     │         │code to pass  │         │keep tests    │
    └──────────────┘         └──────────────┘         │    green     │
           │                        │                 └──────────────┘
           ▼                        ▼                        │
       Test FAILS              Test PASSES                   ▼
  (proves requirement)      (proves solution)          (repeat loop)
```

1. **RED (Failing Test First):**
   - Write an executable test specifying the expected behavior before writing the production logic.
   - Execute the test and verify that it fails with an expected diagnostic (not a compilation error or fixture fault). A test that passes initially proves nothing.
2. **GREEN (Minimal Implementation):**
   - Author the minimum production code required to satisfy the failing test.
   - Do not prematurely over-engineer or add speculative abstractions.
3. **REFACTOR (Clean Without Regressions):**
   - Simplify code, remove duplication, and enhance readability while ensuring all existing tests remain green.

> **The Beyonce Rule:** *"If you liked it, you should have put a test on it."* Infrastructure changes, refactoring, and dependency upgrades are never responsible for catching uncaught bugs; test coverage is.

---

### 2. The Prove-It Bug Reproduction Pattern

For bug reports, regressions, or production defects, agents MUST NOT jump directly into implementation. All fixes follow the **Prove-It Pattern**:

```
Bug Report / Defect Arrives
       │
       ▼
1. Author Reproduction Test (Small / Medium)
       │
       ▼
2. Execute Test ──> Test FAILS (RED)
       │            (Confirms defect existence & root cause)
       ▼
3. Implement Targeted Fix
       │
       ▼
4. Execute Test ──> Test PASSES (GREEN)
       │            (Proves defect resolved)
       ▼
5. Execute Service Test Suite (Regression Guard)
```

1. **Reproduce First:** Write a focused test that isolates and exhibits the reported defect under matching conditions.
2. **Confirm RED:** Run the test to verify failure, proving the bug exists and confirming root cause.
3. **Resolve to GREEN:** Implement the fix until the reproduction test passes cleanly.
4. **Guard Against Regressions:** The reproduction test becomes a permanent regression gate in the repository test suite.

---

### 3. Resource Model: Test Sizing & Execution Budgets

Tests are categorized strictly by execution budget and resource consumption:

| Size | Scope & Constraints | Speed Target | Microservice Examples |
|:---|:---|:---:|:---|
| **Small** | Single process, in-memory only. Zero network, disk I/O, or external containers. Pure logic. | Milliseconds (< 100ms) | Domain entities (`Product`, `Order`), Value Objects (`Money`), aggregate invariants, pure command/query handlers, state machines, MassTransit saga state transforms. |
| **Medium** | Multi-process / containerized dependencies. Ephemeral Docker containers via Testcontainers. Zero external clouds. | Seconds (1s - 8s) | PostgreSQL repositories via EF Core, database migrations, Redis caching / Lua scripts, RabbitMQ MassTransit consumer harnesses, `Respawn` state resets. |
| **Large** | Distributed system orchestration. Multi-service coordination. | Minutes (< 2 min) | Full ASP.NET Core `WebApplicationFactory<Program>` HTTP pipeline tests, Aspire AppHost orchestration flows, end-to-end checkout & payment authorization journeys. |

#### Agent Execution Protocol:
- **Inner Loop (Active TDD):** Run only the focused test or single test project (`dotnet test tests/Shopizy.<Service>.UnitTests --filter "..."`).
- **Pre-Commit Verification:** Run the affected service's unit and integration test suites.
- **Solution Gate:** Run full solution verification (`dotnet test Shopizy.sln`) prior to branch merge.

---

### 4. Testing Invariants

#### A. State Assertion over Interaction Mocks
- Assert on observable outcomes, return values, database state, and published domain events.
- **Avoid Interaction Mocking:** Do not verify internal method call counts, call sequences, or mock private implementation details. Interaction assertions couple tests to volatile implementation details and prevent safe refactoring.

#### B. DAMP over DRY Test Structure
- Unit and integration tests prioritize **DAMP (Descriptive And Meaningful Phrases)** over DRY (Don't Repeat Yourself).
- Each test must tell a self-contained story without requiring readers to decipher complex inheritance hierarchies or hidden shared test setups.
- Use explicit Arrange-Act-Assert (AAA) formatting.
- Naming convention: `[UnitOfWork]_[Condition/Scenario]_[ExpectedResult]` (e.g., `ReserveStockAsync_WhenQuantityExceedsAvailable_ReturnsInsufficientStockResult`).

#### C. Zero Moq / Zero AutoMapper Invariant
- **Moq is Strictly Prohibited:** Use real domain fakes, in-memory stubs, or `NSubstitute` solely at external third-party boundaries (such as payment gateways or third-party carrier APIs).
- **AutoMapper is Strictly Prohibited:** All DTO-to-entity mappings must be explicit static factory methods or compile-time mapping generators (Mapperly). Explicit mappings eliminate runtime reflection errors and simplify direct assertion in unit tests.

---

## Consequences

### Positive
- AI agents operate with deterministic verification gates, preventing hallucinated implementations and silent regressions.
- Test suites remain resilient to refactoring because assertions focus on observable state contracts rather than internal call graphs.
- Fast feedback inner loops (Small tests running in milliseconds) preserve agent efficiency and token economy while Medium/Large tests safeguard distributed consistency.
- Eliminate risks associated with deprecated or problematic mocking libraries through clear banning policies and preference for real containers.

### Negative / Trade-offs
- Requiring a failing reproduction test before bug fixing requires discipline and upfront investment.
- Medium and Large tests require a local Docker runtime for Testcontainers and Aspire verification.