# Specification: Shipping & Tracking Service (`shipping-service`)

## 1. Executive Summary & Objectives
Module 9 (`shipping-service`) delivers multi-carrier shipping fee calculation (USPS, UPS, FedEx, DHL), basket-level free shipping threshold evaluation ($75), shipment tracking creation, and milestone update recording (*Label Created -> Package Received -> In Transit -> Out for Delivery -> Delivered*). It provides dynamic shipping estimates based on parcel weight, dimensions, and destination postal code while maintaining auditable shipment tracking histories.

## 2. Personas & User Stories
- **US-1 (Shopper - Shipping Rate Estimation)**: As a shopper at checkout, I want to calculate shipping options (Ground, Express, Overnight) across major carriers with delivery estimates, so that I can choose the option that fits my timeline and budget.
- **US-2 (Shopper - Free Shipping Qualification)**: As a shopper whose order subtotal exceeds the $75 threshold, I want the standard ground shipping fee waived automatically, so that I benefit from free shipping incentives.
- **US-3 (Shopper - Step-by-Step Package Tracking)**: As a shopper, I want to view current milestone scans and estimated arrival dates for my dispatched shipment, so that I have complete delivery visibility.
- **US-4 (Store Administrator / Carrier Webhook - Milestone Updates)**: As a carrier integration or store administrator, I want to append new milestone tracking events to an active shipment, so that package state progression is recorded accurately.

## 3. Detailed Acceptance Criteria (Given-When-Then)
- **AC-1.1 (Carrier Rate Calculation)**: Given a parcel weight of 2.5 kg and destination postal code, When shipping rates are requested, Then rate options for USPS, UPS, FedEx, and DHL with valid monetary amounts and estimated delivery days are returned.
- **AC-2.1 (Free Shipping Threshold Applied)**: Given an order subtotal of $85 (>= $75 threshold), When shipping rates are evaluated, Then standard ground shipping rate is $0.00 with description `"Free Ground Shipping (Order over $75)"`.
- **AC-2.2 (Standard Ground Fee When Under Threshold)**: Given an order subtotal of $50 (< $75 threshold), When shipping rates are evaluated, Then standard ground shipping reflects standard rate (e.g. $5.99).
- **AC-3.1 (Shipment Creation)**: Given an authorized order dispatch request with order ID and carrier selection, When a shipment is created, Then a unique tracking number (`trk_...`) and initial milestone status `LabelCreated` are recorded.
- **AC-4.1 (Milestone Event Appending)**: Given an existing shipment, When milestone update `InTransit` with location `"Distribution Center - Chicago"` is posted, Then the shipment tracking history contains the milestone and the current status reflects `InTransit`.

## 4. API & Integration Contracts
- `POST /api/v1/shipping/rates`
  - Request: `CalculateShippingRatesRequest(decimal Subtotal, decimal WeightKg, string DestinationZip, string Country)`
  - Returns: `200 OK` with `IReadOnlyList<ShippingRateOption>`
- `POST /api/v1/shipping/shipments` (StoreAdmin only)
  - Request: `CreateShipmentRequest(Guid OrderId, string Carrier, string ServiceLevel, decimal WeightKg, string DestinationAddress, string DestinationZip)`
  - Returns: `201 Created` with `ShipmentResponse`
- `GET /api/v1/shipping/shipments/{trackingNumber}`
  - Returns: `200 OK` with `ShipmentResponse` including `Milestones` list
- `POST /api/v1/shipping/shipments/{trackingNumber}/milestones` (StoreAdmin / Carrier Webhook)
  - Request: `AddMilestoneRequest(ShipmentStatus Status, string Location, string Description)`
  - Returns: `200 OK` with updated `ShipmentResponse`

## 5. Security & Isolation Constraints
- Rate calculation and tracking lookup endpoints are public.
- Shipment creation and milestone appending require `StoreAdmin` role or carrier webhook authentication.
- Dedicated `shippingdb` PostgreSQL resource.

## 6. Verifiable Automated Test Scenarios
- **Unit Tests**:
  - Rate calculation algorithms across carriers and parcel weights.
  - Free shipping threshold ($75) boundary validation ($74.99 vs $75.00 vs $100).
  - Milestone status progression order.
- **Integration Tests**:
  - EF Core database persistence of `Shipment` aggregate and owned `ShipmentMilestone` history.
- **Automated E2E Scenarios (6 Scenarios)**:
  1. **E2E-1: Carrier Rate Calculation**: Weight/destination yields USPS, UPS, FedEx, DHL options.
  2. **E2E-2: Free Shipping Threshold**: Subtotal >= $75 gives $0.00 ground rate.
  3. **E2E-3: Sub-Threshold Rate**: Subtotal < $75 charges ground shipping fee.
  4. **E2E-4: Admin Shipment Creation**: Admin creates shipment, receives tracking number and initial milestone.
  5. **E2E-5: Milestone Tracking Progression**: Appending InTransit and Delivered updates tracking history.
  6. **E2E-6: Customer Tracking Lookup & Non-Admin Creation Block**: Customer queries tracking number; non-admin blocked from creating shipment.
