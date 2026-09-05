# Specification: Abandoned Cart Recovery Worker (`cart-abandonment-worker`)

## 1. Executive Summary & Objectives
The **Abandoned Cart Recovery Worker** (`cart-abandonment-worker`) is the automated revenue recovery service of the Shopizy microservices platform. It runs scheduled background sweeps detecting inactive shopping carts abandoned for greater than 2 hours ($T_{\text{abandon}} \ge 2\text{ hours}$), enforces a 24-hour deduplication cooldown window ($T_{\text{cooldown}} = 24\text{ hours}$) to prevent customer spam, generates personalized cart recovery links (`https://shopizy.com/cart/restore/{token}`), and dispatches recovery emails with item details via `NotificationService`.

---

## 2. Personas & User Stories

- **US-1 (Automated Inactivity Detection)**: As a store merchant, I want the system to automatically identify carts inactive for more than 2 hours, so that potential drop-off purchases can be recovered.
- **US-2 (Deduplication Cooldown & Spam Guard)**: As an online shopper, I want the recovery notifications to observe a 24-hour cooldown period, so that I am never spammed with repeated emails for the same shopping session.
- **US-3 (Personalized Cart Restore Link)**: As an online shopper, I want the recovery email to include a direct 1-click restore link with my cart items preserved, so that I can immediately resume checkout.
- **US-4 (Manual & Scheduled Trigger)**: As an administrator, I want to trigger a manual scan or inspect recovery metrics, so that system operational health and conversion recovery rates are observable.

---

## 3. Detailed Acceptance Criteria (Given-When-Then)

### AC-1.1: Cart Abandonment Threshold
- **Given** a cart with items whose last update was $> 2\text{ hours}$ ago,
- **When** the recovery worker executes its detection sweep,
- **Then** the cart is qualified as `Abandoned` and marked for recovery dispatch.
- **And** carts updated $< 2\text{ hours}$ ago or empty carts are ignored.

### AC-2.1: Deduplication Cooldown Guard
- **Given** a customer cart that was already sent a recovery email within the last 24 hours,
- **When** the recovery worker evaluates that cart in subsequent sweeps,
- **Then** the worker suppresses dispatch and logs `CooldownActive`.

### AC-3.1: Recovery Link Generation
- **Given** an abandoned cart with ID $C$ and customer email `"user@example.com"`,
- **When** the recovery email payload is generated,
- **Then** the email contains a secure tokenized restore URL format: `https://shopizy.com/cart/restore/{recoveryToken}`.

### AC-4.1: Administrative Sweep & Audit
- **Given** an authenticated user with `StoreAdmin` role,
- **When** `POST /api/v1/cart-abandonment/sweep` is invoked,
- **Then** an immediate scan runs, returning processed cart counts, notifications dispatched, and cooldown-suppressed counts.

---

## 4. API & Integration Contracts

### REST Endpoints
- `POST /api/v1/cart-abandonment/sweep` (StoreAdmin only)
  - Response: `200 OK` with `AbandonmentSweepResult(int CartsEvaluated, int RecoveriesDispatched, int SuppressedByCooldown, DateTime TimestampUtc)`
- `GET /api/v1/cart-abandonment/records` (StoreAdmin only)
  - Query: `?customerId={guid}`
  - Response: `200 OK` with `List<CartRecoveryRecordResponse>`
- `GET /api/v1/cart-abandonment/restore/{token}` (Public / Shopper)
  - Response: `200 OK` with `RestoreCartResponse(Guid CartId, Guid CustomerId, List<CartItemDto> Items, bool Expired)`

---

## 5. Data Models & Entities
1. **AbandonedCartRecord**:
   - `Id` (Guid, PK)
   - `CartId` (Guid, Indexed)
   - `CustomerId` (Guid, Indexed)
   - `CustomerEmail` (string)
   - `CartTotal` (decimal)
   - `ItemsJson` (string)
   - `LastActivityUtc` (DateTime)
   - `RecoveryToken` (string, Unique Index)
   - `DispatchedAtUtc` (DateTime)
   - `IsRestored` (bool)
   - `RestoredAtUtc` (DateTime?)

---

## 6. Automated Test Criteria (MANDATORY)

### 6.1 Unit Test Criteria
- Carts with inactivity $< 2\text{ hours}$ are NOT marked abandoned.
- Carts with inactivity $\ge 2\text{ hours}$ ARE marked abandoned.
- Empty carts are ignored.
- Cooldown check: Carts notified within 24 hours are suppressed.
- Recovery URL format verification (`https://shopizy.com/cart/restore/{token}`).

### 6.2 Integration Test Criteria
- `AbandonedCartRepository`: Persists records, enforces token uniqueness, and queries recent dispatches by customer/cart.

### 6.3 Automated End-to-End (E2E) Test Scenarios
- **E2E-01 (Inactivity Sweep & Dispatch)**: Sweep identifies 2-hour inactive cart -> generates token, dispatches recovery notification, saves record.
- **E2E-02 (24-Hour Cooldown Suppression)**: Re-running sweep immediately for same cart -> suppressed by cooldown.
- **E2E-03 (Recent Cart Ignored)**: Cart updated 30 minutes ago -> ignored by sweep.
- **E2E-04 (Token Restoration)**: Fetching restore details with valid recovery token -> returns cart ID and items.
- **E2E-05 (Invalid/Expired Token Handling)**: Fetching restore details with unknown token -> returns 404 Not Found.
- **E2E-06 (RBAC & Unauthorized Protection)**: Sweep and admin query endpoints reject non-admin requests with 403 / 401.
