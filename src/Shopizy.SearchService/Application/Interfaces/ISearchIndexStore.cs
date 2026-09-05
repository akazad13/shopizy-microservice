using Shopizy.SearchService.Domain.Entities;
using Shopizy.SearchService.Domain.ValueObjects;

namespace Shopizy.SearchService.Application.Interfaces;

public interface ISearchIndexStore
{
    Task IndexAsync(SearchProductDocument document, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<SearchProductDocument?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<(IReadOnlyList<SearchProductDocument> Items, int TotalCount, SearchFacets Facets)> SearchAsync(SearchQuery query, IReadOnlyList<string> expandedKeywords, CancellationToken ct = default);
    Task<IReadOnlyList<string>> GetAllTermsAsync(CancellationToken ct = default);
}

public interface ISynonymProvider
{
    IReadOnlyList<string> ExpandSynonyms(string query);
}
