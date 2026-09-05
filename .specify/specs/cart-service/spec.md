# Specification: Shopping Cart Service (`cart-service`)

## 1. Executive Summary & Objectives
The **Shopping Cart Service** is a high-performance, low-latency microservice in the Shopizy platform. It manages ephemeral and persistent shopping carts for both guest shoppers and authenticated customers. Built on Redis with sub-millisecond read/write latency, the service enforces price snapshotting upon adding items to protect shoppers from silent in-flight price increases while proactively flagging price discrepancies prior to order checkout.

### Primary Objectives:
1. Provide lightning-fast shopping cart mutations (sub-10ms p95 latency) backed by Redis.
2. Support anonymous guest carts identified by secure session UUIDs and authenticated customer carts scoped by JWT `sub` claims.
3. Provide seamless, loss-free cart merging when a guest user authenticates.
4. Capture immutable price snapshots when items are added to the cart.
5. Perform live price discrepancy checks against current catalog prices, providing clear notification to the shopper if items increased or decreased in price.
6. Enforce strict customer isolation (Constitution Principle V) and idempotency on mutating operations (Constitution Principle VI).

---

## 2. Personas & User Stories

### Personas:
- **Guest Shopper**: An anonymous visitor browsing the storefront who adds products to a temporary cart before creating an account or logging in.
- **Authenticated Customer**: A registered buyer who expects their cart to persist across sessions and devices, and merge with any guest cart created prior to login.
- **Storefront Client / Checkout Engine**: The frontend or downstream order service that queries cart items, totals, and validated price snapshots to initialize order placement.

### User Stories:
- **US-1 (Guest Cart Creation & Mutation)**: As a Guest Shopper, I want to add items with specific variants to my cart, adjust quantities, and remove items, so that I can prepare my purchase without having to register first.
- **US-2 (Cart Merging on Login)**: As an Authenticated Customer logging into the storefront, I want my active guest cart to seamlessly merge with my saved customer cart, so that I do not lose items I added before logging in.
- **US-3 (Price Snapshotting & Discrepancy Alert)**: As a Customer reviewing my cart, I want the system to alert me if any item's catalog price changed since I added it to my cart, so that I have complete transparency before placing my order.
- **US-4 (Customer Data Isolation)**: As an Authenticated Customer, I want my shopping cart to be strictly confidential and isolated from other customers, so that no unauthorized user can inspect or alter my cart contents.
- **US-5 (Cart Expiration & TTL)**: As a System Operator, I want inactive guest carts to expire after 7 days and customer carts to persist for 30 days of inactivity in Redis, so that memory usage remains bounded.

---

## 3. Detailed Acceptance Criteria (Given-When-Then)

### AC-1: Add Item to Cart
- **AC-1.1**: Given a valid `variantId`, `productId`, `productName`, `variantSku`, `unitPrice` ($29.99), and `quantity` (2), When the client sends `POST /api/v1/cart/items`, Then the item is stored in the cart in Redis with a price snapshot of $29.99 and line subtotal of $59.98.
- **AC-1.2**: Given an item already exists in the cart with quantity 2, When the client adds the same variant with quantity 3, Then the item's quantity is incremented to 5 and the price snapshot reflects the latest unit price.
- **AC-1.3**: Given a request with quantity <= 0 or > 99, When the client sends `POST /api/v1/cart/items`, Then the service rejects the request with HTTP 400 Bad Request and RFC 7807 problem details.

### AC-2: Update and Remove Items
- **AC-2.1**: Given an existing cart containing variant `V1`, When the client sends `PUT /api/v1/cart/items/{variantId}` with quantity 4, Then the cart item quantity is updated to 4 and the cart subtotal recalculates immediately.
- **AC-2.2**: Given an existing cart containing variant `V1`, When the client sends `DELETE /api/v1/cart/items/{variantId}`, Then the item is removed from the cart; if the cart becomes empty, an empty cart object is returned.
- **AC-2.3**: Given an existing cart, When the client sends `DELETE /api/v1/cart`, Then all items are deleted and the cart is reset in Redis.

### AC-3: Cart Merging
- **AC-3.1**: Given an authenticated customer with an existing cart (Item A: qty 1) and a guest cart ID (Item B: qty 2), When the client sends `POST /api/v1/cart/merge` with `guestCartId`, Then the customer cart contains both Item A (qty 1) and Item B (qty 2), and the guest cart is deleted from Redis.
- **AC-3.2**: Given an authenticated customer cart containing Item A (qty 2) and a guest cart containing Item A (qty 3), When merged, Then the customer cart contains Item A with quantity 5 (capped at max 99).

### AC-4: Price Discrepancy Detection
- **AC-4.1**: Given an item added to the cart at snapshot price $50.00, When the cart is retrieved via `GET /api/v1/cart` and the live catalog price for that variant is now $55.00, Then the response includes a `PriceDiscrepancy` alert indicating the price increased by $5.00 (`OldPrice`: 50.00, `CurrentPrice`: 55.00, `HasPriceChanged`: true).
- **AC-4.2**: Given an item whose price has not changed ($50.00 == $50.00), When retrieved, Then `HasPriceChanged` is `false` and no alert is triggered.

### AC-5: Security & Multi-Tenant Isolation
- **AC-5.1**: Given an authenticated customer with ID `Cust-123`, When requesting `GET /api/v1/cart`, Then the service resolves the cart ID from the JWT `sub` claim and returns only `Cust-123`'s cart.
- **AC-5.2**: Given an unauthenticated request without a valid guest cart header/identifier, Then the service rejects or issues a new guest cart ID, never exposing another user's cart.

---

## 4. API & Integration Contracts

### Endpoints:
All endpoints are prefixed with `/api/v1/cart`.

| HTTP Verb | Path | Auth | Description | Status Codes |
| :--- | :--- | :--- | :--- | :--- |
| `GET` | `/api/v1/cart` | Optional / Bearer | Get current cart with live price discrepancy checks | 200 OK |
| `POST` | `/api/v1/cart/items` | Optional / Bearer | Add item or increment quantity in cart | 200 OK, 400 Bad Request |
| `PUT` | `/api/v1/cart/items/{variantId}` | Optional / Bearer | Update item quantity | 200 OK, 404 Not Found, 400 Bad Request |
| `DELETE` | `/api/v1/cart/items/{variantId}` | Optional / Bearer | Remove item from cart | 200 OK, 404 Not Found |
| `DELETE` | `/api/v1/cart` | Optional / Bearer | Clear all items from cart | 204 No Content |
| `POST` | `/api/v1/cart/merge` | Required (Bearer) | Merge guest cart into customer cart | 200 OK, 400 Bad Request, 401 Unauthorized |

### Request Headers:
- `X-Guest-Cart-Id`: String UUID representing anonymous guest cart when unauthenticated.
- `Authorization`: `Bearer <jwt>` when authenticated.
- `Idempotency-Key`: UUID header to prevent duplicate item additions on client network retries.

### DTO Schemas:

#### `AddToCartRequest`:
```json
{
  "productId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "variantId": "7bb85f64-5717-4562-b3fc-2c963f66afa6",
  "productName": "Wireless Noise Cancelling Headphones",
  "variantSku": "TECH-NOISE-BLK",
  "attributes": {
    "Color": "Matte Black",
    "Style": "Over-Ear"
  },
  "quantity": 1,
  "unitPrice": {
    "amount": 199.99,
    "currency": "USD"
  }
}
```

#### `CartResponse`:
```json
{
  "cartId": "customer:d7b003a8-4bb9-4b6d-a110-38e07fb31b67",
  "customerId": "d7b003a8-4bb9-4b6d-a110-38e07fb31b67",
  "items": [
    {
      "productId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "variantId": "7bb85f64-5717-4562-b3fc-2c963f66afa6",
      "productName": "Wireless Noise Cancelling Headphones",
      "variantSku": "TECH-NOISE-BLK",
      "attributes": {
        "Color": "Matte Black"
      },
      "quantity": 2,
      "snapshotPrice": {
        "amount": 199.99,
        "currency": "USD"
      },
      "currentCatalogPrice": {
        "amount": 199.99,
        "currency": "USD"
      },
      "hasPriceChanged": false,
      "priceDifference": 0.0,
      "lineTotal": {
        "amount": 399.98,
        "currency": "USD"
      },
      "addedAtUtc": "2026-09-06T00:00:00Z"
    }
  ],
  "totalItemsCount": 2,
  "subtotal": {
    "amount": 399.98,
    "currency": "USD"
  },
  "hasAnyPriceDiscrepancy": false,
  "updatedAtUtc": "2026-09-06T00:00:00Z"
}
```

---

## 5. Data Models & State Transitions

### Redis Key Patterns:
- Guest Cart: `cart:guest:{guestUuid}` (TTL: 7 days)
- Customer Cart: `cart:customer:{customerUuid}` (TTL: 30 days)

### Domain Entities & Value Objects:
- **`Cart` (Aggregate Root)**:
  - `Id`: string
  - `CustomerId`: Guid?
  - `Items`: List of `CartItem`
  - `UpdatedAtUtc`: DateTimeOffset
  - Methods: `AddItem()`, `UpdateItemQuantity()`, `RemoveItem()`, `Clear()`, `MergeWith()`, `CheckDiscrepancies()`
- **`CartItem` (Value Object / Entity)**:
  - `ProductId`: Guid
  - `VariantId`: Guid
  - `ProductName`: string
  - `VariantSku`: string
  - `Attributes`: Dictionary<string, string>
  - `Quantity`: int (1..99)
  - `UnitPrice`: Money
  - `AddedAtUtc`: DateTimeOffset

---

## 6. Automated Test Criteria (MANDATORY)

### 6.1 Unit Test Criteria (`tests/Shopizy.CartService.UnitTests`)
1. **Cart Creation & Item Validation**:
   - Creating cart with invalid ID throws `DomainException`.
   - Adding item with zero or negative quantity throws `DomainException`.
   - Adding item with quantity > 99 throws `DomainException`.
   - Adding item with null product name or empty variant SKU throws `DomainException`.
2. **Item Mutation & Quantity Accumulation**:
   - Adding the same variant increments quantity up to 99.
   - Adding the same variant beyond 99 caps or throws validation error.
   - Updating item quantity changes line total and cart subtotal accurately.
   - Removing item correctly reduces item count and recalculates subtotal.
3. **Cart Merging Logic**:
   - Merging empty customer cart with guest cart adopts all guest items.
   - Merging customer cart with overlapping guest items sums quantities correctly.
   - Merging with non-overlapping items retains both sets of items.
4. **Price Discrepancy Logic**:
   - When current price == snapshot price, `hasPriceChanged` is false.
   - When current price > snapshot price, discrepancy reflects increase.
   - When current price < snapshot price, discrepancy reflects discount.

### 6.2 Integration Test Criteria (`tests/Shopizy.CartService.IntegrationTests`)
1. **Redis Repository Persistence**:
   - Save cart to Redis and retrieve it; all fields and item attributes deserialize accurately.
   - Delete cart removes the Redis key.
   - Updating cart preserves expiration TTL.
2. **Catalog Price Service Mock / Client**:
   - Live price fetching compares cached snapshot with catalog response.

### 6.3 Automated End-to-End (E2E) Test Scenarios (`tests/Shopizy.CartService.E2ETests`)
- **Scenario E2E-01: Guest Cart Lifecycle & Quantity Updates**:
  - Step 1: Guest sends `POST /api/v1/cart/items` with `X-Guest-Cart-Id: guest-123`, adding Variant A ($40.00, qty 2). Expect 200 OK + subtotal $80.00.
  - Step 2: Guest sends `PUT /api/v1/cart/items/{variantAId}` with qty 3. Expect 200 OK + subtotal $120.00.
  - Step 3: Guest sends `GET /api/v1/cart` with `X-Guest-Cart-Id: guest-123`. Expect 200 OK with 1 item, total count 3.
  - Step 4: Guest sends `DELETE /api/v1/cart/items/{variantAId}`. Expect 200 OK + empty cart.
- **Scenario E2E-02: Guest Cart Merging into Customer Cart on Login**:
  - Step 1: Guest adds Variant A (qty 2) with `X-Guest-Cart-Id: guest-merge`.
  - Step 2: Authenticated customer logs in with JWT and sends `POST /api/v1/cart/merge` with body `{"guestCartId": "guest-merge"}`.
  - Step 3: GET customer cart returns Variant A (qty 2).
  - Step 4: GET guest cart returns empty (guest cart was cleaned up).
- **Scenario E2E-03: Price Discrepancy Detection on Cart Review**:
  - Step 1: Customer adds Variant B at snapshot price $100.00.
  - Step 2: Catalog price changes to $115.00.
  - Step 3: Customer requests `GET /api/v1/cart`. Expect 200 OK with `hasPriceChanged: true`, `priceDifference: 15.00`, `currentCatalogPrice: 115.00`.
- **Scenario E2E-04: Multi-Tenant Customer Data Isolation (Principle V)**:
  - Step 1: Customer A adds items to their authenticated cart.
  - Step 2: Customer B attempts to view or modify Customer A's cart. Expect 403 Forbidden or automatic isolation to Customer B's empty cart.
- **Scenario E2E-05: Idempotency Key Protection (Principle VI)**:
  - Step 1: Client sends `POST /api/v1/cart/items` with `Idempotency-Key: 11111111-2222-3333-4444-555555555555`. Expect 200 OK.
  - Step 2: Network retry re-sends exact same request with same key. Expect identical cached response without duplicating items.
- **Scenario E2E-06: Cart Clear Reset**:
  - Step 1: Customer sends `DELETE /api/v1/cart`. Expect 204 No Content.
  - Step 2: `GET /api/v1/cart` returns empty items array and $0.00 subtotal.

---

## 7. Non-Functional & Security Requirements
- **Performance**: Read and write cart mutations execute under 15ms in Redis.
- **Clean Architecture Compliance**: Domain contains zero EF Core, Redis, or Web references.
- **Zero Trust Security (Principle V)**: Customer carts derive identity strictly from validated JWT claims.
- **Reliability & Resilience**: Redis connection utilizes Polly automatic retries with circuit breaker.
