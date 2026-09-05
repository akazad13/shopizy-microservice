# Implementation Tasks: Search & Discovery Engine (`search-service`)

- [x] 1. Architecture & Domain Model: Define `SearchProductDocument`, `SearchQuery`, and `SearchFacets`.
- [x] 2. Fuzzy Matching & Synonym Expansion: Implement typo-tolerant string distance (Damerau-Levenshtein) and retail synonym expansion.
- [x] 3. Index Store & Faceting Aggregator: Implement `ISearchIndexStore` with category, brand, price tier, and stock facet calculations.
- [x] 4. Application Service & Handlers: Implement `SearchApplicationService` with query coordination, sorting, and pagination.
- [x] 5. Minimal APIs & RBAC: Implement `/api/v1/search` and admin-protected `/api/v1/search/index` endpoints.
- [x] 6. Aspire Wiring: Register `search-service` in `Shopizy.AppHost`.
- [x] 7. Automated Unit Tests: Write unit tests covering fuzzy matching, synonyms, and facet aggregations.
- [x] 8. Automated Integration Tests: Write integration tests for indexing lifecycle and facet queries.
- [x] 9. Automated E2E Tests: Write 6 E2E tests validating typo tolerance, synonyms, did-you-mean, multi-attribute facets, filtering, and admin indexing.
