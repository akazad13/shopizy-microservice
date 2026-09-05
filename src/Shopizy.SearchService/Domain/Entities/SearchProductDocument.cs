namespace Shopizy.SearchService.Domain.Entities;

public sealed class SearchProductDocument
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string BrandName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Currency { get; set; } = "USD";
    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public bool InStock { get; set; }
    public List<string> Tags { get; set; } = new();
    public DateTimeOffset IndexedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}
