# Quality Checklist: [MODULE/FEATURE NAME] (`[module-slug]`)

## 1. Specification Readiness
- [ ] User stories clearly defined with actionable business value.
- [ ] Acceptance criteria expressed in explicit Given-When-Then format.
- [ ] API endpoints and data models documented with request/response schemas.

## 2. Automated Testing Completeness (Zero Manual Dependency)
- [ ] Unit test criteria explicitly enumerate domain logic, invariants, and edge cases.
- [ ] Integration test criteria cover persistence, database migrations, and HTTP pipeline.
- [ ] Automated E2E test scenarios cover full lifecycle and fault injection/RFC 7807 error paths.

## 3. Architectural & Security Compliance
- [ ] Clean Architecture layer boundaries strictly enforced (Domain has 0 external dependencies).
- [ ] Zero hardcoded secrets, credentials, or unprotected sensitive PII.
- [ ] Cryptographic authentication and role/permission claims enforced on mutating routes.
- [ ] Idempotency supported on state-altering operations.

## 4. Verification & Delivery
- [ ] All unit, integration, and E2E tests pass with 100% success rate.
- [ ] Build / linter passes cleanly with zero warnings (`--warnaserror`).
- [ ] Review Agent has signed off with `STATUS: APPROVED` in `review-log.md`.
- [ ] `README.md` synchronized with updated roadmap and test totals.
