# Specification: Reviews, Ratings & Wishlists (`review-service`)

## 1. Executive Summary & Objectives
The **Reviews, Ratings & Wishlists Service** (`review-service`) is the social proof and customer retention engine for the Shopizy microservices ecosystem. It enables customers to submit 1–5 star reviews with optional photos, computes accurate product aggregate ratings, awards Verified Buyer badges by confirming delivered order fulfillment status, provides community helpfulness upvoting/downvoting, and manages customer wishlists with item tracking and price change awareness.

---

## 2. Personas & User Stories

- **US-1 (Product Reviews & Ratings)**: As an online shopper, I want to submit 1 to 5 star ratings and detailed written reviews for products I've purchased, so that I can share feedback and inform other buyers.
- **US-2 (Verified Buyer Badge)**: As an online shopper, I want reviews from verified buyers who completed and received orders for the product to feature a distinct Verified Buyer badge, so that reviews represent genuine product experiences.
- **US-3 (Review Helpfulness Voting)**: As an online shopper, I want to vote whether a review was helpful or unhelpful, so that the community surfaces high-quality reviews at the top.
- **US-4 (Product Aggregate Ratings)**: As an online shopper browsing catalog items, I want to see accurate aggregate statistics (average rating and total review count), so that I can quickly evaluate product popularity and quality.
- **US-5 (Customer Wishlist Management & Isolation)**: As an online shopper, I want to save products to my personal wishlist, remove items, and view my wishlist with complete privacy (zero cross-customer data leakage).

---

## 3. Detailed Acceptance Criteria (Given-When-Then)

### AC-1.1: Product Review Submission & Rating Scale
- **Given** an authenticated customer,
- **When** the customer submits a review with rating $R$ where $1 \le R \le 5$, title, comment, and optional image URLs,
- **Then** the service persists the review with status `Published` (or `PendingModeration` if flagged) and recalculates product aggregate rating.
- **And** if rating is $< 1$ or $> 5$, the service rejects submission with validation error.

### AC-2.1: Verified Buyer Badge Validation
- **Given** a customer submitting a review for `ProductId`,
- **When** the service checks order history for that customer and product:
  - If a delivered order containing `ProductId` is verified, the review is assigned `IsVerifiedBuyer = true`.
  - If no delivered order exists for that customer and product, the review is created with `IsVerifiedBuyer = false`.

### AC-3.1: Helpful Voting System
- **Given** an existing published review,
- **When** an authenticated user votes `Helpful` or `Unhelpful`,
- **Then** the review's helpful/unhelpful counter increments.
- **And** a customer cannot vote multiple times on the same review with conflicting or duplicate votes (idempotent or one-vote-per-user enforcement).

### AC-4.1: Aggregate Product Rating Computation
- **Given** multiple published reviews for `ProductId`,
- **When** `GET /api/v1/reviews/product/{productId}/summary` is queried,
- **Then** the service returns `AverageRating` (rounded to 1 decimal place), `TotalReviews`, and star distribution breakdown ($1\star$ to $5\star$ counts).
- **And** if no reviews exist, returns average `0.0` and count `0`.

### AC-5.1: Wishlist Management & Customer Isolation
- **Given** customer A and customer B,
- **When** customer A adds items to their wishlist and queries `GET /api/v1/wishlists/my`,
- **Then** customer A receives only their items.
- **And** if customer A attempts to access customer B's wishlist via `GET /api/v1/wishlists/user/{userId}`, the service returns `403 Forbidden` according to Constitution Principle V.

---

## 4. API & Integration Contracts

### REST Endpoints
- `POST /api/v1/reviews` (Customer auth required)
  - Request: `CreateReviewRequest(Guid ProductId, int Rating, string Title, string Comment, List<string>? ImageUrls, Guid? VerifiedOrderId)`
  - Response: `201 Created` with `ReviewResponse`
- `GET /api/v1/reviews/product/{productId}` (Public)
  - Query: `?page=1&pageSize=10&verifiedOnly=false`
  - Response: `200 OK` with `PagedResult<ReviewResponse>`
- `GET /api/v1/reviews/product/{productId}/summary` (Public)
  - Response: `200 OK` with `ProductReviewSummaryResponse(Guid ProductId, decimal AverageRating, int TotalReviews, Dictionary<int, int> RatingDistribution)`
- `POST /api/v1/reviews/{id}/vote` (Customer auth required)
  - Request: `VoteReviewRequest(bool IsHelpful)`
  - Response: `200 OK` with `ReviewVoteSummaryResponse(Guid ReviewId, int HelpfulCount, int UnhelpfulCount)`
- `DELETE /api/v1/reviews/{id}` (Customer owner or StoreAdmin)
  - Response: `204 NoContent`
- `POST /api/v1/wishlists/items` (Customer auth required)
  - Request: `AddWishlistItemRequest(Guid ProductId, string ProductName, string Sku, decimal PriceSnapshot)`
  - Response: `201 Created` with `WishlistItemResponse`
- `GET /api/v1/wishlists/my` (Customer auth required)
  - Response: `200 OK` with `WishlistResponse(Guid Id, Guid CustomerId, List<WishlistItemResponse> Items)`
- `DELETE /api/v1/wishlists/items/{productId}` (Customer auth required)
  - Response: `204 NoContent`
- `GET /api/v1/wishlists/user/{userId}` (StoreAdmin or Matching Customer)
  - Response: `200 OK` with `WishlistResponse` or `403 Forbidden`

---

## 5. Data Models & State Transitions

### Entities
1. **Review**:
   - `Id` (Guid, PK)
   - `ProductId` (Guid, Indexed)
   - `CustomerId` (Guid, Indexed)
   - `CustomerName` (string)
   - `Rating` (int, 1–5 range check)
   - `Title` (string, max 150 chars)
   - `Comment` (string, max 3000 chars)
   - `ImageUrls` (List<string>)
   - `IsVerifiedBuyer` (bool)
   - `HelpfulVotes` (int, default 0)
   - `UnhelpfulVotes` (int, default 0)
   - `CreatedAtUtc` (DateTime)
   - `UpdatedAtUtc` (DateTime?)
2. **ReviewVote**:
   - `Id` (Guid, PK)
   - `ReviewId` (Guid, FK)
   - `UserId` (Guid)
   - `IsHelpful` (bool)
   - `VotedAtUtc` (DateTime)
   - *Constraint*: Unique on `(ReviewId, UserId)`
3. **Wishlist**:
   - `Id` (Guid, PK)
   - `CustomerId` (Guid, Unique Index)
   - `CreatedAtUtc` (DateTime)
   - `Items` (Collection of `WishlistItem`)
4. **WishlistItem**:
   - `Id` (Guid, PK)
   - `WishlistId` (Guid, FK)
   - `ProductId` (Guid)
   - `ProductName` (string)
   - `Sku` (string)
   - `PriceSnapshot` (decimal)
   - `AddedAtUtc` (DateTime)
   - *Constraint*: Unique on `(WishlistId, ProductId)`

---

## 6. Automated Test Criteria (MANDATORY)

### 6.1 Unit Test Criteria
- `Review.Create` with rating $< 1$ or $> 5$ throws `ReviewDomainException`.
- `Review.Create` with empty title or comment throws `ReviewDomainException`.
- `Review.AddVote` correctly increments helpful or unhelpful count and handles vote flipping.
- `ProductReviewSummary` calculation with weighted average and zero-review baseline.

### 6.2 Integration Test Criteria
- `ReviewRepository` persists and retrieves reviews by `ProductId` with rating filters.
- `WishlistRepository` adds, removes, and retrieves wishlist items with customer isolation.
- Unique constraint enforcement on `(ReviewId, UserId)` vote and `(WishlistId, ProductId)`.

### 6.3 Automated End-to-End (E2E) Test Scenarios
- **E2E-01 (Review Creation & Verification)**: Authenticated customer submits review with verified order -> returns 201 Created with `IsVerifiedBuyer = true`.
- **E2E-02 (Unverified Buyer Review Submission)**: Customer submits review without verified order -> returns 201 Created with `IsVerifiedBuyer = false`.
- **E2E-03 (Aggregate Rating Summary)**: Multiple reviews submitted for product -> summary endpoint accurately reflects rounded average rating and total counts.
- **E2E-04 (Review Helpfulness Voting)**: Customer votes helpful on review -> helpful count increments; duplicate vote updates cleanly.
- **E2E-05 (Wishlist CRUD & Customer Isolation)**: Customer adds item to wishlist, queries `/my` -> item returned. Another customer queries `/user/{userId}` for first customer -> receives 403 Forbidden.
- **E2E-06 (RBAC & Unauthorized Protection)**: Unauthenticated request to submit review or mutate wishlist returns 401 Unauthorized.

---

## 7. Non-Functional & Security Requirements
- **Response Latency**: Review summary and public list endpoints return in $< 50\text{ ms}$ for cached/indexed queries.
- **Customer Privacy**: Wishlist items and customer review histories adhere to Principle V zero-trust isolation.
- **Input Validation**: Text inputs sanitized against XSS; rating bounded to $[1, 5]$.
