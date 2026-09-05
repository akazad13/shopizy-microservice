# Specification: Order & Inventory Service (`order-service`)

## 1. Executive Summary & Objectives
The **Order & Inventory Service** (`order-service`) is the transaction orchestrator of the Shopizy microservices platform. It manages order lifecycle progression, enforces zero-overselling through atomic stock reservations, guarantees duplicate submission protection (idempotency), automates 15-minute unpaid order expiration with inventory restocking, and emits domain integration events via the Transactional Outbox pattern.

### Key Objectives:
- **Atomic Stock Reservation**: Prevent overselling by acquiring atomic reservations before order confirmation.
- **Strict Status Progression**: `PendingPayment` $\to$ `Processing` $\to$ `Shipping` $\to$ `Delivered` / `Cancelled`.
- **15-Minute Expiration**: Automatically expire unpaid orders and release reserved stock.
- **Idempotent Order Creation**: Protect checkout against network retries or double-clicking using `Idempotency-Key`.
- **Customer Multi-Tenant Isolation**: Customers can only view and mutate their own orders; `StoreAdmin` can view all orders and transition fulfillment states.

---

## 2. Personas & User Stories

- **US-1 (Customer Checkout)**: As an authenticated Customer, I want to create an order from my cart items so that my inventory is reserved and I receive an order summary with total pricing.
- **US-2 (Zero Overselling)**: As a Customer, I want the system to reject my order if insufficient stock exists, so that I am never charged for out-of-stock items.
- **US-3 (Order Expiration)**: As the System / StoreAdmin, I want orders that remain unpaid for 15 minutes to automatically cancel and release reserved stock back to available inventory.
- **US-4 (Order Cancellation)**: As a Customer or StoreAdmin, I want to cancel an order before it has shipped so that my stock is restocked and payment/refund can be handled.
- **US-5 (Fulfillment Status Progression)**: As a StoreAdmin, I want to advance order status from `Processing` to `Shipping` and `Delivered` so customers are updated.
- **US-6 (Customer Isolation)**: As a Customer, I want to ensure my order details and addresses are private and cannot be queried by other shoppers.

---

## 3. Detailed Acceptance Criteria (Given-When-Then)

### AC-1: Order Creation & Stock Reservation
- **AC-1.1**: Given an authenticated customer with items in stock, When `POST /api/v1/orders` is called with cart items and shipping address, Then an order is created with status `PendingPayment`, an expiration time set to `UtcNow + 15 minutes`, inventory is reserved, and HTTP 201 Created is returned with order details.
- **AC-1.2**: Given an item with available stock of 2, When a customer requests quantity 5, Then the order creation fails with HTTP 409 Conflict / 400 Bad Request ("Insufficient stock for variant {id}"), and zero stock is deducted.
- **AC-1.3**: Given a request with header `Idempotency-Key: {key}`, When identical requests are submitted concurrently or sequentially, Then the second request returns the cached identical response and does not reserve duplicate inventory.

### AC-2: Unpaid Order Expiration & Restocking
- **AC-2.1**: Given an order in `PendingPayment` status created > 15 minutes ago, When the expiration worker runs (or `POST /api/v1/orders/{id}/expire` is triggered), Then the order status transitions to `Cancelled`, the reason is marked `PaymentExpired`, and reserved inventory is credited back.
- **AC-2.2**: Given an order in `Processing` or `Shipping` status, When an expiration attempt occurs, Then the transition is rejected and status remains unchanged.

### AC-3: Customer & Admin Cancellation
- **AC-3.1**: Given an order in `PendingPayment` or `Processing` status, When the customer or admin requests `POST /api/v1/orders/{id}/cancel`, Then the status transitions to `Cancelled` and all item stock quantities are restored to available inventory.
- **AC-3.2**: Given an order in `Shipping` or `Delivered` status, When cancellation is requested, Then the system returns HTTP 400 Bad Request ("Cannot cancel order after shipment dispatch").

### AC-4: Fulfillment Status Progression
- **AC-4.1**: Given an order in `PendingPayment`, When payment confirmation event/endpoint is processed (`POST /api/v1/orders/{id}/pay`), Then status transitions to `Processing`.
- **AC-4.2**: Given an order in `Processing`, When StoreAdmin sends `POST /api/v1/orders/{id}/ship`, Then status transitions to `Shipping`.
- **AC-4.3**: Given an order in `Shipping`, When StoreAdmin sends `POST /api/v1/orders/{id}/deliver`, Then status transitions to `Delivered`.

### AC-5: Multi-Tenant Data Isolation (Principle V)
- **AC-5.1**: Given Customer A, When querying `GET /api/v1/orders/{id}` belonging to Customer B, Then the service returns HTTP 404 Not Found (or 403 Forbidden).
- **AC-5.2**: Given Customer A, When querying `GET /api/v1/orders`, Then only Customer A's orders are returned in the list.

---

## 4. API & Integration Contracts

### Endpoints

| Verb | Path | Auth Required | Roles | Description |
| :--- | :--- | :---: | :--- | :--- |
| `POST` | `/api/v1/orders` | Yes | Customer | Create order & reserve stock (requires `Idempotency-Key`) |
| `GET` | `/api/v1/orders/{id}` | Yes | Customer, StoreAdmin | Get order by ID (customer isolated) |
| `GET` | `/api/v1/orders` | Yes | Customer, StoreAdmin | List customer's orders (or all for admin) |
| `POST` | `/api/v1/orders/{id}/pay` | Yes | StoreAdmin / System | Mark order as paid (`Processing`) |
| `POST` | `/api/v1/orders/{id}/cancel` | Yes | Customer, StoreAdmin | Cancel order & restore inventory |
| `POST` | `/api/v1/orders/{id}/expire` | Yes | StoreAdmin / Worker | Expire unpaid order (>15 min) & restore inventory |
| `POST` | `/api/v1/orders/{id}/ship` | Yes | StoreAdmin | Mark order shipped |
| `POST` | `/api/v1/orders/{id}/deliver` | Yes | StoreAdmin | Mark order delivered |
| `GET` | `/api/v1/inventory/{variantId}` | Yes | Customer, StoreAdmin | Check variant inventory stock level |
| `POST` | `/api/v1/inventory/{variantId}/adjust` | Yes | StoreAdmin | Adjust stock quantity for variant |

### Request & Response Schemas

#### Create Order Request:
```json
{
  "items": [
    {
      "productId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "variantId": "7cb62a19-1234-4bc8-8e6f-9988aabbccdd",
      "productName": "Wireless Noise-Cancelling Headphones",
      "variantSku": "WH-1000XM5-BLK",
      "quantity": 2,
      "unitPrice": { "amount": 299.99, "currency": "USD" }
    }
  ],
  "shippingAddress": {
    "fullName": "Alex Mercer",
    "street": "123 Market Street, Apt 4B",
    "city": "San Francisco",
    "state": "CA",
    "postalCode": "94103",
    "country": "USA"
  }
}
```

#### Order Response:
```json
{
  "id": "9fa85f64-5717-4562-b3fc-2c963f66afa6",
  "orderNumber": "ORD-20260906-0001",
  "customerId": "1fa85f64-5717-4562-b3fc-2c963f66afa6",
  "status": "PendingPayment",
  "items": [
    {
      "productId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "variantId": "7cb62a19-1234-4bc8-8e6f-9988aabbccdd",
      "productName": "Wireless Noise-Cancelling Headphones",
      "variantSku": "WH-1000XM5-BLK",
      "quantity": 2,
      "unitPrice": { "amount": 299.99, "currency": "USD" },
      "lineTotal": { "amount": 599.98, "currency": "USD" }
    }
  ],
  "totalAmount": { "amount": 599.98, "currency": "USD" },
  "expiresAtUtc": "2026-09-06T02:30:00Z",
  "createdAtUtc": "2026-09-06T02:15:00Z"
}
```

---

## 5. Data Models & State Transitions

### State Machine:
```
[PendingPayment] ──── (pay within 15 min) ────> [Processing] ──── (ship) ────> [Shipping] ──── (deliver) ────> [Delivered]
       │                                             │
       ├──── (unpaid > 15 min) ──> [Cancelled]       └──── (cancel prior to ship) ──> [Cancelled]
       └──── (customer cancels) ─> [Cancelled]
```

### Entities:
- **`Order` (Aggregate Root)**: `Id`, `OrderNumber`, `CustomerId`, `OrderStatus`, `ShippingAddress`, `List<OrderItem>`, `TotalAmount`, `CreatedAtUtc`, `ExpiresAtUtc`, `CancelledAtUtc`, `CancellationReason`.
- **`OrderItem` (Entity)**: `Id`, `OrderId`, `ProductId`, `VariantId`, `ProductName`, `VariantSku`, `Quantity`, `UnitPrice`, `LineTotal`.
- **`InventoryItem` (Entity / Stock Register)**: `VariantId`, `AvailableStock`, `ReservedStock`, `Version` (optimistic lock).
- **`ShippingAddress` (Value Object)**: `FullName`, `Street`, `City`, `State`, `PostalCode`, `Country`.

---

## 6. Automated Test Criteria (MANDATORY)

### 6.1 Unit Test Criteria
- `Order` state transitions: valid transitions succeed; invalid transitions throw `DomainException`.
- Price calculation: multiple items, quantity multiplication, total sums.
- Expiration check: order expired when `UtcNow > ExpiresAtUtc` and status is `PendingPayment`.
- Inventory reservation: reserving more than available throws domain exception; reserving available moves stock from Available to Reserved.
- Inventory release/restock: releasing reservation moves stock back from Reserved to Available.

### 6.2 Integration Test Criteria
- EF Core persistence: save `Order` with `OrderItems`, retrieve by ID, cascade updates.
- Concurrency test: simultaneous stock reservation attempts on limited inventory correctly prevent overselling.
- Multi-tenant query filter: customer queries are automatically partitioned by `CustomerId`.

### 6.3 Automated End-to-End (E2E) Test Scenarios
- **Scenario E2E-01 (Successful Order Checkout & Stock Reservation)**:
  - Add initial inventory of 10 units for Variant V1.
  - Customer creates order for 2 units.
  - Assert status is `PendingPayment`, expires in 15 minutes, remaining available stock is 8, reserved is 2.
- **Scenario E2E-02 (Zero-Overselling Stock Depletion Rejection)**:
  - Variant V2 has stock of 1.
  - Customer attempts to order 2 units.
  - Assert response is 400 Bad Request with insufficient stock error, inventory remains 1 available, 0 reserved.
- **Scenario E2E-03 (15-Minute Unpaid Expiration & Auto-Restock)**:
  - Create order with 3 units reserved.
  - Simulate clock advancing > 15 minutes and trigger expiration worker.
  - Assert order status is `Cancelled`, available stock returns to original count, reserved stock drops to 0.
- **Scenario E2E-04 (Order Cancellation & Restocking Prior to Shipment)**:
  - Create order and mark as `Processing` (paid).
  - Cancel order before shipping.
  - Assert order status is `Cancelled` and reserved/deducted stock is restored.
- **Scenario E2E-05 (Customer Multi-Tenant Isolation)**:
  - Customer A creates Order O1.
  - Customer B attempts `GET /api/v1/orders/{O1.Id}`.
  - Assert HTTP 404 Not Found.
- **Scenario E2E-06 (Idempotent Checkout Protection)**:
  - Submit order creation request with `Idempotency-Key: X`.
  - Re-submit identical request with same key.
  - Assert identical 201 response, but only one order created and single stock reservation applied.
