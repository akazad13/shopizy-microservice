# 🚀 Feature: Identity & Access Service (`identity-service`)

## 📋 Summary
This pull request implements the **Identity & Access Service (`identity-service`)** specification adhering strictly to the Spec-Driven Development (SDD) lifecycle. It provides secure user registration with a 12-character strong password policy, PBKDF2 password hashing with cryptographic salt, stateless JWT Bearer token issuance, refresh token rotation, role-based access control (`Customer` and `StoreAdmin`), and comprehensive automated unit, integration, and E2E test suites.

---

## 🏛️ PRD & Architecture Traceability
- **PRD Goals Addressed**: Phase 1 MVP — User registration, credential authentication, refresh tokens, role claims, and user directory protection.
- **Architectural Component**: Identity & Access Service (`src/Shopizy.IdentityService`) orchestrated via `.NET Aspire` (`Shopizy.AppHost`, `Shopizy.ServiceDefaults`).
- **Specification Document**: `.specify/specs/identity-service/spec.md`
- **Technical Plan**: `.specify/specs/identity-service/plan.md`
- **Quality Checklist**: `.specify/specs/identity-service/checklist.md`

---

## ✨ Changes & Deliverables

### 1. Production Source Code (`src/Shopizy.IdentityService`)
- **Domain Layer**:
  - `User`: Aggregate root managing identity, password hash, role, refresh token collection, and raising `UserRegisteredDomainEvent`.
  - `UserRole`: Enum distinguishing `Customer` and `StoreAdmin`.
  - `RefreshToken`: Entity managing expiration, active status, and cryptographic revocation.
  - `Email`: Value object enforcing RFC format and lowercasing normalization.
  - `PasswordPolicy`: Domain validator enforcing $\ge 12$ characters, uppercase, lowercase, numeric digit, and special character.
- **Application Layer**:
  - `IdentityService`: Application service handling registration, login, token refresh rotation, profile retrieval, and directory query.
  - `IUserRepository`, `IPasswordHasher`, `IJwtTokenGenerator`, `IIdentityService`: Domain and infrastructure abstraction contracts.
  - DTOs: `RegisterRequest`, `LoginRequest`, `RefreshTokenRequest`, `AuthResponse`, `UserResponse`.
- **Infrastructure Layer**:
  - `IdentityDbContext`: EF Core context with entity configurations and unique email index.
  - `UserRepository`: Repository implementation with entity tracking handling new/modified child collections.
  - `PasswordHasher`: PBKDF2 HMAC-SHA512 with 128-bit salt and constant-time string comparison (`CryptographicOperations.FixedTimeEquals`).
  - `JwtTokenGenerator`: Issues signed JWT Bearer tokens with subject, email, and role claims.
- **Endpoints & Presentation**:
  - Minimal API endpoints:
    - `POST /api/v1/identity/register`
    - `POST /api/v1/identity/login`
    - `POST /api/v1/identity/refresh`
    - `GET /api/v1/identity/me` (Authorized)
    - `GET /api/v1/identity/users` (Authorized `StoreAdmin` only)
  - RFC 7807 Problem Details error handler.
- **AppHost Orchestration**:
  - Registered `identity-service` with PostgreSQL, Redis, and RabbitMQ container bindings in `src/Shopizy.AppHost/Program.cs`.

### 2. Automated Test Coverage
- **Unit Tests (`tests/Shopizy.IdentityService.UnitTests`)**: 35 tests covering password policy boundaries, email validation, aggregate invariant enforcement, password hashing, and JWT token claims.
- **Integration Tests (`tests/Shopizy.IdentityService.IntegrationTests`)**: 4 tests covering EF Core database persistence, unique constraint handling, and refresh token lifecycle.
- **Automated E2E Tests (`tests/Shopizy.IdentityService.E2ETests`)**: 4 full scenario tests executed against in-memory HTTP test server using `WebApplicationFactory`:
  - **Scenario E2E-01**: Registration, login, and `/me` query.
  - **Scenario E2E-02**: RBAC protection (`Customer` 403 Forbidden vs `StoreAdmin` 200 OK).
  - **Scenario E2E-03**: Refresh token rotation and replay prevention.
  - **Scenario E2E-04**: Fault injection (weak password, duplicate email, missing tokens).

---

## 🔍 Multi-Agent Review Loop Report
- **Review Agent Status**: `APPROVED`
- **Review Cycles Completed**: 2 iterations
- **Issues Resolved During Loop**:
  - Corrected child entity state tracking in `UserRepository.UpdateAsync` for newly added refresh tokens.
  - Harmonized JWT claim assertion in unit tests with standard JWT claim mappings.

---

## 🧪 Verification & Test Results
- **Solution Build**: Passed cleanly with `--warnaserror` (0 warnings, 0 errors).
- **Test Pass Rate**: 100% (73 passed across all 5 test assemblies, 0 failed).
- **Review Log**: `.specify/specs/identity-service/review-log.md`

---

## 👥 Reviewer Checklist
- [x] Code adheres strictly to Clean Architecture and Project Constitution
- [x] Zero breaking changes to shared contracts
- [x] Automated E2E scenarios cover happy path and critical error handling
- [x] All 73 tests green with zero flaky behavior
