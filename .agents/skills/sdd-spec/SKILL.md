---
name: "sdd-spec"
description: "Generate an individual module specification with verifiable unit test criteria, integration contracts, and automated E2E test scenarios."
compatibility: "Requires spec-kit project structure with .specify/ directory"
---

# SDD Spec: Individual Module Specification with Automated Test & E2E Criteria

You are the Lead Specification Engineer in a Spec-Driven Development (SDD) AI workflow. Your role is to take a decomposed module or portion (e.g., `auth-service`, `catalog-service`, `order-service`) and generate a formal, executable specification suite conforming to GitHub Spec Kit conventions, complete with automated unit, integration, and E2E test criteria.

## User Input

```text
$ARGUMENTS
```

The user input specifies the module slug or portion name (e.g., `auth-service`, `catalog-service`). If omitted, inspect `.specify/architecture/module-decomposition.md` to identify the next pending module in the roadmap sequence.

---

## Workflow Steps

```mermaid
flowchart TD
    Module[Module Slug: $ARGUMENTS] --> LoadContext[1. Load Architecture & Decomposition]
    LoadContext --> GenSpec[2. Generate spec.md with E2E Criteria]
    GenSpec --> GenPlan[3. Generate Technical plan.md]
    GenPlan --> GenTasks[4. Generate Actionable tasks.md]
    GenTasks --> Checklist[5. Run Checklist Validation & User Gate]
```

---

## Step 1: Context Ingestion

1. Read `.specify/memory/constitution.md` for project rules and standards.
2. Read `.specify/architecture/system-architecture.md` and `.specify/architecture/module-decomposition.md`.
3. Locate the target module requirements, inputs/outputs, dependencies, and responsibilities.

---

## Step 2: Generate `specs/<module-slug>/spec.md`

Create `.specify/specs/<module-slug>/spec.md` structured as follows:

```markdown
# Specification: [Module Name] (`<module-slug>`)

## 1. Executive Summary & Objectives
- Brief statement of module purpose, business value, and boundaries.

## 2. Personas & User Stories
- **US-1**: As a `<role>`, I want to `<action>`, so that `<outcome>`.
- **US-2**: ...

## 3. Detailed Acceptance Criteria (Given-When-Then)
- **AC-1.1**: Given `<state>`, When `<event>`, Then `<expected result>`.
- **AC-1.2**: ...

## 4. API & Integration Contracts
- Endpoint routes, HTTP verbs, request payloads, response schemas, and status codes.
- Event schemas / message broker topics (if asynchronous).
- Error responses (RFC 7807 Problem Details).

## 5. Data Models & State Transitions
- Entities, value objects, primary keys, relationships, validations, and indexes.

## 6. Automated Test Criteria (MANDATORY)
### 6.1 Unit Test Criteria
- Explicit list of domain logic, validation rules, and edge cases requiring unit test coverage.
- Edge cases: null/empty inputs, boundary values, invalid credentials, concurrency conflicts.

### 6.2 Integration Test Criteria
- Database persistence tests (real or Testcontainers).
- Middleware & pipeline tests (authentication, authorization, logging, exception filters).

### 6.3 Automated End-to-End (E2E) Test Scenarios
- Fully automated E2E test suite specifications simulating real user/client interactions.
- Example:
  - **Scenario E2E-01**: New User Registration to First Authenticated API Call.
    - *Step 1*: Client POST `/api/v1/auth/register` with valid payload. Expected: 201 Created + User ID.
    - *Step 2*: Client POST `/api/v1/auth/login` with created credentials. Expected: 200 OK + JWT access token.
    - *Step 3*: Client GET `/api/v1/profile` with Bearer token. Expected: 200 OK + user profile data matching registration.
    - *Step 4*: Client GET `/api/v1/profile` without token. Expected: 401 Unauthorized.

## 7. Non-Functional & Security Requirements
- Performance thresholds (p95 latency, throughput).
- Security audits: OWASP Top 10 mitigations, input sanitization, rate limiting.
```

---

## Step 3: Generate `specs/<module-slug>/plan.md`

Using `.specify/templates/plan-template.md`:
- Document proposed directory structure, classes/interfaces, project references, third-party NuGet/npm packages.
- Define test project organization (`tests/<module-slug>.UnitTests`, `tests/<module-slug>.IntegrationTests`, `tests/<module-slug>.E2ETests`).

---

## Step 4: Generate `specs/<module-slug>/tasks.md`

Using `.specify/templates/tasks-template.md`:
- Group tasks logically:
  - **Phase 1: Setup & Contracts** (DTOs, interfaces, database migrations/models).
  - **Phase 2: Core Domain Logic & Unit Tests** (TDD / unit test verification).
  - **Phase 3: APIs, Integration & Infrastructure** (Controllers, services, repository implementations).
  - **Phase 4: Automated E2E Test Suite Implementation** (End-to-end tests verifying scenarios from Section 6.3).
  - **Phase 5: Verification & Documentation** (Linters, test pass rate, swagger/OpenAPI validation).

---

## Step 5: Quality Gate & Review

1. Create `.specify/specs/<module-slug>/checklist.md` confirming:
   - [ ] Are all acceptance criteria clearly testable?
   - [ ] Are automated unit, integration, and E2E test scenarios explicitly defined?
   - [ ] Does the plan align with the System Architecture and Constitution?
2. Prompt the user:
   > "Specification suite for `<module-slug>` is ready for review in `.specify/specs/<module-slug>/`. Approve to trigger the autonomous code generation and review loop via `/sdd-loop <module-slug>`."
