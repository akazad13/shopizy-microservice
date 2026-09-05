# Specification: Payment & Refund Gateway (`payment-service`)

## 1. Executive Summary & Objectives
The **Payment & Refund Gateway** (`payment-service`) provides tokenized payment processing, transaction lifecycle state reconciliation, and automated post-payment electronic refunds. It strictly maintains PCI DSS compliance by storing zero raw Primary Account Numbers (PAN) or Card Verification Values (CVV). All mutating payment and refund operations are idempotency-protected via `Idempotency-Key` headers.

### Key Objectives:
- **Tokenized Transactions**: Process payments via opaque gateway tokens (`tok_...` / `pm_...`) without touching raw card data.
- **Payment Lifecycle**: `Initiated` $\to$ `Succeeded` / `Failed` $\to$ `Refunded` (Full or Partial).
- **Automated Electronic Refunds**: Support full or partial refunds for pre-shipment order cancellations with transactional audit logs.
- **Strict Idempotency (Principle VI)**: Guarantee that duplicate charges never occur during network timeouts, retries, or rapid double-clicks.
- **Customer Multi-Tenant Isolation (Principle V)**: Customers can only query their own payment and refund histories.

---

## 2. Personas & User Stories

- **US-1 (Pay for Order)**: As a Customer, I want to pay for my pending order using a tokenized card payment method so that my order transitions to `Processing`.
- **US-2 (Duplicate Payment Protection)**: As a Customer, I want the system to reject or deduplicate simultaneous payment submissions so that my credit card is never charged twice for the same order.
- **US-3 (Automated Refund)**: As a Customer or StoreAdmin, I want a full electronic refund initiated immediately when a paid order is cancelled prior to shipment.
- **US-4 (Payment History & Receipts)**: As a Customer, I want to retrieve receipts and payment transaction records for my orders.
- **US-5 (Failed Payment Reconciliation)**: As the System, I want payment rejections (e.g. insufficient funds, card declined) to transition the transaction to `Failed` and emit error notifications.

---

## 3. Detailed Acceptance Criteria (Given-When-Then)

### AC-1: Process Payment Intent & Charge
- **AC-1.1**: Given an existing order `ORD-123` in `PendingPayment` for $150.00 USD, When `POST /api/v1/payments` is called with `orderId`, valid payment token `tok_visa`, and `Idempotency-Key`, Then a `PaymentTransaction` record is created in status `Succeeded`, payment timestamp is recorded, and HTTP 201 Created is returned with transaction receipt.
- **AC-1.2**: Given a payment request with a declining token (e.g. `tok_declined`), When payment is processed, Then status is set to `Failed`, failure reason is recorded, and HTTP 402 Payment Required (or 400 Bad Request) is returned.
- **AC-1.3**: Given an order that has already been paid (`Succeeded`), When a subsequent payment request is sent without a cached idempotency key, Then the service rejects the request with HTTP 400 Bad Request ("Order already paid").

### AC-2: Automated Electronic Refunds
- **AC-2.1**: Given a successful payment transaction `PAY-123` for $100.00 USD, When `POST /api/v1/payments/{paymentId}/refund` is requested with reason `"OrderCancelled"` and amount $100.00, Then a `RefundRecord` is created, payment status transitions to `Refunded`, and HTTP 200 OK is returned with refund reference.
- **AC-2.2**: Given a payment transaction that was already refunded, When another refund request is submitted, Then the service rejects the request with HTTP 400 Bad Request ("Payment already refunded").
- **AC-2.3**: Given a refund request for an amount exceeding original transaction amount, Then the service rejects the request with HTTP 400 Bad Request ("Refund amount exceeds transaction balance").

### AC-3: Idempotency Protection (Principle VI)
- **AC-3.1**: Given a charge request with header `Idempotency-Key: {key}`, When identical requests are submitted concurrently or sequentially, Then the second request returns the cached identical response and does not double-charge the gateway.

### AC-4: Customer Data Isolation (Principle V)
- **AC-4.1**: Given Customer A, When querying `GET /api/v1/payments/{id}` belonging to Customer B, Then the service returns HTTP 404 Not Found.
- **AC-4.2**: Given Customer A, When querying `GET /api/v1/payments`, Then only Customer A's transactions are returned.

---

## 4. API & Integration Contracts

### Endpoints

| Verb | Path | Auth Required | Roles | Description |
| :--- | :--- | :---: | :--- | :--- |
| `POST` | `/api/v1/payments` | Yes | Customer | Charge payment method for order (`Idempotency-Key` required) |
| `GET` | `/api/v1/payments/{id}` | Yes | Customer, StoreAdmin | Get payment transaction details by ID |
| `GET` | `/api/v1/payments` | Yes | Customer, StoreAdmin | List customer payments (or all for admin) |
| `POST` | `/api/v1/payments/{id}/refund` | Yes | Customer, StoreAdmin | Process electronic refund for payment |
| `GET` | `/api/v1/payments/order/{orderId}` | Yes | Customer, StoreAdmin | Get payment status by order ID |

---

## 5. Automated Test Criteria (MANDATORY)

### 5.1 Unit Test Criteria
- `PaymentTransaction` state machine transitions: `Initiated` $\to$ `Succeeded`, `Initiated` $\to$ `Failed`, `Succeeded` $\to$ `Refunded`.
- Re-charging an already paid transaction throws `PaymentDomainException`.
- Refunding more than the original charge amount throws `PaymentDomainException`.
- Refunding a failed transaction throws `PaymentDomainException`.

### 5.2 Integration Test Criteria
- Persistence: Store `PaymentTransaction` and `RefundRecord` in PostgreSQL `paymentdb` / in-memory DB and retrieve accurately.
- Gateway mock integration: Verify gateway provider receives correct token, currency, and amount.

### 5.3 Automated End-to-End (E2E) Test Scenarios
- **Scenario E2E-01 (Successful Card Payment)**: Customer submits valid payment token for order. Assert 201 Created, status `Succeeded`, gateway transaction reference generated.
- **Scenario E2E-02 (Declined Card Payment)**: Customer submits invalid/declined token. Assert 400/402, status `Failed`, failure code recorded.
- **Scenario E2E-03 (Automated Post-Payment Refund)**: Process successful payment. Trigger refund. Assert status `Refunded`, refund amount matches, refund reference present.
- **Scenario E2E-04 (Duplicate Charge Prevention via Idempotency)**: Submit payment with `Idempotency-Key`. Re-submit with same key. Assert single charge and identical cached response.
- **Scenario E2E-05 (Customer Multi-Tenant Isolation)**: Customer B cannot view Customer A's payment details.
- **Scenario E2E-06 (Double Refund Rejection)**: Attempting to refund an already refunded payment is rejected with 400 Bad Request.
