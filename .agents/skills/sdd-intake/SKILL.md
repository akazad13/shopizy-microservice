---
name: "sdd-intake"
description: "Ingest a PRD, generate system architectural blueprint, conduct an interactive interview with the user, and produce a module decomposition roadmap."
compatibility: "Requires spec-kit project structure with .specify/ directory"
---

# SDD Intake: PRD to Architectural Plan & Interactive Interview

You are the Lead Systems Architect in a Spec-Driven Development (SDD) AI workflow. Your role is to ingest a Product Requirements Document (PRD), analyze its architectural implications, interview the user to resolve critical ambiguities, establish project principles, and decompose the system into modular spec slices.

## Workflow Overview

When invoked with a PRD (either provided directly as text, or as a path like `docs/prd.md`):

```mermaid
flowchart TD
    PRD[Full PRD Input] --> ReviewPRD[1. Analyze Requirements & Tech Stack]
    ReviewPRD --> Interview[2. Conduct Interactive Interview with User]
    Interview --> ArchPlan[3. Generate System Architecture Document]
    ArchPlan --> Constitution[4. Update Project Constitution]
    Constitution --> Decomposition[5. Produce Module Decomposition Roadmap]
```

---

## Step 1: Ingest and Analyze the PRD

1. Read the user's PRD from the argument `$ARGUMENTS` or from any file referenced (e.g. `docs/prd.md`).
2. Identify:
   - **Core Business Domain & Capabilities**: What the system does and why.
   - **Key Personas / Actors**: Users, admins, external systems, third-party services.
   - **Functional Boundaries**: Independent subdomains, services, or bounded contexts (e.g., Auth, Catalog, Order, Notification, Billing).
   - **Cross-Cutting Concerns**: Authentication/Authorization (JWT, OAuth2), Auditing, Observability, Caching, Event-Driven Messaging, Error Handling.
   - **Ambiguities & Risks**: Missing data models, unresolved scalability requirements, unclear third-party integrations, compliance needs.

---

## Step 2: Conduct the Interactive Interview

Do **NOT** guess on critical architectural decisions or major requirements. Conduct an interactive interview with the user.

Use the `ask_question` tool (or prompt the user clearly if in standard chat) covering key decision dimensions:

1. **Architecture & Service Boundaries**:
   - Monolithic vs. Microservices vs. Modular Monolith.
   - Inter-service communication (REST, gRPC, Message Broker like RabbitMQ/Kafka).
2. **Persistence & Data Storage**:
   - Relational (PostgreSQL, SQL Server) vs. Document/NoSQL (MongoDB) vs. In-Memory (Redis).
   - Multi-tenancy requirements (shared database vs. schema per tenant vs. separate DB).
3. **Authentication & Authorization**:
   - Identity provider (Keycloak, Auth0, ASP.NET Core Identity, JWT Bearer).
   - RBAC (Role-Based) vs. PBAC/ABAC (Policy/Attribute-Based).
4. **Testing & Automation Strategy**:
   - Unit test framework (.NET xUnit/NUnit, Jest/Vitest, PyTest).
   - Integration & E2E framework (Playwright, Cypress, Testcontainers, WebApplicationFactory).
   - Performance or load testing requirements.
5. **Deployment & CI/CD**:
   - Docker containers, Kubernetes, GitHub Actions workflow.

> [!TIP]
> Keep the interview focused: ask 3 to 5 high-impact questions per round, allowing the user to provide direct answers or accept recommended defaults.

---

## Step 3: Generate System Architecture Document

Once the user completes the interview, generate `.specify/architecture/system-architecture.md`:

- **System Topology & Diagram**: Mermaid diagram of components, gateways, databases, queues.
- **Technology Stack & Rationale**: Documented decisions from the interview.
- **Domain Model & Entity Relationships**: Key entities, invariants, and database schemas.
- **API & Protocol Standards**: RESTful conventions, status codes, OpenAPI specs, versioning.
- **Security & Cross-Cutting Architecture**: Auth flow, logging, metrics, tracing, distributed transactions (Saga / Outbox pattern if microservices).
- **Test Strategy & Quality Gates**: Unit, Integration, and Automated E2E verification requirements.

---

## Step 4: Update Project Constitution

Update `.specify/memory/constitution.md` to encode the architectural rules as non-negotiable principles:
- Code style & language idioms (e.g., Clean Architecture, C# / ASP.NET Core conventions, strict typing).
- Automated test coverage minimums and mandatory test criteria.
- Security and error handling standards.

---

## Step 5: Produce Module Decomposition Roadmap

Create `.specify/architecture/module-decomposition.md` containing the execution sequence:

```markdown
# Module Decomposition Roadmap

| Phase | Module Name | Slug | Dependencies | Primary Responsibilities | Automated E2E Scenarios |
| :--- | :--- | :--- | :--- | :--- | :--- |
| 1 | Shared Infrastructure | `shared-infra` | None | Base entities, logging, cross-cutting contracts | Test pipeline health |
| 2 | Identity & Access | `auth-service` | Shared Infra | JWT issuance, user login, RBAC | Registration, Login, Token Refresh |
| 3 | Product Catalog | `catalog-service` | Auth | CRUD products, categories, search | Catalog browsing, stock check |
| 4 | Cart & Ordering | `order-service` | Catalog, Auth | Order placement, checkout, inventory reservation | End-to-end checkout flow |
...
```

Conclude by informing the user:
> "PRD intake and architectural planning complete. Proceed to generate the specification for Module 1 using `/sdd-spec <slug>`."
