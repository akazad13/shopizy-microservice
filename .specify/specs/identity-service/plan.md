# Technical Implementation Plan: Identity & Access Service (`identity-service`)

> **Module Slug:** `identity-service`  
> **Target Framework:** .NET 10 (C# 14)  
> **Architectural Pattern:** Clean Architecture (Hexagonal)  
> **Host Model:** ASP.NET Core Minimal API with .NET Aspire (`Shopizy.ServiceDefaults`)  

---

## 1. Architectural Alignment & Project Structure

The Identity & Access Service follows strict Clean Architecture boundaries and integrates with the existing solution projects:

- **Referenced Projects**:
  - `src/Shopizy.SharedKernel`: DDD primitives (`AggregateRoot`, `Entity`, `ValueObject`), functional `Result<T>`, error models, global exception handler.
  - `src/Shopizy.ServiceDefaults`: OpenTelemetry instrumentation, standard resilience handlers, health check mappings (`/health`, `/alive`).
- **Orchestration**:
  - `src/Shopizy.AppHost`: Registers `identity-service` as an Aspire microservice resource with references to PostgreSQL and Redis.
- **Projects to Create**:
  - `src/Shopizy.IdentityService/Shopizy.IdentityService.csproj` (Web API)
  - `tests/Shopizy.IdentityService.UnitTests/Shopizy.IdentityService.UnitTests.csproj`
  - `tests/Shopizy.IdentityService.IntegrationTests/Shopizy.IdentityService.IntegrationTests.csproj`
  - `tests/Shopizy.IdentityService.E2ETests/Shopizy.IdentityService.E2ETests.csproj`

---

## 2. Directory Layout

```text
src/Shopizy.IdentityService/
  ├── Domain/
  │   ├── Entities/
  │   │   ├── User.cs
  │   │   └── RefreshToken.cs
  │   ├── Enums/
  │   │   └── UserRole.cs
  │   ├── ValueObjects/
  │   │   └── Email.cs
  │   ├── Rules/
  │   │   └── PasswordPolicy.cs
  │   └── Events/
  │       └── UserRegisteredDomainEvent.cs
  ├── Application/
  │   ├── Contracts/
  │   │   ├── RegisterRequest.cs
  │   │   ├── LoginRequest.cs
  │   │   ├── RefreshTokenRequest.cs
  │   │   ├── AuthResponse.cs
  │   │   └── UserResponse.cs
  │   ├── Interfaces/
  │   │   ├── IUserRepository.cs
  │   │   ├── IPasswordHasher.cs
  │   │   ├── IJwtTokenGenerator.cs
  │   │   └── IIdentityService.cs
  │   └── Services/
  │       └── IdentityService.cs
  ├── Infrastructure/
  │   ├── Persistence/
  │   │   ├── IdentityDbContext.cs
  │   │   └── UserRepository.cs
  │   └── Security/
  │       ├── PasswordHasher.cs
  │       ├── JwtTokenGenerator.cs
  │       └── JwtOptions.cs
  ├── Endpoints/
  │   └── IdentityEndpoints.cs
  ├── Extensions/
  │   └── ServiceCollectionExtensions.cs
  ├── Program.cs
  ├── appsettings.json
  └── Shopizy.IdentityService.csproj

tests/Shopizy.IdentityService.UnitTests/
  ├── Domain/
  │   ├── PasswordPolicyTests.cs
  │   ├── EmailValueObjectTests.cs
  │   └── UserAggregateTests.cs
  └── Security/
      ├── PasswordHasherTests.cs
      └── JwtTokenGeneratorTests.cs

tests/Shopizy.IdentityService.IntegrationTests/
  ├── Persistence/
  │   ├── UserRepositoryTests.cs
  │   └── RefreshTokenPersistenceTests.cs
  └── Infrastructure/
      └── IdentityDbContextTests.cs

tests/Shopizy.IdentityService.E2ETests/
  ├── Fixtures/
  │   └── IdentityApplicationFactory.cs
  └── Scenarios/
      └── IdentityE2ETests.cs
```

---

## 3. Package Dependencies

### `Shopizy.IdentityService.csproj`
- `Microsoft.AspNetCore.Authentication.JwtBearer` (10.0.11)
- `Microsoft.EntityFrameworkCore.InMemory` (10.0.11)
- ProjectReference to `Shopizy.SharedKernel`
- ProjectReference to `Shopizy.ServiceDefaults`

### Test Projects
- `Microsoft.NET.Test.Sdk` (17.14.1)
- `xunit` (2.9.3)
- `xunit.runner.visualstudio` (3.1.4)
- `FluentAssertions` (8.10.0)
- `Moq` (4.20.72)
- `Microsoft.AspNetCore.Mvc.Testing` (10.0.11)
- ProjectReference to `Shopizy.IdentityService`
- ProjectReference to `Shopizy.SharedKernel`

---

## 4. Verification Strategy

1. **Build Verification**:
   ```bash
   dotnet build Shopizy.sln --configuration Release --warnaserror
   ```
2. **Automated Test Suite**:
   ```bash
   dotnet test Shopizy.sln --configuration Release --verbosity normal
   ```
3. **Multi-Agent Review Audit**:
   Conduct impartial review covering the 5 pillars, produce `review-log.md`, and verify zero failures before git commit.
