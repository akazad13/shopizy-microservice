# 0011. Read Scalability via CQRS and Search Engine Offloading

- **Status:** Accepted
- **Date:** 2026-09-05

## Context

E-commerce traffic is heavily read-skewed (typically 100:1 to 500:1 read-to-write ratios). Customers continuously browse catalogs, search by keywords with typos, filter by multiple facet attributes (price ranges, brands, sizes, ratings, availability), and view product details. Executing complex text search queries and dynamic multi-facet aggregation queries directly against PostgreSQL primary relational tables degrades database performance and causes table contention for write-heavy order processing.

## Decision

- Implement **Command Query Responsibility Segregation (CQRS)** paired with **Search Engine Offloading**:
  - **Write Path**: Handled exclusively by `CatalogService` using EF Core 10 writes to PostgreSQL with optimistic concurrency tokens.
  - When product or catalog updates occur, `CatalogService` publishes `CatalogItemUpdatedEvent` to RabbitMQ via the Transactional Outbox.
  - **Read & Search Path**: Handled by `Shopizy.SearchService`, which consumes catalog events and indexes documents into a dedicated search engine (**Elasticsearch / Meilisearch**):
    - Supports n-gram fuzzy matching, retail synonym dictionaries, and typo tolerance (e.g. `"iphne"` maps to `"iPhone"`).
    - Computes dynamic multi-attribute facet buckets in sub-second response times (<500ms).
  - All public storefront search and faceted category browse queries route directly to `SearchService`, completely bypassing the primary PostgreSQL databases.
  - Multi-tier caching: In-memory `IMemoryCache` (L1) for top 100 high-frequency catalog items, and Redis (L2) for cached category trees and facet schemas.

## Consequences

- Primary PostgreSQL databases experience virtually zero load from catalog search and browsing traffic, reserving database resources for orders, inventory, and payments.
- Search queries return in sub-500ms even across catalogs with tens of thousands of variant products.
- Introduces eventual consistency between catalog edits and search index updates (typically <1 second lag via RabbitMQ event projection).
