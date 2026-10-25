# 0003. Clean Architecture and Domain-Driven Design per Service

- **Status:** Accepted
- **Date:** 2026-09-05

## Context

In microservice architectures, developers frequently succumb to either the "Anemic Domain Model" anti-pattern (where entities are mere property bags and business rules leak into API controllers) or framework lock-in (where domain classes are coupled to EF Core attributes, database annotations, or ASP.NET Core abstractions). This coupling severely hinders unit testing, increases regression rates during framework upgrades, and obscures core business invariants.

## Decision

- Enforce **Clean Architecture (Hexagonal Architecture)** and **Domain-Driven Design (DDD)** within every microservice boundary:
  1. **Domain Layer**:
     - Contains pure business entities, aggregate roots, value objects, domain events, and domain exceptions.
     - **Strict zero-dependency invariant**: The Domain project must never reference EF Core, ASP.NET Core, MassTransit, or any external third-party NuGet package (except language analyzers).
  2. **Application Layer**:
     - Defines use cases, CQRS commands/queries, MediatR handlers, FluentValidation validators, and outbound repository/service interfaces (ports).
  3. **Infrastructure Layer**:
     - Implements interfaces for persistence (EF Core, Dapper), message publishing (MassTransit), caching (StackExchange.Redis), and external APIs (adapters).
  4. **Api / Presentation Layer**:
     - Configures ASP.NET Core Minimal APIs / FastEndpoints, authentication middleware, DTO mappings, and OpenAPI schemas.
- Dependency Inversion principle is absolute: outer layers depend on inner layers; inner layers never reference outer layers.

## Consequences

- Domain invariants (e.g. stock reservation logic, order state transitions, cart price snapshotting) can be verified with high-speed unit tests in milliseconds without spinning up databases or web hosts.
- Persistence technologies or database schema changes can be altered in Infrastructure without modifying business logic.
- Adds an upfront cost of project separation and DTO/entity mapping, which is mitigated by shared conventions in `Shopizy.SharedKernel`.
