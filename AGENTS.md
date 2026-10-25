# AGENTS.md — AI Agent Guidance & Repository Rules

This document provides mandatory guidance for autonomous AI coding agents (Antigravity, Claude Code, GitHub Copilot, Cursor, Windsurf, Devin) operating in `akazad13/shopizy-microservice`.

---

## 1. System Identity & Architecture

**Product**: `Shopizy Microservices Platform` — Enterprise e-commerce microservices ecosystem built with .NET 10, Clean Architecture, DDD, MassTransit, and .NET Aspire.
- **Microservices**: Catalog, Basket, Ordering, Identity, Discount, Payment, Notification, Shipping, Inventory, Customer.
- **Framework**: .NET 10, C# 14, ASP.NET Core Minimal APIs.
- **Messaging**: RabbitMQ + MassTransit with Transactional Outbox pattern.
- **Persistence**: Database-per-Service (PostgreSQL 17), Redis caching, Redis Lua stock locks.
- **Orchestration**: .NET Aspire (`Shopizy.AppHost`).

---

## 2. Inviolable Architectural Invariants

1. **Database-Per-Service Isolation**:
   - Microservices MUST maintain isolated database contexts.
   - Cross-service table joins and direct foreign keys are strictly prohibited. Inter-service data sharing must occur via asynchronous domain events or synchronous API calls.
2. **Transactional Outbox for Events**:
   - All events published to RabbitMQ must use MassTransit's Entity Framework Core Outbox to ensure transactional consistency with local database commits.
3. **Inventory Reservation Atomicity**:
   - Stock deduction must use atomic Redis Lua scripts or database row-level versioning to eliminate race conditions during high-concurrency flash sales.
4. **Banned Packages Policy**:
   - `Moq` is prohibited (use `NSubstitute`).
   - `AutoMapper` is prohibited (use explicit mapping or compile-time generators).

---

## 3. Commands Cheat Sheet

```bash
# Build entire microservice solution
dotnet build Shopizy.sln

# Run all unit and integration tests
dotnet test Shopizy.sln

# Run Aspire orchestrator (starts all microservices + infra)
dotnet run --project src/Shopizy.AppHost/Shopizy.AppHost.csproj
```

---

## 4. Verification Gates for AI Agents

Before submitting code changes or finishing a turn:
1. Run `dotnet build Shopizy.sln` — must compile with **0 errors**.
2. Run `dotnet test Shopizy.sln` — all tests must pass.
3. Ensure no cross-database foreign key relationships were introduced.
