# Specification: Product Catalog Service (`catalog-service`)

> **Document Version:** 1.0.0  
> **Status:** Approved  
> **Module Slug:** `catalog-service`  
> **Target Framework:** .NET 10 (C# 14)  
> **Dependencies:** `shared-kernel`, `identity-service`  

---

## 1. Executive Summary & Objectives

The **Product Catalog Service (`catalog-service`)** manages the entire product domain for the Shopizy headless e-commerce ecosystem. It governs hierarchical category trees, brand catalogs, product definitions, a parent-variant dimensional matrix (handling SKU, barcodes, pricing, attributes, and inventory levels), product image galleries, and cache-backed multi-faceted browsing.

### Core Business & Technical Value
- **Hierarchical Taxonomies**: Multi-level category hierarchy enabling flexible navigation (e.g. `Electronics > Audio > Headphones`).
- **Parent-Variant Matrix**: Sophisticated product matrix supporting multi-attribute variants (e.g. Size, Color, Material) with independent SKUs, pricing, barcodes, and live stock tracking.
- **Optimistic Concurrency Control**: Protection against concurrent modification anomalies on products and stock levels using entity versioning tokens.
- **Role-Based Access Control**: Public/anonymous read access for storefront browsing; strict `StoreAdmin` role authorization for all catalog modifications.
- **Idempotency & Resilience**: State-altering endpoints enforce the `Idempotency-Key` header via shared-kernel middleware to protect against duplicate requests.
- **Full Test Automation**: 100% automated test coverage across unit tests, database integration tests, and in-memory WebApplicationFactory E2E tests.

---

## 2. Personas & User Stories

- **US-1 (Category Management)**: As a StoreAdmin, I want to create and organize hierarchical categories so that products can be classified into structured taxonomies.
- **US-2 (Brand Management)**: As a StoreAdmin, I want to register and maintain brands so that products can be associated with verified manufacturers.
- **US-3 (Product & Variant Management)**: As a StoreAdmin, I want to create products with parent-level metadata, multiple dimensional variants (SKUs, pricing, stock levels), and image galleries so that complex merchandise can be cataloged.
- **US-4 (Catalog Browsing & Discovery)**: As a Shopper/Customer, I want to browse products with filtering (by category, brand, price range, stock availability), search keywords, sorting, and pagination so that I can easily discover merchandise.
- **US-5 (Product Details & Variant Stock)**: As a Shopper/Customer, I want to retrieve full product details including variants, current stock levels, and image galleries so that I make informed buying decisions.
- **US-6 (Stock Level Management)**: As a StoreAdmin, I want to adjust variant stock counts and have updates protected by optimistic concurrency so that inventory changes do not overwrite concurrent sales.
- **US-7 (Security & Access Control)**: As a Platform Operator, I want unauthorized or customer roles to be strictly prevented from mutating the catalog (403 Forbidden) while allowing public read access.
- **US-8 (Standardized Error Responses)**: As an API Consumer, I want all validation failures, missing resources, and conflicts formatted as RFC 7807 Problem Details.

---

## 3. Detailed Acceptance Criteria (Given-When-Then)

### AC-1: Hierarchical Category Management
- **AC-1.1**: Given a valid category creation payload (name, slug, optional description, optional parent category ID), When an authenticated `StoreAdmin` sends `POST /api/v1/catalog/categories`, Then the category is persisted and HTTP 201 Created is returned with category details.
- **AC-1.2**: Given a category payload with an invalid or non-existent `ParentCategoryId`, When creation is attempted, Then HTTP 400 Bad Request or 404 Not Found is returned with RFC 7807 Problem Details.
- **AC-1.3**: Given a category with children, When `GET /api/v1/catalog/categories` is requested, Then the response returns categories with their child categories structured or linkable.

### AC-2: Brand Management
- **AC-2.1**: Given a valid brand payload (name, slug, description, website URL), When an authenticated `StoreAdmin` sends `POST /api/v1/catalog/brands`, Then the brand is persisted and HTTP 201 Created is returned.
- **AC-2.2**: Given duplicate brand slug or empty brand name, When creation is attempted, Then HTTP 400 Bad Request or 409 Conflict is returned with RFC 7807 Problem Details.

### AC-3: Product & Dimensional Variant Creation
- **AC-3.1**: Given valid product metadata (name, slug, description, category ID, brand ID, base price) and one or more variants (SKU, barcode, price, stock quantity, attributes), When an authenticated `StoreAdmin` sends `POST /api/v1/catalog/products`, Then the product aggregate with variants is created and HTTP 201 Created is returned with product and variant details.
- **AC-3.2**: Given a duplicate SKU across variants, When product or variant creation is attempted, Then HTTP 409 Conflict is returned with RFC 7807 Problem Details.
- **AC-3.3**: Given negative price or negative stock quantity, When product creation is attempted, Then HTTP 400 Bad Request is returned with validation details.

### AC-4: Catalog Browsing, Search, Filtering & Pagination
- **AC-4.1**: Given published products in the database, When an anonymous or customer user queries `GET /api/v1/catalog/products` with pagination parameters (`page=1&pageSize=10`), Then HTTP 200 OK is returned with paginated items, total count, page number, and page size.
- **AC-4.2**: Given filter parameters (`categoryId`, `brandId`, `minPrice`, `maxPrice`, `inStockOnly=true`, `searchTerm`), When `GET /api/v1/catalog/products` is queried, Then only matching active products are returned.
- **AC-4.3**: Given sort options (`price_asc`, `price_desc`, `name_asc`, `newest`), When `GET /api/v1/catalog/products` is queried, Then products are returned in the requested sort order.

### AC-5: Product Details & Variant Retrieval
- **AC-5.1**: Given an existing product ID, When `GET /api/v1/catalog/products/{id}` is invoked, Then HTTP 200 OK is returned with product details, brand, category, image gallery, and associated variants with stock availability.
- **AC-5.2**: Given a non-existent product ID, When `GET /api/v1/catalog/products/{id}` is invoked, Then HTTP 404 Not Found is returned with RFC 7807 Problem Details.

### AC-6: Optimistic Concurrency & Stock Adjustment
- **AC-6.1**: Given a valid product update with matching version/concurrency token, When an authenticated `StoreAdmin` sends `PUT /api/v1/catalog/products/{id}`, Then the update succeeds and HTTP 200 OK is returned with an incremented version token.
- **AC-6.2**: Given a product update with a stale version token, When update is attempted, Then HTTP 409 Conflict is returned with RFC 7807 Problem Details indicating concurrency violation.
- **AC-6.3**: Given a variant stock adjustment payload (`newQuantity`), When an authenticated `StoreAdmin` sends `PUT /api/v1/catalog/products/{id}/variants/{variantId}/stock`, Then stock is updated and HTTP 200 OK is returned.

### AC-7: Role-Based Access Control
- **AC-7.1**: Given an unauthenticated client or a user with `Customer` role attempting to invoke `POST /api/v1/catalog/categories`, `POST /api/v1/catalog/brands`, `POST /api/v1/catalog/products`, or `PUT /api/v1/catalog/...`, Then the API returns HTTP 401 Unauthorized or HTTP 403 Forbidden.
- **AC-7.2**: Given any anonymous client accessing `GET /api/v1/catalog/categories`, `GET /api/v1/catalog/brands`, or `GET /api/v1/catalog/products`, Then HTTP 200 OK is returned.

---

## 4. API & Integration Contracts

### 4.1 Endpoint Directory

| Verb | Route | Auth Required | Authorized Roles | Description |
| :--- | :--- | :--- | :--- | :--- |
| `GET` | `/api/v1/catalog/categories` | No | Anonymous | List all active categories in tree or flat form |
| `GET` | `/api/v1/catalog/categories/{id}` | No | Anonymous | Get single category by ID |
| `POST` | `/api/v1/catalog/categories` | Yes (Bearer) | `StoreAdmin` | Create category |
| `PUT` | `/api/v1/catalog/categories/{id}` | Yes (Bearer) | `StoreAdmin` | Update category |
| `GET` | `/api/v1/catalog/brands` | No | Anonymous | List all active brands |
| `GET` | `/api/v1/catalog/brands/{id}` | No | Anonymous | Get single brand by ID |
| `POST` | `/api/v1/catalog/brands` | Yes (Bearer) | `StoreAdmin` | Create brand |
| `PUT` | `/api/v1/catalog/brands/{id}` | Yes (Bearer) | `StoreAdmin` | Update brand |
| `GET` | `/api/v1/catalog/products` | No | Anonymous | Search & filter paginated products |
| `GET` | `/api/v1/catalog/products/{id}` | No | Anonymous | Get product details by ID |
| `POST` | `/api/v1/catalog/products` | Yes (Bearer) | `StoreAdmin` | Create product with variants |
| `PUT` | `/api/v1/catalog/products/{id}` | Yes (Bearer) | `StoreAdmin` | Update product with concurrency check |
| `DELETE` | `/api/v1/catalog/products/{id}` | Yes (Bearer) | `StoreAdmin` | Archive product |
| `POST` | `/api/v1/catalog/products/{id}/variants` | Yes (Bearer) | `StoreAdmin` | Add variant to product |
| `PUT` | `/api/v1/catalog/products/{id}/variants/{variantId}/stock` | Yes (Bearer) | `StoreAdmin` | Update variant stock level |

### 4.2 Request / Response Schemas

#### Create Category Request (`POST /api/v1/catalog/categories`)
```json
{
  "name": "Headphones",
  "slug": "headphones",
  "description": "Noise-cancelling and wireless headphones",
  "parentCategoryId": "2da12345-6789-4abc-def0-123456789abc"
}
```

#### Create Brand Request (`POST /api/v1/catalog/brands`)
```json
{
  "name": "AudioTech",
  "slug": "audiotech",
  "description": "Premium audio gear manufacturer",
  "websiteUrl": "https://audiotech.example.com",
  "logoUrl": "https://audiotech.example.com/logo.png"
}
```

#### Create Product Request (`POST /api/v1/catalog/products`)
```json
{
  "name": "Wireless Noise-Cancelling Headphones Pro",
  "slug": "wireless-noise-cancelling-headphones-pro",
  "description": "High-fidelity wireless headphones with active noise cancellation.",
  "categoryId": "2da12345-6789-4abc-def0-123456789abc",
  "brandId": "3ea12345-6789-4abc-def0-123456789abc",
  "basePrice": 299.99,
  "currency": "USD",
  "images": [
    {
      "url": "https://cdn.shopizy.test/headphones-main.jpg",
      "altText": "Front view",
      "displayOrder": 1,
      "isMain": true
    }
  ],
  "variants": [
    {
      "sku": "HP-PRO-BLK",
      "barcode": "190198000001",
      "price": 299.99,
      "stockQuantity": 50,
      "attributes": {
        "Color": "Midnight Black"
      }
    },
    {
      "sku": "HP-PRO-SLV",
      "barcode": "190198000002",
      "price": 319.99,
      "stockQuantity": 25,
      "attributes": {
        "Color": "Silver"
      }
    }
  ]
}
```

#### Product Detail Response (`GET /api/v1/catalog/products/{id}`)
```json
{
  "id": "7fa12345-6789-4abc-def0-123456789abc",
  "name": "Wireless Noise-Cancelling Headphones Pro",
  "slug": "wireless-noise-cancelling-headphones-pro",
  "description": "High-fidelity wireless headphones with active noise cancellation.",
  "status": "Published",
  "basePrice": 299.99,
  "currency": "USD",
  "version": 1,
  "category": {
    "id": "2da12345-6789-4abc-def0-123456789abc",
    "name": "Headphones",
    "slug": "headphones"
  },
  "brand": {
    "id": "3ea12345-6789-4abc-def0-123456789abc",
    "name": "AudioTech",
    "slug": "audiotech"
  },
  "images": [
    {
      "id": "8aa12345-6789-4abc-def0-123456789abc",
      "url": "https://cdn.shopizy.test/headphones-main.jpg",
      "altText": "Front view",
      "displayOrder": 1,
      "isMain": true
    }
  ],
  "variants": [
    {
      "id": "9ba12345-6789-4abc-def0-123456789abc",
      "sku": "HP-PRO-BLK",
      "barcode": "190198000001",
      "price": 299.99,
      "stockQuantity": 50,
      "isInStock": true,
      "attributes": {
        "Color": "Midnight Black"
      }
    }
  ],
  "createdAtUtc": "2026-09-06T00:00:00Z",
  "updatedAtUtc": null
}
```

---

## 5. Data Models & State Transitions

### 5.1 Entities & Aggregates

- **Category**: Entity
  - `Id` (Guid, PK)
  - `Name` (string, max 100)
  - `Slug` (string, max 120, unique index)
  - `Description` (string?, max 500)
  - `ParentCategoryId` (Guid?, FK to Category)
  - `IsActive` (bool, default true)
  - `CreatedAtUtc` (DateTime)
  - `SubCategories` (`IReadOnlyCollection<Category>`)

- **Brand**: Entity
  - `Id` (Guid, PK)
  - `Name` (string, max 100)
  - `Slug` (string, max 120, unique index)
  - `Description` (string?, max 1000)
  - `WebsiteUrl` (string?, max 255)
  - `LogoUrl` (string?, max 500)
  - `IsActive` (bool, default true)
  - `CreatedAtUtc` (DateTime)

- **Product**: Aggregate Root
  - `Id` (Guid, PK)
  - `Name` (string, max 200)
  - `Slug` (string, max 220, unique index)
  - `Description` (string, max 4000)
  - `CategoryId` (Guid, FK to Category)
  - `BrandId` (Guid, FK to Brand)
  - `BasePrice` (`Money` Value Object: Amount decimal, Currency string)
  - `Status` (`ProductStatus` Enum: `Draft`, `Published`, `Archived`)
  - `Version` (int, concurrency token)
  - `CreatedAtUtc` (DateTime)
  - `UpdatedAtUtc` (DateTime?)
  - `Images` (`IReadOnlyCollection<ProductImage>`)
  - `Variants` (`IReadOnlyCollection<ProductVariant>`)

- **ProductVariant**: Entity (Child of Product)
  - `Id` (Guid, PK)
  - `ProductId` (Guid, FK to Product)
  - `Sku` (string, max 64, unique index)
  - `Barcode` (string?, max 64)
  - `Price` (`Money` Value Object)
  - `StockQuantity` (int, non-negative)
  - `Attributes` (Dictionary<string, string>, JSON serialized)
  - `IsActive` (bool, default true)
  - `CreatedAtUtc` (DateTime)
  - `UpdatedAtUtc` (DateTime?)

- **ProductImage**: Entity (Child of Product)
  - `Id` (Guid, PK)
  - `ProductId` (Guid, FK to Product)
  - `Url` (string, max 1000)
  - `AltText` (string?, max 200)
  - `DisplayOrder` (int)
  - `IsMain` (bool)

---

## 6. Automated Test Criteria (MANDATORY GATE)

### 6.1 Unit Test Criteria
- [ ] **Category Invariants**: Validates name/slug non-empty, slug format, self-referencing parent prevention.
- [ ] **Brand Invariants**: Validates non-empty name and slug, URL formats.
- [ ] **Product & Money Validation**: Rejects negative base price or invalid currency; validates required description and name.
- [ ] **Product Variant Invariants**: Rejects negative stock quantities, empty SKUs, duplicate SKUs across variants.
- [ ] **Product Status Machine**: Validates transitions: `Draft -> Published`, `Published -> Archived`, rejects modifications when `Archived`.
- [ ] **Optimistic Concurrency**: Verifies version increment and conflict detection on concurrent updates.

### 6.2 Integration Test Criteria
- [ ] **Category Hierarchy Persistence**: Verifies parent-child navigation and cascade behavior in EF Core.
- [ ] **Brand Persistence**: Verifies unique slug constraint and retrieval.
- [ ] **Product & Variant Matrix Persistence**: Verifies relational mapping, JSON attribute storage, and foreign key integrity.
- [ ] **Concurrency Conflict Handling**: Verifies EF Core `DbUpdateConcurrencyException` maps cleanly to RFC 7807 Problem Details HTTP 409 Conflict.

### 6.3 Automated End-to-End (E2E) Test Scenarios
- **Scenario E2E-01**: StoreAdmin Category Hierarchy & Brand Creation
  - StoreAdmin creates parent category `Electronics`.
  - StoreAdmin creates child category `Audio` referencing `Electronics`.
  - StoreAdmin creates brand `AudioTech`.
  - Public user retrieves category tree and verifies hierarchy.
- **Scenario E2E-02**: StoreAdmin Product with Dimensional Variants & Gallery Creation
  - StoreAdmin posts product with 2 variants (`HP-PRO-BLK`, `HP-PRO-SLV`) and image gallery.
  - Verification returns 201 Created with correct SKUs, attributes, and stock counts.
- **Scenario E2E-03**: Customer Public Browsing, Filtering, Sorting & Pagination
  - Client queries `GET /api/v1/catalog/products` without authentication.
  - Queries with `categoryId`, `minPrice`, `maxPrice`, `inStockOnly=true`.
  - Verifies paginated response structure and correct sorting (`price_asc`).
- **Scenario E2E-04**: Customer Product Detail & Variant Stock Inspection
  - Client queries `GET /api/v1/catalog/products/{id}`.
  - Verifies complete product schema, brand, category, gallery, and variant stock.
- **Scenario E2E-05**: Role-Based Access Control Security Enforcement
  - Unauthenticated client attempts `POST /api/v1/catalog/products` -> 401 Unauthorized.
  - Authenticated `Customer` role attempts `POST /api/v1/catalog/products` -> 403 Forbidden.
  - Authenticated `StoreAdmin` role attempts `POST /api/v1/catalog/products` -> 201 Created.
- **Scenario E2E-06**: Optimistic Concurrency Protection on Product Update
  - StoreAdmin fetches product (Version = 1).
  - First update succeeds (Version becomes 2).
  - Second update using stale Version = 1 receives HTTP 409 Conflict.
- **Scenario E2E-07**: Idempotent Product Creation via `Idempotency-Key` Header
  - StoreAdmin submits `POST /api/v1/catalog/products` with `Idempotency-Key: test-key-123`.
  - Submitting identical request again with same key returns cached 201 response with `X-Cache-Lookup: HIT` without duplicate creation.

---

## 7. Non-Functional & Security Requirements
- **Latency**: p95 response time < 150ms for read endpoints; < 300ms for admin mutations.
- **Security**: JWT authentication for all write endpoints; role authorization strictly enforcing `StoreAdmin`.
- **Error Standard**: 100% compliant with RFC 7807 Problem Details.
- **Build Quality**: 0 warnings with `--warnaserror`.
