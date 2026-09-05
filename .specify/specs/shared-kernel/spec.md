# Specification: Shared Kernel & Aspire Orchestrator (`shared-kernel`)

> **Document Version:** 1.0.0  
> **Status:** Ratified  
> **Module Slug:** `shared-kernel`  
> **Target Framework:** .NET 10 (C# 14)  
> **Dependencies:** None (Foundational Layer)  

---

## 1. Executive Summary & Objectives

The **Shared Kernel & Aspire Orchestrator** is the bedrock foundational infrastructure module for the entire Shopizy microservices platform. It establishes the common DDD building blocks, functional result error types, asynchronous MassTransit integration event contracts, transactional outbox primitives, RFC 7807 ProblemDetails middleware, and .NET Aspire orchestration (`Shopizy.AppHost` and `Shopizy.ServiceDefaults`).

### Core Business & Technical Value
- **Uniform Domain Modeling**: Standardizes aggregate roots, entities, and domain event dispatch across all 10 downstream microservices.
- **Contract-Driven Asynchronous Messaging**: Provides strongly-typed MassTransit integration events ensuring zero serialization mismatches across services.
- **Unified Local Inner-Loop**: Equips developers with a single-click/command `dotnet run` AppHost orchestrating PostgreSQL, Redis, RabbitMQ, and Elasticsearch with an interactive Aspire telemetry dashboard.
- **Zero Ambiguity Error Handling**: Establishes functional `Result<T>` and RFC 7807 ProblemDetails responses for all HTTP endpoints.

---

## 2. Personas & User Stories

- **US-1 (Backend Engineer - DDD Primitives)**: As a microservice developer, I want foundational `AggregateRoot`, `Entity`, and `ValueObject` base classes so that I can implement rich, encapsulation-safe domain models without boilerplate.
- **US-2 (Systems Architect - Event Contracts)**: As a lead architect, I want strongly-typed, immutable integration event contracts for Orders, Inventory, Payments, and Shipping so that microservices communicate reliably across RabbitMQ.
- **US-3 (DevOps / Developer - Aspire Inner-Loop)**: As an engineer, I want `Shopizy.AppHost` to orchestrate PostgreSQL, Redis, RabbitMQ, and service discovery so that I can run the entire platform locally with zero manual setup.
- **US-4 (API Consumer - Predictable Errors)**: As a client/frontend developer, I want all microservices to return consistent RFC 7807 `ProblemDetails` for errors and idempotency conflicts so that client handling is completely standardized.

---

## 3. Detailed Acceptance Criteria (Given-When-Then)

### AC-1: Functional Result & Error Types
- **AC-1.1**: Given a valid value $V$, When calling `Result<T>.Success(V)`, Then `IsSuccess` is `true`, `Value` equals $V$, and `Error` is `Error.None`.
- **AC-1.2**: Given an error condition $E$, When calling `Result<T>.Failure(E)`, Then `IsSuccess` is `false`, `Error` equals $E$, and accessing `Value` throws an `InvalidOperationException`.
- **AC-1.3**: Given standard error archetypes (`Validation`, `NotFound`, `Conflict`, `Unauthorized`), When converted to RFC 7807 ProblemDetails, Then HTTP status codes map deterministically to `400`, `404`, `409`, and `401`.

### AC-2: Base DDD Aggregate Root & Domain Events
- **AC-2.1**: Given an aggregate root, When `RaiseDomainEvent(IDomainEvent @event)` is invoked, Then the event is staged in `DomainEvents` collection.
- **AC-2.2**: Given staged domain events, When `ClearDomainEvents()` is called, Then the staged collection is emptied.

### AC-3: Integration Event Contracts & Serialization Fidelity
- **AC-3.1**: Given any integration event contract (`OrderPlacedIntegrationEvent`, `PaymentCompletedIntegrationEvent`, etc.), When serialized to JSON and deserialized across MassTransit JSON envelope, Then all properties (IDs, Guids, Decimals, Timestamps) preserve 100% precision.

### AC-4: .NET Aspire ServiceDefaults & Observability
- **AC-4.1**: Given any ASP.NET Core service referencing `Shopizy.ServiceDefaults`, When querying `/alive` and `/health`, Then the service returns HTTP 200 with structured JSON health status.
- **AC-4.2**: Given an incoming HTTP request or MassTransit consumer execution, When inspected in the Aspire Dashboard, Then OpenTelemetry trace IDs and activity spans correlate seamlessly.

### AC-5: Idempotency Validation
- **AC-5.1**: Given a request with header `Idempotency-Key: {guid}`, When processed for the first time, Then the operation executes and the response payload is cached in Redis for 60 seconds.
- **AC-5.2**: Given a duplicate request with the identical `Idempotency-Key`, When received within the cache window, Then the cached response is returned immediately with header `X-Cache-Lookup: HIT` without re-executing business logic.

---

## 4. API & Integration Contracts

### 4.1 MassTransit Integration Event Schemas

#### `OrderPlacedIntegrationEvent`
```json
{
  "$type": "Shopizy.SharedKernel.Contracts.Orders.OrderPlacedIntegrationEvent",
  "orderId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "customerId": "8ba12f64-1111-4562-b3fc-2c963f66afa1",
  "totalAmount": 149.99,
  "currency": "USD",
  "placedAtUtc": "2026-09-05T20:00:00Z",
  "expiresAtUtc": "2026-09-05T20:15:00Z",
  "items": [
    {
      "productId": "9fa85f64-5717-4562-b3fc-2c963f66afa7",
      "sku": "SNK-BLK-42",
      "quantity": 1,
      "unitPrice": 149.99
    }
  ]
}
```

#### `OrderExpiredIntegrationEvent`
```json
{
  "$type": "Shopizy.SharedKernel.Contracts.Orders.OrderExpiredIntegrationEvent",
  "orderId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "expiredAtUtc": "2026-09-05T20:15:00Z",
  "reason": "15-minute payment window elapsed without completed transaction"
}
```

#### `PaymentCompletedIntegrationEvent`
```json
{
  "$type": "Shopizy.SharedKernel.Contracts.Payments.PaymentCompletedIntegrationEvent",
  "paymentId": "4da85f64-5717-4562-b3fc-2c963f66afa8",
  "orderId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "transactionId": "txn_stripe_99923847",
  "amountPaid": 149.99,
  "currency": "USD",
  "paidAtUtc": "2026-09-05T20:05:00Z"
}
```

#### `StockReservedIntegrationEvent` & `StockReservationFailedIntegrationEvent`
- Published by Inventory/Catalog upon order placement to confirm or reject inventory allocation.

---

## 5. Data Models & Outbox Architecture

### 5.1 Transactional Outbox Entity (`OutboxMessage`)
```csharp
public sealed class OutboxMessage
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string EventType { get; init; } = default!;
    public string PayloadJson { get; init; } = default!;
    public DateTime CreatedAtUtc { get; init; } = DateTime.UtcNow;
    public DateTime? ProcessedAtUtc { get; set; }
    public string? Error { get; set; }
    public int RetryCount { get; set; } = 0;
}
```

---

## 6. Automated Test Criteria (MANDATORY)

### 6.1 Unit Test Criteria (`Shopizy.SharedKernel.UnitTests`)
1. **Result Pattern**:
   - `Result.Success()` creates success status with no errors.
   - `Result.Failure(error)` sets correct error message, code, and classification.
   - `Result<T>.Map()` transforms success values and propagates failures without executing the mapping lambda.
2. **Domain Primitives**:
   - `Entity<TId>` equality checks evaluate true when IDs and types match; false when IDs differ.
   - `ValueObject` equality compares structural component properties rather than reference equality.
   - `AggregateRoot<TId>` records domain events and clears them on command.
3. **Integration Event Serialization**:
   - Round-trip JSON serialization and deserialization of all 11 MassTransit event contracts via `System.Text.Json` with exact numerical precision.

### 6.2 Integration Test Criteria (`Shopizy.SharedKernel.IntegrationTests`)
1. **Aspire ServiceDefaults Health Endpoints**:
   - Verification of `/health` and `/alive` returning 200 OK.
2. **Global Exception Middleware**:
   - Unhandled domain exception translates to RFC 7807 `ProblemDetails` with correlation ID and 400 Bad Request.
   - Unhandled system exception translates to 500 Internal Server Error without leaking internal stack traces.
3. **Idempotency Service**:
   - In-memory/Redis idempotency store test validating duplicate requests return exact cached response.

### 6.3 Automated End-to-End (E2E) Test Scenarios (`Shopizy.SharedKernel.E2ETests`)
- **Scenario E2E-01: Aspire AppHost Resource Provisioning**:
  - *Step 1*: Spin up `Shopizy.AppHost` test harness.
  - *Step 2*: Verify PostgreSQL, Redis, and RabbitMQ container resource allocations are healthy.
  - *Step 3*: Assert connection strings are dynamically generated and resolvable via Aspire service discovery.
- **Scenario E2E-02: End-to-End Outbox Event Pipeline**:
  - *Step 1*: Publish an integration event into the `OutboxMessage` store.
  - *Step 2*: Background outbox publisher picks up unread message and publishes to RabbitMQ test bus.
  - *Step 3*: Consumer receives event and asserts payload integrity.

---

## 7. Non-Functional & Security Standards

- **Zero External Dependencies in Domain**: `Shopizy.SharedKernel` core DDD library depends only on basic BCL (`System`).
- **Standardized Logging**: OpenTelemetry Activity Source configured in `Shopizy.ServiceDefaults` to propagate W3C TraceContext headers across HTTP and MassTransit.
- **Deterministic Build**: Zero warnings under `TreatWarningsAsErrors=true`.
