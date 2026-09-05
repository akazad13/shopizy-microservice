using FluentAssertions;
using Shopizy.SearchService.Domain.Entities;
using Shopizy.SearchService.Domain.ValueObjects;
using Shopizy.SearchService.Infrastructure.Indexing;
using Xunit;

namespace Shopizy.SearchService.IntegrationTests;

public class SearchIndexStoreIntegrationTests
{
    [Fact]
    public async Task IndexAndSearch_FacetedAggregations_CalculateCorrectly()
    {
        var store = new InMemorySearchIndexStore();

        var doc1 = new SearchProductDocument
        {
            Id = Guid.NewGuid(),
            Title = "Nike Air Zoom Pegasus",
            CategoryName = "Footwear",
            BrandName = "Nike",
            Price = 120m,
            InStock = true,
            AverageRating = 4.8
        };

        var doc2 = new SearchProductDocument
        {
            Id = Guid.NewGuid(),
            Title = "Adidas Ultraboost Light",
            CategoryName = "Footwear",
            BrandName = "Adidas",
            Price = 180m,
            InStock = true,
            AverageRating = 4.5
        };

        var doc3 = new SearchProductDocument
        {
            Id = Guid.NewGuid(),
            Title = "Casual Graphic T-Shirt",
            CategoryName = "Apparel",
            BrandName = "Nike",
            Price = 22m,
            InStock = false,
            AverageRating = 4.0
        };

        await store.IndexAsync(doc1);
        await store.IndexAsync(doc2);
        await store.IndexAsync(doc3);

        var (items, totalCount, facets) = await store.SearchAsync(new SearchQuery(), Array.Empty<string>());

        totalCount.Should().Be(3);
        facets.Categories["Footwear"].Should().Be(2);
        facets.Categories["Apparel"].Should().Be(1);
        facets.Brands["Nike"].Should().Be(2);
        facets.Brands["Adidas"].Should().Be(1);
        facets.PriceRanges["Under $25"].Should().Be(1);
        facets.PriceRanges["$100+"].Should().Be(2);
        facets.InStockCount.Should().Be(2);
        facets.OutOfStockCount.Should().Be(1);
    }

    [Fact]
    public async Task SearchAsync_WithFilters_RestrictsResultsAccurately()
    {
        var store = new InMemorySearchIndexStore();

        var doc1 = new SearchProductDocument
        {
            Id = Guid.NewGuid(),
            Title = "Apple iPhone 15 Pro",
            CategoryName = "Electronics",
            BrandName = "Apple",
            Price = 999m,
            InStock = true,
            AverageRating = 4.9
        };

        var doc2 = new SearchProductDocument
        {
            Id = Guid.NewGuid(),
            Title = "Apple iPhone SE",
            CategoryName = "Electronics",
            BrandName = "Apple",
            Price = 429m,
            InStock = false,
            AverageRating = 4.2
        };

        await store.IndexAsync(doc1);
        await store.IndexAsync(doc2);

        var query = new SearchQuery
        {
            Brand = "Apple",
            InStockOnly = true
        };

        var (items, totalCount, _) = await store.SearchAsync(query, Array.Empty<string>());

        totalCount.Should().Be(1);
        items.Single().Title.Should().Be("Apple iPhone 15 Pro");
    }
}
