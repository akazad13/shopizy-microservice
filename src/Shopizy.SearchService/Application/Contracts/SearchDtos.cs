using Shopizy.SearchService.Domain.Entities;
using Shopizy.SearchService.Domain.ValueObjects;

namespace Shopizy.SearchService.Application.Contracts;

public sealed record SearchResponse(
    IReadOnlyList<SearchProductDocument> Items,
    int TotalCount,
    int Page,
    int PageSize,
    SearchFacets Facets,
    string? DidYouMean = null);

public sealed record IndexProductRequest(
    Guid Id,
    string Title,
    string Description,
    string CategoryName,
    string BrandName,
    decimal Price,
    string? Currency,
    double AverageRating,
    int ReviewCount,
    bool InStock,
    List<string>? Tags);
