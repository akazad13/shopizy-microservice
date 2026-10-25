# 0002. Orchestration and Observability with .NET Aspire

- **Status:** Accepted
- **Date:** 2026-09-05

## Context

Running 13 microservices alongside multiple containerized infrastructure dependencies (PostgreSQL with individual databases, Redis for caching and SignalR, and RabbitMQ for messaging) creates a difficult local developer inner loop. Traditional Docker Compose configurations require managing port maps, manual health check scripts, duplicate environment variables across projects, and separate telemetry tools (Jaeger, Prometheus, Grafana) which slow down local iteration and debugging.

## Decision

- Adopt **.NET Aspire 10** as the primary distributed orchestration and developer ergonomics framework.
- Centralize resource orchestration in `src/Shopizy.AppHost`:
  - Provision containerized infrastructure dependencies (`builder.AddPostgres("shopizy-postgres")`, `builder.AddRedis("shopizy-redis")`, `builder.AddRabbitMQ("shopizy-rabbitmq")`).
  - Configure isolated logical databases per service (`identitydb`, `catalogdb`, `orderdb`, etc.).
  - Wire service projects with declarative references (`.WithReference(orderDb).WithReference(redis).WithReference(rabbitmq)`).
- Enforce standardized resilience, health probes, and OpenTelemetry instrumentation across all services using `src/Shopizy.ServiceDefaults` (`AddServiceDefaults()`, `MapDefaultEndpoints()`).
- Leverage the integrated .NET Aspire Dashboard for real-time distributed tracing, structured logs, and resource health metrics during local execution and testing.

## Consequences

- Starting the entire 13-microservice topology and infrastructure requires only running `Shopizy.AppHost` (`dotnet run --project src/Shopizy.AppHost`).
- Service discovery and connection strings are injected automatically via Aspire's environment variable conventions, eliminating fragile hardcoded hostnames and ports.
- Native OpenTelemetry distributed tracing is active out-of-the-box across HTTP and MassTransit message boundaries.
- For non-local CI/CD and deployment, Aspire resource graphs translate to container manifests for Azure Container Apps (ACA) or Kubernetes.
