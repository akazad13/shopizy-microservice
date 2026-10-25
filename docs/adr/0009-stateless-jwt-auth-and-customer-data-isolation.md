# 0009. Stateless Asymmetric JWT Authentication and Query-Level Isolation

- **Status:** Accepted
- **Date:** 2026-09-05

## Context

In an architecture with 13 distributed microservices, requiring every incoming request to synchronously query a centralized session database or auth service introduces a latency bottleneck and single point of failure. At the same time, customer privacy and tenant security are paramount: customers must never be able to inspect, alter, or infer orders, addresses, carts, or profile information belonging to another customer.

## Decision

- Adopt **Stateless Asymmetric JWT Bearer Authentication**:
  - `Shopizy.IdentityService` issues RSA-signed (RS256) JSON Web Tokens containing claims (`sub` for UserId, `email`, and `roles` like `Customer` or `StoreAdmin`).
  - Downstream microservices validate tokens locally using the Identity Service's public key (JWKS) without making synchronous HTTP calls to the auth service.
- Enforce **Query-Level Customer Data Isolation**:
  - All non-admin database queries and commands must automatically bind `CustomerId = CurrentUser.Id` retrieved from the cryptographically verified JWT claims.
  - Repository interfaces and EF Core Global Query Filters prevent inadvertent cross-tenant data leaks.
  - Admin endpoints require the `StoreAdmin` role policy and are monitored through structured audit logging.
- Strict PCI Compliance: Raw credit card numbers and CVV codes must never enter, touch, or be stored in Shopizy databases; tokenized payment methods (Stripe Elements / tokens) are used exclusively.

## Consequences

- Services authenticate and authorize requests independently at line speed with zero external network overhead.
- Rotating keys requires publishing updated JWKS public keys, which services cache with periodic refresh.
- Accidental IDOR (Insecure Direct Object Reference) vulnerabilities are prevented at the database query level.
