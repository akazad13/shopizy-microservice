# Specification: Promotion & Coupon Service (`promotion-service`)

## 1. Executive Summary & Objectives
Module 8 (`promotion-service`) delivers dynamic promotion rules, coupon validation, automated calculation engines, and discount safety limits. It supports percentage and fixed-amount discounts, minimum subtotal qualifying thresholds, Buy-X-Get-Y (BOGO) offers, category-specific eligibility restrictions, and non-negotiable safety caps (maximum discount ceilings, global usage limits, and strict activation date windows) preventing coupon exploit anomalies.

## 2. Personas & User Stories
- **US-1 (Shopper - Apply Qualifying Discount)**: As a shopper, I want to apply a valid promo code (percentage or fixed amount) to my cart/order subtotal, so that my payable amount is reduced accurately.
- **US-2 (Shopper - Minimum Spend Requirement)**: As a shopper, I want clear feedback when my order subtotal does not meet the minimum spend requirement for a coupon, so that I understand why the discount was not applied.
- **US-3 (Shopper - BOGO Promotions)**: As a shopper, when I purchase qualifying items in an active Buy-X-Get-Y campaign, I want the system to automatically calculate the discounted or free item amount, so that I receive the incentive transparently.
- **US-4 (Store Administrator - Campaign Configuration)**: As a store administrator, I want to create and configure promotion campaigns with start/end date windows, category eligibility rules, global usage limits, and safety discount caps, so that marketing campaigns are profitable and secure.
- **US-5 (Store Administrator - Fraud & Over-Discount Protection)**: As a store administrator, I want maximum discount caps enforced strictly so that a percentage discount can never exceed a predefined monetary ceiling even on large carts.

## 3. Detailed Acceptance Criteria (Given-When-Then)
- **AC-1.1 (Percentage Discount with Cap)**: Given an active coupon `"SAVE20"` (20% off, max discount $50), When applied to an eligible subtotal of $300, Then the discount applied is capped at $50 (not $60).
- **AC-1.2 (Fixed Amount Discount)**: Given an active coupon `"FLAT15"` ($15 off, min spend $50), When applied to an order subtotal of $80, Then the discount applied is $15 and payable subtotal is $65.
- **AC-2.1 (Minimum Spend Threshold Enforcement)**: Given a coupon `"TIER100"` requiring minimum spend of $100, When applied to a cart of $75, Then the application is rejected with error code `"Promotion.MinimumSpendNotMet"`.
- **AC-3.1 (BOGO Calculation)**: Given a BOGO rule ("Buy 2, Get 1 Free" on qualifying Category/Product), When a customer has 3 eligible items in cart, Then the lowest priced item among the 3 is discounted 100%.
- **AC-4.1 (Category Restriction)**: Given a coupon restricted to Category `"Footwear"`, When applied to a cart containing only `"Electronics"`, Then the discount is rejected with error code `"Promotion.CategoryIneligible"`.
- **AC-5.1 (Usage Limit & Expiration Gate)**: Given an expired coupon or a coupon whose `CurrentUsageCount >= MaxGlobalUsages`, When a customer attempts to apply it, Then the request is rejected with error code `"Promotion.CouponExpiredOrExhausted"`.

## 4. API & Integration Contracts
- `POST /api/v1/promotions/apply`
  - Body: `ApplyPromotionRequest(string CouponCode, decimal Subtotal, string Currency, List<CartItemDto> Items)`
  - Returns: `200 OK` with `PromotionEvaluationResult(bool IsValid, decimal DiscountAmount, string? FailureReason, string? AppliedRuleDescription)`
- `POST /api/v1/promotions/campaigns` (StoreAdmin only)
  - Creates a new promotion campaign / coupon
  - Returns `201 Created`
- `GET /api/v1/promotions/campaigns` (StoreAdmin only)
  - Returns list of active and inactive campaigns
- `DELETE /api/v1/promotions/campaigns/{id}` (StoreAdmin only)
  - Voids or deactivates a campaign

## 5. Security & Isolation Constraints
- Discount application endpoint is accessible anonymously and by authenticated shoppers.
- Campaign creation and administrative management requires `StoreAdmin` role.
- All monetary operations use `Money` value object or strict decimal precision with currency validation.

## 6. Verifiable Automated Test Scenarios
- **Unit Tests**:
  - Percentage discount calculations and safety cap ceilings.
  - Fixed-amount and minimum spend boundary tests.
  - BOGO algorithmic item evaluations.
  - Date window and usage limit checks.
- **Integration Tests**:
  - EF Core database persistence of promotion campaigns, usage counters, and concurrency protection.
- **Automated E2E Scenarios (6 Scenarios)**:
  1. **E2E-1: Percentage Discount with Safety Cap**: Capping $60 raw discount to $50 limit.
  2. **E2E-2: Fixed Discount with Minimum Spend**: $15 off on $80 subtotal.
  3. **E2E-3: Minimum Spend Rejection**: Subtotal below threshold rejected cleanly.
  4. **E2E-4: Category Eligibility**: Discount only applied to matching items in mixed basket.
  5. **E2E-5: BOGO Offer**: Buy 2 get 1 free discounts lowest eligible item.
  6. **E2E-6: Admin Campaign Management & Expiry/Exhaustion Gate**: StoreAdmin creates campaign, customer exhausts usage limit, next application rejected.
