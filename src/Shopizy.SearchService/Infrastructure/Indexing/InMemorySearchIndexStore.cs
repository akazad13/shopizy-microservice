using System.Collections.Concurrent;
using Shopizy.SearchService.Application.Interfaces;
using Shopizy.SearchService.Domain;
using Shopizy.SearchService.Domain.Entities;
using Shopizy.SearchService.Domain.ValueObjects;

namespace Shopizy.SearchService.Infrastructure.Indexing;

public sealed class InMemorySearchIndexStore : ISearchIndexStore
{
    private readonly ConcurrentDictionary<Guid, SearchProductDocument> _documents = new();

    public Task IndexAsync(SearchProductDocument document, CancellationToken ct = default)
    {
        _documents[document.Id] = document;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        _documents.TryRemove(id, out _);
        return Task.CompletedTask;
    }

    public Task<SearchProductDocument?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        _documents.TryGetValue(id, out var doc);
        return Task.FromResult(doc);
    }

    public Task<IReadOnlyList<string>> GetAllTermsAsync(CancellationToken ct = default)
    {
        var terms = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var doc in _documents.Values)
        {
            AddWords(terms, doc.Title);
            AddWords(terms, doc.Description);
            AddWords(terms, doc.CategoryName);
            AddWords(terms, doc.BrandName);
            foreach (var tag in doc.Tags) AddWords(terms, tag);
        }
        return Task.FromResult<IReadOnlyList<string>>(terms.ToList());
    }

    private static void AddWords(HashSet<string> set, string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return;
        var words = text.Split(new[] { ' ', ',', '.', '-', '/', '(', ')' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        foreach (var w in words)
        {
            if (w.Length > 2) set.Add(w.ToLowerInvariant());
        }
    }

    public Task<(IReadOnlyList<SearchProductDocument> Items, int TotalCount, SearchFacets Facets)> SearchAsync(
        SearchQuery query,
        IReadOnlyList<string> expandedKeywords,
        CancellationToken ct = default)
    {
        var allDocs = _documents.Values.ToList();

        // 1. Keyword search (including fuzzy & synonyms)
        IEnumerable<SearchProductDocument> matchingDocs = allDocs;
        if (!string.IsNullOrWhiteSpace(query.QueryText))
        {
            var queryTokens = query.QueryText.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            matchingDocs = matchingDocs.Where(doc =>
            {
                var docText = $"{doc.Title} {doc.Description} {doc.CategoryName} {doc.BrandName} {string.Join(" ", doc.Tags)}".ToLowerInvariant();

                // Direct or synonym match
                foreach (var kw in expandedKeywords)
                {
                    if (docText.Contains(kw.ToLowerInvariant()))
                        return true;
                }

                // Fuzzy match against individual words in title/brand/tags
                var docWords = docText.Split(new[] { ' ', ',', '.', '-', '/' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                foreach (var qt in queryTokens)
                {
                    foreach (var dw in docWords)
                    {
                        if (FuzzyMatchEngine.IsFuzzyMatch(qt, dw))
                            return true;
                    }
                }

                return false;
            });
        }

        // 2. Calculate facets on pre-filtered set (or matching keyword set)
        var preFacetList = matchingDocs.ToList();
        var facets = CalculateFacets(preFacetList);

        // 3. Apply attribute filters
        if (!string.IsNullOrWhiteSpace(query.Category))
        {
            matchingDocs = matchingDocs.Where(d => string.Equals(d.CategoryName, query.Category, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(query.Brand))
        {
            matchingDocs = matchingDocs.Where(d => string.Equals(d.BrandName, query.Brand, StringComparison.OrdinalIgnoreCase));
        }

        if (query.MinPrice.HasValue)
        {
            matchingDocs = matchingDocs.Where(d => d.Price >= query.MinPrice.Value);
        }

        if (query.MaxPrice.HasValue)
        {
            matchingDocs = matchingDocs.Where(d => d.Price <= query.MaxPrice.Value);
        }

        if (query.MinRating.HasValue)
        {
            matchingDocs = matchingDocs.Where(d => d.AverageRating >= query.MinRating.Value);
        }

        if (query.InStockOnly.HasValue && query.InStockOnly.Value)
        {
            matchingDocs = matchingDocs.Where(d => d.InStock);
        }

        var filteredList = matchingDocs.ToList();
        int totalCount = filteredList.Count;

        // 4. Sorting & Pagination
        if (string.Equals(query.SortBy, "price_asc", StringComparison.OrdinalIgnoreCase))
            filteredList = filteredList.OrderBy(d => d.Price).ToList();
        else if (string.Equals(query.SortBy, "price_desc", StringComparison.OrdinalIgnoreCase))
            filteredList = filteredList.OrderByDescending(d => d.Price).ToList();
        else if (string.Equals(query.SortBy, "rating", StringComparison.OrdinalIgnoreCase))
            filteredList = filteredList.OrderByDescending(d => d.AverageRating).ToList();

        int skip = (Math.Max(1, query.Page) - 1) * Math.Max(1, query.PageSize);
        var pagedItems = filteredList.Skip(skip).Take(Math.Max(1, query.PageSize)).ToList();

        return Task.FromResult<(IReadOnlyList<SearchProductDocument>, int, SearchFacets)>((pagedItems, totalCount, facets));
    }

    private static SearchFacets CalculateFacets(List<SearchProductDocument> docs)
    {
        var facets = new SearchFacets();

        foreach (var doc in docs)
        {
            if (!string.IsNullOrWhiteSpace(doc.CategoryName))
            {
                facets.Categories.TryGetValue(doc.CategoryName, out int c);
                facets.Categories[doc.CategoryName] = c + 1;
            }

            if (!string.IsNullOrWhiteSpace(doc.BrandName))
            {
                facets.Brands.TryGetValue(doc.BrandName, out int b);
                facets.Brands[doc.BrandName] = b + 1;
            }

            string priceRange = doc.Price switch
            {
                < 25m => "Under $25",
                >= 25m and < 50m => "$25-$50",
                >= 50m and < 100m => "$50-$100",
                _ => "$100+"
            };

            facets.PriceRanges.TryGetValue(priceRange, out int pr);
            facets.PriceRanges[priceRange] = pr + 1;

            if (doc.InStock)
                facets.InStockCount++;
            else
                facets.OutOfStockCount++;
        }

        return facets;
    }
}
