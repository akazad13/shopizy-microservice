# Implementation Plan: Search & Discovery Engine (`search-service`)

## 1. Architectural Approach
The `search-service` is an ASP.NET Core 10 Minimal API service built following Clean Architecture.
It maintains high-speed inverted index structures, fuzzy search engines (Levenshtein distance algorithm with prefix trees), synonym token normalizers, and dynamic facet aggregators.

## 2. Solution Structure
- `src/Shopizy.SearchService/`
  - `Domain/`:
    - `Entities/SearchProductDocument.cs`
    - `ValueObjects/FacetFilter.cs`, `ValueObjects/SearchQuery.cs`
  - `Application/`:
    - `Interfaces/ISearchIndexStore.cs`, `Interfaces/ISynonymProvider.cs`
    - `Services/SearchApplicationService.cs`
    - `Contracts/SearchDtos.cs`
  - `Infrastructure/`:
    - `Indexing/InMemorySearchIndexStore.cs` (or persistent index store supporting fuzzy matching, synonym expansion, and faceting)
    - `Synonyms/RetailSynonymProvider.cs`
  - `Endpoints/SearchEndpoints.cs`
  - `Program.cs`
- `tests/Shopizy.SearchService.UnitTests/`
  - Fuzzy matching unit tests, synonym tokenizer unit tests, facet aggregator unit tests.
- `tests/Shopizy.SearchService.IntegrationTests/`
  - Indexing and query execution integration tests.
- `tests/Shopizy.SearchService.E2ETests/`
  - 6 end-to-end API tests covering all acceptance criteria.

## 3. Aspire Orchestration
Add `search-service` to `Shopizy.AppHost/Program.cs`.
