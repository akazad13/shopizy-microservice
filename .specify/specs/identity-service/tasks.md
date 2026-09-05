# Actionable Tasks: Identity & Access Service (`identity-service`)

## Phase 1: Setup & Domain Contracts
- [x] [P1-01] Create `src/Shopizy.IdentityService/Shopizy.IdentityService.csproj` configured for .NET 10.
- [x] [P1-02] Define `UserRole` enum (`Customer`, `StoreAdmin`) and `Email` value object.
- [x] [P1-03] Define `PasswordPolicy` enforcing $\ge 12$ chars, uppercase, lowercase, number, symbol.
- [x] [P1-04] Define `RefreshToken` entity and `User` aggregate root with domain events.
- [x] [P1-05] Define Application DTOs (`RegisterRequest`, `LoginRequest`, `RefreshTokenRequest`, `AuthResponse`, `UserResponse`).
- [x] [P1-06] Define service and repository interfaces (`IUserRepository`, `IPasswordHasher`, `IJwtTokenGenerator`, `IIdentityService`).

## Phase 2: Domain Logic & Unit Tests
- [x] [P2-01] Implement `PasswordPolicy` domain validation rules.
- [x] [P2-02] Implement `PasswordHasher` using PBKDF2 HMAC-SHA512 with random salt.
- [x] [P2-03] Implement `JwtTokenGenerator` issuing signed JWT Bearer tokens with claims.
- [x] [P2-04] Create `tests/Shopizy.IdentityService.UnitTests` project.
- [x] [P2-05] Write unit tests for `PasswordPolicy` (12-char boundary, diversity requirements).
- [x] [P2-06] Write unit tests for `Email` value object and `User` aggregate.
- [x] [P2-07] Write unit tests for `PasswordHasher` and `JwtTokenGenerator`.

## Phase 3: Infrastructure, API & Integration Tests
- [x] [P3-01] Implement `IdentityDbContext` mapping `User` and `RefreshToken` with indexes.
- [x] [P3-02] Implement `UserRepository` and `IdentityService` application logic.
- [x] [P3-03] Implement minimal API endpoints (`/register`, `/login`, `/refresh`, `/me`, `/users`).
- [x] [P3-04] Wire authentication, authorization, and Aspire `ServiceDefaults`.
- [x] [P3-05] Create `tests/Shopizy.IdentityService.IntegrationTests` project.
- [x] [P3-06] Write integration tests verifying database persistence and unique email constraints.

## Phase 4: Automated E2E Test Suite
- [x] [P4-01] Create `tests/Shopizy.IdentityService.E2ETests` project with `WebApplicationFactory`.
- [x] [P4-02] Implement Scenario E2E-01: Full user registration, login, and `/me` query.
- [x] [P4-03] Implement Scenario E2E-02: Role-based access control protecting `/users` from customers.
- [x] [P4-04] Implement Scenario E2E-03: Token refresh cycle and rotation.
- [x] [P4-05] Implement Scenario E2E-04: Fault injection and RFC 7807 Problem Details.

## Phase 5: Solution Integration & Multi-Agent Review Loop
- [x] [P5-01] Add projects to `Shopizy.sln` and `Shopizy.slnx`.
- [x] [P5-02] Update `Shopizy.AppHost` with `identity-service` resource.
- [x] [P5-03] Execute solution-wide `dotnet build` with `--warnaserror`.
- [x] [P5-04] Execute solution-wide `dotnet test` with 100% pass rate.
- [x] [P5-05] Run Review Agent audit, write `review-log.md` with `STATUS: APPROVED`, and mark tasks complete.
