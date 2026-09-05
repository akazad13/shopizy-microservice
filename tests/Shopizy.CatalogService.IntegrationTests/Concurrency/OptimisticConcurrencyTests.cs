using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Shopizy.CatalogService.Application.Contracts;
using Shopizy.CatalogService.Domain.Entities;
using Shopizy.CatalogService.Domain.ValueObjects;
using Shopizy.CatalogService.Infrastructure.Persistence;
using Shopizy.CatalogService.Infrastructure.Persistence.Repositories;
using Shopizy.SharedKernel.Results;
using CatalogAppService = Shopizy.CatalogService.Application.Services.CatalogService;

namespace Shopizy.CatalogService.IntegrationTests.Concurrency;

public class OptimisticConcurrencyTests
{
    [Fact]
    public async Task ProductUpdate_WhenConcurrentModificationOccurs_FailsWithConcurrencyConflict()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var options = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        using var context = new CatalogDbContext(options);
        var categoryRepo = new CategoryRepository(context);
        var brandRepo = new BrandRepository(context);
        var productRepo = new ProductRepository(context);
        var service = new CatalogAppService(categoryRepo, brandRepo, productRepo);

        var category = Category.Create("Peripherals", "peripherals").Value;
        var brand = Brand.Create("LogiTech", "logitech").Value;
        await categoryRepo.AddAsync(category);
        await brandRepo.AddAsync(brand);

        var price = Money.Create(150m, "USD").Value;
        var createResult = await service.CreateProductAsync(new CreateProductRequest(
            "Wireless Mouse",
            "wireless-mouse",
            "Ergonomic mouse",
            category.Id,
            brand.Id,
            150m));

        createResult.IsSuccess.Should().BeTrue();
        var productId = createResult.Value.Id;
        createResult.Value.Version.Should().Be(1);

        // Client 1 updates successfully -> Version becomes 2
        var update1Result = await service.UpdateProductAsync(productId, new UpdateProductRequest(
            "Wireless Mouse V2",
            "wireless-mouse-v2",
            "Updated ergonomics",
            category.Id,
            brand.Id,
            160m,
            "USD",
            ExpectedVersion: 1));

        update1Result.IsSuccess.Should().BeTrue();
        update1Result.Value.Version.Should().Be(2);

        // Client 2 attempts update with stale ExpectedVersion: 1
        var update2Result = await service.UpdateProductAsync(productId, new UpdateProductRequest(
            "Wireless Mouse Alt",
            "wireless-mouse-alt",
            "Conflicting changes",
            category.Id,
            brand.Id,
            170m,
            "USD",
            ExpectedVersion: 1)); // Stale version!

        // Assert
        update2Result.IsFailure.Should().BeTrue();
        update2Result.Error.Type.Should().Be(ErrorType.Conflict);
        update2Result.Error.Code.Should().Be("Product.ConcurrencyConflict");
    }
}
