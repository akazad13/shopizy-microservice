# Technical Implementation Plan: [MODULE/FEATURE NAME] (`[module-slug]`)

## 1. Architectural Alignment
- **Architecture Pattern**: Clean Architecture / Hexagonal Architecture
- **Layer Mapping**:
  - `Domain`: Pure entities, value objects, domain events, domain exceptions. Zero external dependencies.
  - `Application`: Command/Query handlers, use case interactors, DTO contracts, validator implementations.
  - `Infrastructure`: Database DbContext/ORM entities, repositories, external API clients, message brokers.
  - `Api / Presentation`: Minimal API / Controllers, route definitions, auth filters, middleware.

---

## 2. Directory Layout & Proposed Code Tree
```text
src/[module-slug]/
  ├── Domain/
  │   ├── Entities/
  │   ├── Events/
  │   └── Exceptions/
  ├── Application/
  │   ├── Commands/
  │   ├── Queries/
  │   └── Interfaces/
  ├── Infrastructure/
  │   ├── Persistence/
  │   └── Clients/
  └── Api/
      ├── Endpoints/
      └── Middleware/
tests/[module-slug].Tests/
  ├── Unit/
  ├── Integration/
  └── E2E/
```

---

## 3. Technology & Dependencies
- **Runtime**: [e.g. Node.js 22 / Python 3.12 / .NET 10 / Go 1.23]
- **ORM / Persistence**: [e.g. Prisma / EF Core / SQLAlchemy / pgx]
- **Validation**: [e.g. Zod / FluentValidation / Pydantic]
- **Test Runners**: [e.g. Vitest / xUnit / Pytest / Go test]

---

## 4. Quality & Verification Gates
1. Linter / Build verification with strict error checking (`--warnaserror` / `tsc --noEmit`).
2. Automated Unit Tests verifying all validation edge cases.
3. Automated Integration Tests verifying persistence and database transactions.
4. Automated E2E Tests executing lifecycle scenarios without manual dependencies.
