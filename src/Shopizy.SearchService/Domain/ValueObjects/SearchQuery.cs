namespace Shopizy.SearchService.Domain.ValueObjects;

public sealed class SearchQuery
{
    public string? QueryText { get; set; }
    public string? Category { get; set; }
    public string? Brand { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public double? MinRating { get; set; }
    public bool? InStockOnly { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SortBy { get; set; }
}

public sealed class SearchFacets
{
    public Dictionary<string, int> Categories { get; set; } = new();
    public Dictionary<string, int> Brands { get; set; } = new();
    public Dictionary<string, int> PriceRanges { get; set; } = new();
    public int InStockCount { get; set; }
    public int OutOfStockCount { get; set; }
}
