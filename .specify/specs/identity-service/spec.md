# Specification: Identity & Access Service (`identity-service`)

> **Document Version:** 1.0.0  
> **Status:** Approved  
> **Module Slug:** `identity-service`  
> **Target Framework:** .NET 10 (C# 14)  
> **Dependencies:** `shared-kernel`  

---

## 1. Executive Summary & Objectives

The **Identity & Access Service (`identity-service`)** is the core security and authentication boundary for the Shopizy microservice ecosystem. It governs user identity management, credential validation, cryptographic password hashing, stateless asymmetric/symmetric JWT access token issuance, refresh token lifecycle management, and role-based access control (RBAC) separating `Customer` and `StoreAdmin` personas.

### Core Business & Technical Value
- **Zero Trust Security**: Issues signed JWT Bearer tokens carrying subject (`sub`), email, and role claims.
- **Enterprise Password Standard**: Enforces a strict 12-character minimum password policy requiring uppercase, lowercase, numbers, and special symbols.
- **Stateless Authentication with Token Rotation**: Issues short-lived access tokens accompanied by cryptographically secure, rotatable refresh tokens.
- **Role Separation**: Restricts merchant and user directory endpoints exclusively to `StoreAdmin` roles while customer profiles are strictly isolated.
- **Full Test Automation**: Mandates 100% automated test coverage across unit tests, database persistence integration tests, and in-memory HTTP E2E tests.

---

## 2. Personas & User Stories

- **US-1 (Customer Registration)**: As a prospective buyer, I want to create an account with my name, email, and a secure password so that I can shop, track orders, and manage my profile.
- **US-2 (Customer & Admin Authentication)**: As a registered user, I want to securely log in with my credentials so that I receive an access token and refresh token for API access.
- **US-3 (Token Refresh Lifecycle)**: As an active API client, I want to refresh an expired access token using a valid refresh token so that my user session continues without requiring re-entry of credentials.
- **US-4 (Authenticated Profile Access)**: As an authenticated user, I want to fetch my personal profile (`/me`) so that I can view my account details and roles.
- **US-5 (StoreAdmin Directory Protection)**: As a store administrator, I want to view the system user directory, while ensuring ordinary customers cannot access this endpoint.
- **US-6 (API Consumer Error Handling)**: As an API consumer, I want standardized RFC 7807 Problem Details whenever inputs are malformed, credentials are bad, or unauthorized routes are requested.

---

## 3. Detailed Acceptance Criteria (Given-When-Then)

### AC-1: User Registration & Strong Password Policy
- **AC-1.1**: Given a registration payload with valid first name, last name, email, and a strong password meeting all criteria ($\ge 12$ characters, $\ge 1$ uppercase, $\ge 1$ lowercase, $\ge 1$ digit, $\ge 1$ special character), When `POST /api/v1/identity/register` is invoked, Then the system returns HTTP 201 Created with the created user ID, email, and assigned `Customer` role.
- **AC-1.2**: Given a password failing any policy criteria (e.g. $< 12$ characters, missing uppercase, missing special character), When registration is attempted, Then the system returns HTTP 400 Bad Request with RFC 7807 Problem Details detailing the validation failure.
- **AC-1.3**: Given an email that already exists in the system, When registration is attempted, Then the system returns HTTP 409 Conflict with an RFC 7807 Problem Details response without leaking password hashes.

### AC-2: User Authentication & Token Generation
- **AC-2.1**: Given valid registered credentials, When `POST /api/v1/identity/login` is invoked, Then the system returns HTTP 200 OK with a valid JWT access token, token expiration timestamp, and a cryptographically random refresh token.
- **AC-2.2**: Given incorrect credentials (invalid email or wrong password), When `POST /api/v1/identity/login` is invoked, Then the system returns HTTP 401 Unauthorized with RFC 7807 Problem Details.

### AC-3: Refresh Token Lifecycle & Rotation
- **AC-3.1**: Given a valid, unexpired, and non-revoked refresh token, When `POST /api/v1/identity/refresh` is invoked, Then the system returns HTTP 200 OK with a fresh access token and an updated/rotated refresh token.
- **AC-3.2**: Given an invalid, expired, or already-revoked refresh token, When `POST /api/v1/identity/refresh` is invoked, Then the system returns HTTP 401 Unauthorized.

### AC-4: Profile & Role-Based Access Control
- **AC-4.1**: Given a request to `GET /api/v1/identity/me` with a valid Bearer token, Then the system returns HTTP 200 OK with the authenticated user's profile and roles.
- **AC-4.2**: Given a request to `GET /api/v1/identity/me` without a token or with a tampered token, Then the system returns HTTP 401 Unauthorized.
- **AC-4.3**: Given an authenticated `Customer` attempting to access `GET /api/v1/identity/users`, Then the system returns HTTP 403 Forbidden.
- **AC-4.4**: Given an authenticated `StoreAdmin` accessing `GET /api/v1/identity/users`, Then the system returns HTTP 200 OK with a list of users.

---

## 4. API & Integration Contracts

### 4.1 Endpoint Signatures

| Verb | Route | Auth Required | Authorized Roles | Description |
| :--- | :--- | :--- | :--- | :--- |
| `POST` | `/api/v1/identity/register` | No | Anonymous | Register new user account |
| `POST` | `/api/v1/identity/login` | No | Anonymous | Authenticate and obtain tokens |
| `POST` | `/api/v1/identity/refresh` | No | Anonymous | Exchange refresh token for new access token |
| `GET` | `/api/v1/identity/me` | Yes (Bearer) | Any Authenticated | Fetch current user profile |
| `GET` | `/api/v1/identity/users` | Yes (Bearer) | `StoreAdmin` | Directory listing of registered users |

### 4.2 Request / Response Schemas

#### Register Request (`POST /api/v1/identity/register`)
```json
{
  "email": "customer@shopizy.test",
  "password": "SuperSecretPassword123!",
  "firstName": "John",
  "lastName": "Doe",
  "role": "Customer"
}
```

#### Auth Response (`POST /api/v1/identity/login`, `/register`, `/refresh`)
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "4fa85f6457174562b3fc2c963f66afa6d7f4",
  "expiresAtUtc": "2026-09-06T02:00:00Z",
  "user": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "email": "customer@shopizy.test",
    "firstName": "John",
    "lastName": "Doe",
    "role": "Customer"
  }
}
```

#### Error Response (RFC 7807 Problem Details)
```json
{
  "type": "https://httpstatuses.com/400",
  "title": "Validation Failure",
  "status": 400,
  "detail": "Password does not satisfy policy: minimum 12 characters required.",
  "errors": [
    "Password must be at least 12 characters long."
  ]
}
```

---

## 5. Data Models & State Transitions

### 5.1 Entities & Value Objects

- **User**: Aggregate root
  - `Id` (Guid, PK)
  - `Email` (`Email` Value Object, unique index)
  - `PasswordHash` (string, PBKDF2 hash + salt)
  - `FirstName` (string, max 100)
  - `LastName` (string, max 100)
  - `Role` (`UserRole` Enum: `Customer`, `StoreAdmin`)
  - `IsActive` (bool)
  - `CreatedAtUtc` (DateTime)
  - `UpdatedAtUtc` (DateTime?)
  - `RefreshTokens` (`IReadOnlyCollection<RefreshToken>`)

- **RefreshToken**: Entity
  - `Id` (Guid, PK)
  - `UserId` (Guid, FK)
  - `Token` (string, indexed)
  - `ExpiresAtUtc` (DateTime)
  - `CreatedAtUtc` (DateTime)
  - `RevokedAtUtc` (DateTime?)
  - `IsActive` => `RevokedAtUtc == null && DateTime.UtcNow < ExpiresAtUtc`

---

## 6. Automated Test Criteria (MANDATORY GATE)

### 6.1 Unit Test Criteria
- [ ] **PasswordPolicy**: Verify validation rules:
  - Rejects passwords $< 12$ characters (e.g. `Pass123!`).
  - Rejects passwords missing uppercase letters (e.g. `supersecretpassword123!`).
  - Rejects passwords missing lowercase letters (e.g. `SUPERSECRETPASSWORD123!`).
  - Rejects passwords missing numeric digits (e.g. `SuperSecretPassword!`).
  - Rejects passwords missing special symbols (e.g. `SuperSecretPassword123`).
  - Accepts valid passwords $\ge 12$ characters with all four character classes.
- [ ] **Email Value Object**:
  - Rejects null, whitespace, or malformed email strings.
  - Accepts standard valid RFC email addresses and normalizes to lowercase.
- [ ] **PasswordHasher**:
  - PBKDF2 hashing generates unique salts for identical passwords.
  - Verifies correct passwords successfully.
  - Rejects incorrect passwords and tampered hashes.
- [ ] **JwtTokenGenerator**:
  - Emits valid signed JWT tokens.
  - Contains claims for `sub` (UserId), `email`, and `role`.
- [ ] **User Aggregate**:
  - Correctly registers user and stages `UserRegisteredDomainEvent`.
  - Enforces active status and prevents duplicate refresh token additions.

- [ ] **Customer Data Isolation (Constitution Principle V)**:
  - Enforces that non-admin customers can only retrieve their own profile.
  - Rejects attempts by a customer to inspect another user's profile with HTTP 403 Forbidden.
  - Grants `StoreAdmin` role authority to query any user profile.

### 6.2 Integration Test Criteria
- [ ] **UserRepository Persistence**:
  - Persists and retrieves `User` aggregate from EF Core database.
  - Enforces unique index constraint on `Email`.
  - Correctly persists associated `RefreshToken` entities.
- [ ] **RefreshToken Repository**:
  - Retrieves active refresh token by token string.
  - Correctly revokes tokens and updates revocation timestamp.

### 6.3 Automated End-to-End (E2E) Test Scenarios
- [ ] **Scenario E2E-01: User Registration, Login & Profile Retrieval**
  - *Step 1*: Client POST `/api/v1/identity/register` with valid strong password and payload. Expected: 201 Created + user summary.
  - *Step 2*: Client POST `/api/v1/identity/login` with created credentials. Expected: 200 OK + JWT access token + refresh token.
  - *Step 3*: Client GET `/api/v1/identity/me` with `Authorization: Bearer <token>`. Expected: 200 OK + user profile matching registration.
- [ ] **Scenario E2E-02: Role-Based Access Control (RBAC) Protection**
  - *Step 1*: Customer logs in and attempts GET `/api/v1/identity/users`. Expected: 403 Forbidden.
  - *Step 2*: Admin user logs in (`StoreAdmin` role) and invokes GET `/api/v1/identity/users`. Expected: 200 OK + list containing registered users.
- [ ] **Scenario E2E-03: Token Refresh Lifecycle**
  - *Step 1*: Client logs in and receives `refreshToken`.
  - *Step 2*: Client invokes POST `/api/v1/identity/refresh` with valid `refreshToken`. Expected: 200 OK + newly minted access token and new rotated refresh token.
  - *Step 3*: Client attempts refresh with invalid or revoked token. Expected: 401 Unauthorized.
- [ ] **Scenario E2E-04: Fault Injection & Problem Details**
  - *Step 1*: Client attempts registration with weak password ($< 12$ chars). Expected: 400 Bad Request + RFC 7807 Problem Details.
  - *Step 2*: Client attempts registration with already existing email. Expected: 409 Conflict.
  - *Step 3*: Client calls GET `/api/v1/identity/me` without Bearer token. Expected: 401 Unauthorized.
- [ ] **Scenario E2E-05: Customer Data Isolation (Constitution Principle V)**
  - *Step 1*: Customer A registers and logs in.
  - *Step 2*: Customer B registers and logs in.
  - *Step 3*: Customer A attempts `GET /api/v1/identity/users/{customerB.Id}` with Customer A's Bearer token. Expected: 403 Forbidden with RFC 7807 Problem Details (`User.Forbidden`).
  - *Step 4*: StoreAdmin accesses `GET /api/v1/identity/users/{customerB.Id}` with StoreAdmin Bearer token. Expected: 200 OK + user B profile.
- [ ] **Scenario E2E-06: Idempotency Validation (Constitution Principle VI)**
  - *Step 1*: Client registers user with `Idempotency-Key: {guid}` header. Expected: 201 Created.
  - *Step 2*: Client resends identical registration request with same `Idempotency-Key`. Expected: 201 Created with header `X-Cache-Lookup: HIT` (no 409 Conflict or duplicate creation).

---

## 7. Non-Functional & Security Requirements

- **Password Hashing**: PBKDF2 HMAC-SHA512 with cryptographically random 128-bit salt and minimum 100,000 iterations.
- **JWT Cryptography**: Signed with HMAC-SHA256 (or RSA-256) secret key. Access token lifetime default 60 minutes; refresh token lifetime 7 days.
- **Latency**: P95 authentication latency $< 150\text{ms}$ on standard hardware.
- **Security Protections**: Sensitive password hashes never exposed in API DTOs or logs.
