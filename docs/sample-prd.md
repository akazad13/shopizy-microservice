# Product Requirements Document (PRD): Shopizy Cloud Platform

**Project**: Shopizy E-Commerce Microservices Platform  
**Version**: 1.0.0  
**Author**: Product Management Team  
**Status**: Approved for Architecture & SDD Specification  

---

## 1. Vision & Executive Summary
Shopizy is a resilient, cloud-native e-commerce microservices platform designed to handle high transaction volumes during peak shopping campaigns. The system provides seamless browsing, authenticated cart operations, reliable checkout workflows, and asynchronous order processing.

---

## 2. Target Personas
1. **Shopper / Customer**: Browses product catalog, searches by categories, adds items to shopping cart, checks out securely, and receives order confirmation.
2. **Merchant / Admin**: Manages product listings, tracks inventory, and reviews order fulfillment statuses.
3. **Platform Operations (DevOps)**: Monitors system health, service metrics, error rates, and automated test pipelines.

---

## 3. Scope & Key Functional Capabilities

### 3.1 Identity & Access Management (`auth-service`)
- Customer and Merchant registration with email and password.
- Secure login emitting signed JWT tokens (containing user roles, permissions, and expiration).
- Token refresh endpoint and logout invalidation.
- Role-Based Access Control (Customer vs. Admin).

### 3.2 Product Catalog & Inventory (`catalog-service`)
- Categorized product browsing with full-text search, filtering, and pagination.
- Product details including price, description, images, and live stock levels.
- Admin APIs to create, update, or archive product listings.
- Fast cache-backed product lookups.

### 3.3 Shopping Cart & Checkout (`order-service`)
- Real-time cart management (add, update quantity, remove, clear cart).
- Checkout flow: validates item availability, calculates subtotal, taxes, and shipping fees.
- Order creation in `Pending` state with unique Order Reference ID.
- Emits `OrderPlacedEvent` to message bus upon successful placement.

### 3.4 Notifications & Webhooks (`notification-service`)
- Listens to `OrderPlacedEvent` asynchronously.
- Dispatches email and SMS confirmation receipts.
- Implements idempotent message handling to prevent duplicate customer notifications.

---

## 4. Non-Functional Requirements & Constraints
- **Reliability & Resilience**: 99.9% uptime; inter-service calls must use retries, timeouts, and circuit breakers.
- **Latency Target**: p95 API response time < 200ms for read operations; < 500ms for checkout.
- **Security**: OWASP Top 10 compliance; strict password hashing (Argon2 / PBKDF2 / BCrypt); all errors formatted as RFC 7807 Problem Details.
- **Testing & Quality Standard**: 
  - 100% automated test coverage for critical domain rules.
  - Mandatory automated End-to-End (E2E) test scenarios for core shopper flows.
  - All Pull Requests must pass automated review loops prior to merge.

---

## 5. Architectural Interview Topics (For Clarification)
- Confirm microservices deployment model vs modular monolith.
- Confirm choice of primary database engine (PostgreSQL vs SQL Server).
- Confirm message broker for domain events (RabbitMQ vs Kafka vs In-Memory).
- Confirm automated E2E test framework.
