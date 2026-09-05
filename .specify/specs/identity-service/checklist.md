# Quality Checklist: Identity & Access Service (`identity-service`)

## 1. Specification & Domain Alignment
- [x] Executive summary and personas clearly articulate boundary and value.
- [x] Given-When-Then acceptance criteria specified for all core flows.
- [x] Strong password policy rules ($\ge 12$ chars, uppercase, lowercase, number, symbol) explicitly enumerated.
- [x] REST API endpoints and error schemas (RFC 7807 Problem Details) documented.

## 2. Test Completeness & Quality Gates
- [x] Section 6 mandatory automated test criteria fully defined.
- [x] Unit test criteria for password policy, email validation, and token generator specified.
- [x] Integration test criteria for EF Core persistence specified.
- [x] Automated E2E test scenarios (E2E-01 to E2E-04) explicitly specified with step-by-step assertions.

## 3. Architecture & Security Standards
- [x] Aligns with Clean Architecture (Domain -> Application -> Infrastructure -> Api).
- [x] Adheres to Project Constitution (JWT bearer security, zero trust, customer data isolation).
- [x] .NET Aspire integration defined (`Shopizy.ServiceDefaults` and `Shopizy.AppHost`).
