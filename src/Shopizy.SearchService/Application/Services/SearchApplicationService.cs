using Shopizy.SearchService.Application.Contracts;
using Shopizy.SearchService.Application.Interfaces;
using Shopizy.SearchService.Domain;
using Shopizy.SearchService.Domain.Entities;
using Shopizy.SearchService.Domain.ValueObjects;

namespace Shopizy.SearchService.Application.Services;

public sealed class SearchApplicationService
{
    private readonly ISearchIndexStore _indexStore;
    private readonly ISynonymProvider _synonymProvider;

    public SearchApplicationService(ISearchIndexStore indexStore, ISynonymProvider synonymProvider)
    {
        _indexStore = indexStore;
        _synonymProvider = synonymProvider;
    }

    public async Task<SearchResponse> SearchAsync(SearchQuery query, CancellationToken ct = default)
    {
        var expandedKeywords = !string.IsNullOrWhiteSpace(query.QueryText)
            ? _synonymProvider.ExpandSynonyms(query.QueryText)
            : Array.Empty<string>();

        var (items, totalCount, facets) = await _indexStore.SearchAsync(query, expandedKeywords, ct);

        string? didYouMean = null;
        if (totalCount == 0 && !string.IsNullOrWhiteSpace(query.QueryText))
        {
            didYouMean = await FindDidYouMeanAsync(query.QueryText, ct);
        }

        return new SearchResponse(items, totalCount, query.Page, query.PageSize, facets, didYouMean);
    }

    public async Task IndexProductAsync(IndexProductRequest request, CancellationToken ct = default)
    {
        var doc = new SearchProductDocument
        {
            Id = request.Id,
            Title = request.Title,
            Description = request.Description,
            CategoryName = request.CategoryName,
            BrandName = request.BrandName,
            Price = request.Price,
            Currency = request.Currency ?? "USD",
            AverageRating = request.AverageRating,
            ReviewCount = request.ReviewCount,
            InStock = request.InStock,
            Tags = request.Tags ?? new(),
            IndexedAtUtc = DateTimeOffset.UtcNow
        };

        await _indexStore.IndexAsync(doc, ct);
    }

    public async Task DeleteProductAsync(Guid id, CancellationToken ct = default)
    {
        await _indexStore.DeleteAsync(id, ct);
    }

    private async Task<string?> FindDidYouMeanAsync(string query, CancellationToken ct)
    {
        var allTerms = await _indexStore.GetAllTermsAsync(ct);
        var queryTokens = query.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var suggestedTokens = new List<string>();
        bool changed = false;

        foreach (var qt in queryTokens)
        {
            string bestMatch = qt;
            int minDistance = int.MaxValue;

            foreach (var term in allTerms)
            {
                int dist = FuzzyMatchEngine.DamerauLevenshteinDistance(qt, term);
                if (dist < minDistance && dist <= 2)
                {
                    minDistance = dist;
                    bestMatch = term;
                }
            }

            if (bestMatch != qt) changed = true;
            suggestedTokens.Add(bestMatch);
        }

        return changed ? string.Join(" ", suggestedTokens) : null;
    }
}
