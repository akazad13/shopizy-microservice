using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Shopizy.CatalogService.Application.Contracts;
using Shopizy.CatalogService.Domain.Entities;
using Shopizy.CatalogService.Domain.ValueObjects;
using Shopizy.CatalogService.Infrastructure.Persistence;
using Shopizy.CatalogService.Infrastructure.Persistence.Repositories;

namespace Shopizy.CatalogService.IntegrationTests.Persistence;

public class ProductRepositoryTests
{
    private static CatalogDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new CatalogDbContext(options);
    }

    [Fact]
    public async Task AddAndGetByIdAsync_PersistsProductWithVariantsAndImages()
    {
        // Arrange
        using var context = CreateDbContext();
        var repo = new ProductRepository(context);

        var price = Money.Create(199.99m, "USD").Value;
        var product = Product.Create("Smart Watch", "smart-watch", "Fitness tracker", Guid.NewGuid(), Guid.NewGuid(), price).Value;

        product.AddVariant("SW-BLK", "111222333", price, 20, new Dictionary<string, string> { { "Color", "Black" } });
        product.AddImage("https://img.test/sw.png", "Front view", 1, isMain: true);

        // Act
        await repo.AddAsync(product);
        var retrieved = await repo.GetByIdAsync(product.Id);

        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.Name.Should().Be("Smart Watch");
        retrieved.Variants.Should().ContainSingle(v => v.Sku == "SW-BLK");
        retrieved.Images.Should().ContainSingle(i => i.IsMain);
    }

    [Fact]
    public async Task SearchAsync_WithFiltersAndPagination_ReturnsMatchingProducts()
    {
        // Arrange
        using var context = CreateDbContext();
        var repo = new ProductRepository(context);

        var cat1 = Category.Create("Audio", "audio").Value;
        var cat2 = Category.Create("Video", "video").Value;
        context.Categories.AddRange(cat1, cat2);

        var brand = Brand.Create("TechCo", "techco").Value;
        context.Brands.Add(brand);
        await context.SaveChangesAsync();

        var p1 = Product.Create("Headphones", "headphones", "Wireless ANC", cat1.Id, brand.Id, Money.Create(100m).Value).Value;
        p1.AddVariant("HP-01", null, Money.Create(100m).Value, 5);
        p1.Publish();

        var p2 = Product.Create("Earbuds", "earbuds", "In-ear buds", cat1.Id, brand.Id, Money.Create(50m).Value).Value;
        p2.AddVariant("EB-01", null, Money.Create(50m).Value, 0); // Out of stock
        p2.Publish();

        var p3 = Product.Create("Projector", "projector", "4K home cinema", cat2.Id, brand.Id, Money.Create(500m).Value).Value;
        p3.AddVariant("PR-01", null, Money.Create(500m).Value, 2);
        p3.Publish();

        await repo.AddAsync(p1);
        await repo.AddAsync(p2);
        await repo.AddAsync(p3);

        // Act 1: Filter by category Audio & InStockOnly
        var audioInStock = await repo.SearchAsync(new ProductQueryParameters(CategoryId: cat1.Id, InStockOnly: true));

        // Act 2: Filter by price range
        var priceFilter = await repo.SearchAsync(new ProductQueryParameters(MinPrice: 80m, MaxPrice: 200m));

        // Assert
        audioInStock.Items.Should().ContainSingle(p => p.Name == "Headphones");
        priceFilter.Items.Should().ContainSingle(p => p.Name == "Headphones");
    }

    [Fact]
    public async Task SkuExistsAsync_DetectsExistingSku()
    {
        // Arrange
        using var context = CreateDbContext();
        var repo = new ProductRepository(context);

        var product = Product.Create("Keyboard", "keyboard", "Mechanical keyboard", Guid.NewGuid(), Guid.NewGuid(), Money.Create(89.99m).Value).Value;
        product.AddVariant("KEY-MECH-RED", null, Money.Create(89.99m).Value, 10);
        await repo.AddAsync(product);

        // Act
        var exists = await repo.SkuExistsAsync("KEY-MECH-RED");
        var notExists = await repo.SkuExistsAsync("UNKNOWN-SKU");

        // Assert
        exists.Should().BeTrue();
        notExists.Should().BeFalse();
    }
}
