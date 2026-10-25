# 0001. Record Architecture Decisions

- **Status:** Accepted
- **Date:** 2026-09-05

## Context

Shopizy is an enterprise-scale digital commerce platform consisting of 13 autonomous microservices, multiple data stores (PostgreSQL, Redis, Elasticsearch), asynchronous messaging (RabbitMQ), and distributed orchestration (.NET Aspire). In a system of this size and complexity, engineering decisions made early in the architecture phase will be scrutinized during scale-out, feature expansion, and automated AI-driven agent loops. Without recorded architectural rationale, the reasoning behind structural trade-offs is quickly forgotten or accidentally broken during refactoring.

## Decision

- Record every cross-cutting architectural decision as an Architecture Decision Record (ADR) under `docs/adr/`.
- Use a sequential numbering scheme (`NNNN-title.md`) with a consistent structure: Title, Status, Date, Context, Decision, and Consequences.
- Specifications (`.specify/specs/`) define *what* a module implements and acceptance criteria; ADRs document *why* that architectural approach was chosen over alternatives.
- Whenever an architectural choice is replaced or revised, the original ADR remains preserved and marked `Superseded by NNNN`.

## Consequences

- Architectural invariants are permanently recorded and directly referenceable by human engineers and autonomous coding agents.
- Changes to cross-cutting concerns (e.g. outbox patterns, security models, caching strategies) require reviewing and updating the relevant ADR.
- Onboarding new contributors or validating architectural adherence in CI/CD is simplified through an indexed decision log.
