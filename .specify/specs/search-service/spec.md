# Specification: Search & Discovery Engine (`search-service`)

## 1. Executive Summary & Objectives
Module 7 (`search-service`) delivers intelligent, ultra-fast full-text product search, typo-tolerant fuzzy matching, retail synonym expansions, "Did You Mean?" suggestion generation, and multi-attribute faceted navigation (<500ms response time). It consumes catalog and inventory change events or index ingestion commands to maintain high-performance search indices without imposing read overhead on the core transactional catalog database.

## 2. Personas & User Stories
- **US-1 (Shopper - Fuzzy & Typo Search)**: As a shopper, I want to search for products using terms with spelling errors (e.g. `"iphne"` or `"runing shoe"`), so that I immediately find the intended items.
- **US-2 (Shopper - Retail Synonyms)**: As a shopper, I want searching for colloquial terminology (e.g. `"sneakers"` or `"shades"`) to surface items categorized under canonical names (e.g. `"athletic shoes"` or `"sunglasses"`), so that I don't miss relevant products.
- **US-3 (Shopper - "Did You Mean?" Suggestions)**: As a shopper, when my query yields few or zero results, I want automated "Did You Mean?" suggestions, so that I can easily discover related items.
- **US-4 (Shopper - Multi-Attribute Faceted Filtering)**: As a shopper, I want to filter search results dynamically by category, brand, price range, customer rating threshold, and in-stock status with real-time facet count badges, so that I can quickly narrow down catalog choices.
- **US-5 (Store Administrator - Index Management)**: As a store administrator, I want catalog items to be indexed or updated in the search store efficiently, so that search discovery is always up to date.

## 3. Detailed Acceptance Criteria (Given-When-Then)
- **AC-1.1 (Fuzzy Matching)**: Given indexed products with title `"iPhone 15 Pro"`, When a customer searches for `"iphne"`, Then the response returns `"iPhone 15 Pro"` with a relevance score and response latency under 500ms.
- **AC-1.2 (Multi-Word Matching)**: Given indexed products, When a customer searches for `"running shoes red"`, Then products matching all terms across title, description, or tags are returned first.
- **AC-2.1 (Synonym Expansion)**: Given configured synonym mappings (`"sneakers"` -> `["athletic shoes", "trainers", "sneakers"]`), When a customer searches for `"sneakers"`, Then products with categories or tags matching `"athletic shoes"` are included in the results.
- **AC-3.1 ("Did You Mean?" Suggestions)**: Given no exact matches for a query like `"sumsung"`, When search is executed, Then `"Did You Mean?"` provides `"samsung"` as an alternative suggestion.
- **AC-4.1 (Faceted Navigation & Counts)**: Given a product search query or empty query across the catalog, When faceted search is requested, Then the response contains matching items plus facet distribution buckets for:
  - Categories (with document counts)
  - Brands (with document counts)
  - Price ranges (`Under $25`, `$25-$50`, `$50-$100`, `$100+`)
  - In-stock availability counts
- **AC-4.2 (Filter Application)**: Given faceted search query with `minPrice=50&maxPrice=100&inStockOnly=true`, When executed, Then only items matching both the price window and `InStock == true` are returned.
- **AC-5.1 (Index Upsert & Deletion)**: Given a search document ingest command, When posted to `/api/v1/search/index`, Then the item is indexed and immediately retrievable via search endpoints.

## 4. API & Integration Contracts
- `GET /api/v1/search?q={query}&category={cat}&brand={brand}&minPrice={min}&maxPrice={max}&inStockOnly={bool}&minRating={rating}&page={p}&pageSize={s}`
  - Returns `200 OK` with `SearchResponse` containing:
    - `Items`: list of `SearchProductDocument`
    - `TotalCount`: integer
    - `Facets`: `SearchFacets` (CategoryCounts, BrandCounts, PriceRangeCounts, InStockCount)
    - `DidYouMean`: string or null
- `POST /api/v1/search/index` (Admin only)
  - Ingests `SearchProductDocument`
  - Returns `200 OK`
- `DELETE /api/v1/search/index/{productId}` (Admin only)
  - Removes product from search index
  - Returns `204 No Content`

## 5. Security & Isolation Constraints
- Search querying endpoints are public (accessible by anonymous shoppers and authenticated customers).
- Index mutation endpoints require `StoreAdmin` role.
- Zero cross-tenant leakage of confidential admin or margin attributes.

## 6. Verifiable Automated Test Scenarios
- **Unit Tests**:
  - Levenshtein / Damerau-Levenshtein typo-tolerance distance calculations.
  - Synonym dictionary expansion and tokenization.
  - Facet bucket aggregation calculations.
- **Integration Tests**:
  - Ingestion and persistence of search documents in Search Repository.
  - Query filtering by price range, brand, category, and in-stock flag.
- **Automated E2E Scenarios (6 Scenarios)**:
  1. **E2E-1: Typo Tolerance**: Search with `"iphne"` returns `"iPhone"`.
  2. **E2E-2: Synonym Matching**: Search with `"sneakers"` returns items tagged `"athletic shoes"`.
  3. **E2E-3: "Did You Mean?" Suggestions**: Erroneous query yields accurate suggestion.
  4. **E2E-4: Multi-Attribute Facets**: Facet buckets (categories, brands, price tiers) return accurate item counts.
  5. **E2E-5: Faceted Filtering**: Filtering by `brand`, `minPrice`, `maxPrice`, and `inStockOnly` correctly restricts results.
  6. **E2E-6: Admin Index Ingestion**: Admin indexes a new product, verifies immediate searchability, and deletes it.
