using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Shopizy.CatalogService.Domain.Entities;
using Shopizy.CatalogService.Infrastructure.Persistence;
using Shopizy.CatalogService.Infrastructure.Persistence.Repositories;

namespace Shopizy.CatalogService.IntegrationTests.Persistence;

public class BrandRepositoryTests
{
    private static CatalogDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new CatalogDbContext(options);
    }

    [Fact]
    public async Task AddAndGetBySlugAsync_PersistsBrandCorrectly()
    {
        // Arrange
        using var context = CreateDbContext();
        var repo = new BrandRepository(context);
        var brand = Brand.Create("HyperX", "hyperx", "Gaming peripherals", "https://hyperx.test", "https://hyperx.test/logo.png").Value;

        // Act
        await repo.AddAsync(brand);
        var retrieved = await repo.GetBySlugAsync("hyperx");

        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.Name.Should().Be("HyperX");
        retrieved.WebsiteUrl.Should().Be("https://hyperx.test");
    }

    [Fact]
    public async Task GetAllAsync_WhenActiveOnly_FiltersInactiveBrands()
    {
        // Arrange
        using var context = CreateDbContext();
        var repo = new BrandRepository(context);

        var activeBrand = Brand.Create("Active Brand", "active-brand").Value;
        var inactiveBrand = Brand.Create("Inactive Brand", "inactive-brand").Value;
        inactiveBrand.Deactivate();

        await repo.AddAsync(activeBrand);
        await repo.AddAsync(inactiveBrand);

        // Act
        var activeOnly = await repo.GetAllAsync(activeOnly: true);
        var all = await repo.GetAllAsync(activeOnly: false);

        // Assert
        activeOnly.Should().ContainSingle(b => b.Id == activeBrand.Id);
        all.Should().HaveCount(2);
    }
}
