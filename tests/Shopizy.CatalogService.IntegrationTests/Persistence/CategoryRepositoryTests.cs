using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Shopizy.CatalogService.Domain.Entities;
using Shopizy.CatalogService.Infrastructure.Persistence;
using Shopizy.CatalogService.Infrastructure.Persistence.Repositories;

namespace Shopizy.CatalogService.IntegrationTests.Persistence;

public class CategoryRepositoryTests
{
    private static CatalogDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new CatalogDbContext(options);
    }

    [Fact]
    public async Task AddAndGetByIdAsync_PersistsCategoryCorrectly()
    {
        // Arrange
        using var context = CreateDbContext();
        var repo = new CategoryRepository(context);
        var category = Category.Create("Gaming Consoles", "gaming-consoles", "Next-gen gaming").Value;

        // Act
        await repo.AddAsync(category);
        var retrieved = await repo.GetByIdAsync(category.Id);

        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.Name.Should().Be("Gaming Consoles");
        retrieved.Slug.Should().Be("gaming-consoles");
    }

    [Fact]
    public async Task SubCategories_AreLoadedCorrectly()
    {
        // Arrange
        using var context = CreateDbContext();
        var repo = new CategoryRepository(context);

        var parent = Category.Create("Computers", "computers").Value;
        await repo.AddAsync(parent);

        var child = Category.Create("Laptops", "laptops", null, parent.Id).Value;
        await repo.AddAsync(child);

        // Act
        var retrievedParent = await repo.GetByIdAsync(parent.Id);

        // Assert
        retrievedParent.Should().NotBeNull();
        retrievedParent!.SubCategories.Should().ContainSingle(c => c.Id == child.Id);
    }

    [Fact]
    public async Task SlugExistsAsync_ReturnsTrueWhenMatches()
    {
        // Arrange
        using var context = CreateDbContext();
        var repo = new CategoryRepository(context);
        var category = Category.Create("Tablets", "tablets").Value;
        await repo.AddAsync(category);

        // Act
        var exists = await repo.SlugExistsAsync("tablets");
        var notExists = await repo.SlugExistsAsync("smartphones");

        // Assert
        exists.Should().BeTrue();
        notExists.Should().BeFalse();
    }
}
