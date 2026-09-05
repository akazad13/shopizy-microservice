# Specification: Loyalty Points & Gift Cards (`loyalty-service`)

## 1. Executive Summary & Objectives
The **Loyalty Points & Gift Cards Service** (`loyalty-service`) powers customer retention, repeat purchase incentives, and digital prepaid currency for Shopizy. It manages customer loyalty accounts where points are earned upon order completion (1 point per $1 spent), points redemption for order discounts (100 points = $1 discount), digital gift card creation with secure claiming codes, and gift card balance deduction with idempotency and transaction auditing.

---

## 2. Personas & User Stories

- **US-1 (Loyalty Points Accumulation)**: As a registered customer, I want to earn loyalty points when my order is delivered/completed, so that I am rewarded for my repeat business.
- **US-2 (Loyalty Points Redemption)**: As a registered customer, I want to redeem points at checkout to receive discounts on my cart total, with validation against my available balance.
- **US-3 (Digital Gift Card Issuance)**: As an administrator, I want to generate digital gift cards with preloaded balances and unique 16-character redemption codes.
- **US-4 (Gift Card Spending & Partial Balance)**: As an online shopper, I want to apply a gift card towards my order, deducting part or all of its balance, with remaining funds preserved for future orders.
- **US-5 (Zero-Trust Account Isolation & Auditing)**: As a registered customer, I want my loyalty balance and transaction ledger completely isolated from other customers (Principle V).

---

## 3. Detailed Acceptance Criteria (Given-When-Then)

### AC-1.1: Points Accrual on Completed Order
- **Given** an authenticated customer completes a qualifying order with subtotal $S$ (e.g., $\$150.00$),
- **When** order completion is processed,
- **Then** the customer's loyalty account earns $\lfloor S \rfloor$ points ($150$ points) and a ledger transaction is recorded.

### AC-2.1: Points Redemption at Checkout
- **Given** a customer with $500$ points balance,
- **When** the customer requests redemption of $300$ points,
- **Then** the service calculates a $\$3.00$ discount ($100\text{ points} = \$1.00$), deducts $300$ points, and updates the account.
- **And** if a customer attempts to redeem more points than available, the service rejects the transaction with an error.

### AC-3.1: Digital Gift Card Generation
- **Given** an administrative request to create a gift card for $\$50.00$,
- **When** `POST /api/v1/gift-cards` is invoked by `StoreAdmin`,
- **Then** a gift card is generated with status `Active`, initial balance $\$50.00$, current balance $\$50.00$, and a unique 16-character alphanumeric code.

### AC-4.1: Gift Card Balance Deduction & Exhaustion
- **Given** an active gift card with balance $\$50.00$,
- **When** a shopper applies $\$30.00$ towards an order,
- **Then** the balance is reduced to $\$20.00$ and status remains `Active`.
- **When** the remaining $\$20.00$ is spent,
- **Then** the balance reaches $\$0.00$ and status transitions to `Depleted`.

### AC-5.1: Zero-Trust Customer Isolation
- **Given** customer A and customer B,
- **When** customer A queries `GET /api/v1/loyalty/my`,
- **Then** customer A's balance and ledger are returned.
- **And** if customer A queries customer B's account via `GET /api/v1/loyalty/account/{userId}`, the service returns `403 Forbidden`.

---

## 4. API & Integration Contracts

### REST Endpoints
- `GET /api/v1/loyalty/my` (Customer auth)
  - Response: `200 OK` with `LoyaltyAccountResponse(Guid CustomerId, int PointsBalance, decimal CashEquivalentValue, List<LoyaltyTransactionResponse> Transactions)`
- `POST /api/v1/loyalty/accrue` (Admin / Order worker)
  - Request: `AccruePointsRequest(Guid CustomerId, Guid OrderId, decimal OrderAmount)`
  - Response: `200 OK` with `LoyaltyAccountResponse`
- `POST /api/v1/loyalty/redeem` (Customer auth)
  - Request: `RedeemPointsRequest(int PointsToRedeem, Guid OrderId)`
  - Response: `200 OK` with `PointsRedemptionResponse(int PointsRedeemed, decimal DiscountAmount, int RemainingPoints)`
- `POST /api/v1/gift-cards` (Admin only)
  - Request: `CreateGiftCardRequest(decimal InitialBalance, string Currency, DateTime? ExpiresAtUtc)`
  - Response: `201 Created` with `GiftCardResponse`
- `GET /api/v1/gift-cards/check/{code}` (Public / Shopper)
  - Response: `200 OK` with `GiftCardBalanceResponse(string Code, decimal CurrentBalance, string Currency, string Status)`
- `POST /api/v1/gift-cards/apply` (Shopper auth)
  - Request: `ApplyGiftCardRequest(string Code, decimal AmountToDeduct, Guid OrderId)`
  - Response: `200 OK` with `GiftCardDeductionResponse(string Code, decimal AmountDeducted, decimal RemainingBalance, string Status)`

---

## 5. Data Models & Entities
1. **LoyaltyAccount**:
   - `Id` (Guid, PK)
   - `CustomerId` (Guid, Unique Index)
   - `PointsBalance` (int)
   - `CreatedAtUtc` (DateTime)
   - `UpdatedAtUtc` (DateTime?)
   - `Transactions` (Collection of `LoyaltyTransaction`)
2. **LoyaltyTransaction**:
   - `Id` (Guid, PK)
   - `LoyaltyAccountId` (Guid, FK)
   - `Type` (`Accrual`, `Redemption`, `Adjustment`)
   - `Points` (int)
   - `OrderId` (Guid?)
   - `Description` (string)
   - `CreatedAtUtc` (DateTime)
3. **GiftCard**:
   - `Id` (Guid, PK)
   - `Code` (string, 16 chars, Unique Index)
   - `InitialBalance` (decimal)
   - `CurrentBalance` (decimal)
   - `Currency` (string, max 3)
   - `Status` (`Active`, `Depleted`, `Expired`, `Disabled`)
   - `CreatedAtUtc` (DateTime)
   - `ExpiresAtUtc` (DateTime?)

---

## 6. Automated Test Criteria (MANDATORY)

### 6.1 Unit Test Criteria
- `AccruePoints`: $1 per point correctly converts dollar amounts ($\lfloor 125.75 \rfloor = 125$).
- `RedeemPoints`: 100 points = $1 discount conversion calculation.
- `RedeemPoints` exceeding current balance throws `LoyaltyDomainException`.
- `GiftCard.DeductBalance`: Partial deduction keeps status `Active`, full deduction sets status `Depleted`.
- `GiftCard.DeductBalance` with amount $> \text{CurrentBalance}$ throws `LoyaltyDomainException`.

### 6.2 Integration Test Criteria
- `LoyaltyRepository`: Persists account, transactions, balance updates with EF Core.
- `GiftCardRepository`: Unique code constraint, balance tracking, and status persistence.

### 6.3 Automated End-to-End (E2E) Test Scenarios
- **E2E-01 (Order Points Accrual)**: Admin accrues points for customer order -> balance increases and ledger record created.
- **E2E-02 (Points Redemption at Checkout)**: Customer redeems points -> discount calculated and balance deducted.
- **E2E-03 (Over-Redemption Rejection)**: Customer attempts to redeem more points than available -> 400 Bad Request.
- **E2E-04 (Gift Card Creation & Balance Check)**: Admin creates gift card -> balance check returns 200 with full initial balance.
- **E2E-05 (Gift Card Partial & Full Deduction)**: Deduct partial amount -> status stays active; deduct rest -> status transitions to Depleted.
- **E2E-06 (Customer Isolation & Auth Protection)**: Cross-customer balance inspection rejected with 403; unauthenticated requests return 401.
