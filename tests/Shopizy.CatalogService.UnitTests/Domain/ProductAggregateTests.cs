using FluentAssertions;
using Shopizy.CatalogService.Domain.Entities;
using Shopizy.CatalogService.Domain.Enums;
using Shopizy.CatalogService.Domain.Events;
using Shopizy.CatalogService.Domain.ValueObjects;
using Shopizy.SharedKernel.Results;

namespace Shopizy.CatalogService.UnitTests.Domain;

public class ProductAggregateTests
{
    private static Product CreateValidProduct()
    {
        var price = Money.Create(299.99m, "USD").Value;
        return Product.Create(
            "Noise-Cancelling Headphones",
            "noise-cancelling-headphones",
            "High quality audio with ANC",
            Guid.NewGuid(),
            Guid.NewGuid(),
            price).Value;
    }

    [Fact]
    public void Create_WithValidData_ReturnsSuccessAndStagesEvent()
    {
        // Act
        var product = CreateValidProduct();

        // Assert
        product.Name.Should().Be("Noise-Cancelling Headphones");
        product.Slug.Should().Be("noise-cancelling-headphones");
        product.Status.Should().Be(ProductStatus.Draft);
        product.Version.Should().Be(1);
        product.DomainEvents.Should().ContainSingle(e => e is ProductCreatedDomainEvent);
    }

    [Fact]
    public void Update_WithCorrectVersion_IncrementsVersionAndRaisesEvent()
    {
        // Arrange
        var product = CreateValidProduct();
        var newPrice = Money.Create(279.99m, "USD").Value;

        // Act
        var result = product.Update(
            "Updated Name",
            "updated-name",
            "Updated Description",
            product.CategoryId,
            product.BrandId,
            newPrice,
            expectedVersion: 1);

        // Assert
        result.IsSuccess.Should().BeTrue();
        product.Version.Should().Be(2);
        product.Name.Should().Be("Updated Name");
        product.Slug.Should().Be("updated-name");
        product.DomainEvents.Should().Contain(e => e is ProductUpdatedDomainEvent);
    }

    [Fact]
    public void Update_WithStaleVersion_ReturnsConflictError()
    {
        // Arrange
        var product = CreateValidProduct();
        var newPrice = Money.Create(279.99m, "USD").Value;

        // Act
        var result = product.Update(
            "Updated Name",
            "updated-name",
            "Updated Description",
            product.CategoryId,
            product.BrandId,
            newPrice,
            expectedVersion: 99);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Conflict);
        result.Error.Code.Should().Be("Product.ConcurrencyConflict");
    }

    [Fact]
    public void AddVariant_WithUniqueSku_AddsVariantToCollection()
    {
        // Arrange
        var product = CreateValidProduct();
        var price = Money.Create(299.99m, "USD").Value;

        // Act
        var result = product.AddVariant("SKU-PRO-01", "12345678", price, 15, new Dictionary<string, string> { { "Color", "Black" } });

        // Assert
        result.IsSuccess.Should().BeTrue();
        product.Variants.Should().ContainSingle(v => v.Sku == "SKU-PRO-01");
    }

    [Fact]
    public void AddVariant_WithDuplicateSkuWithinProduct_ReturnsConflictError()
    {
        // Arrange
        var product = CreateValidProduct();
        var price = Money.Create(299.99m, "USD").Value;
        product.AddVariant("SKU-PRO-01", null, price, 10);

        // Act
        var result = product.AddVariant("SKU-PRO-01", null, price, 5);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Product.DuplicateSku");
    }

    [Fact]
    public void UpdateVariantStock_WithValidQuantity_UpdatesStockAndRaisesEvent()
    {
        // Arrange
        var product = CreateValidProduct();
        var price = Money.Create(299.99m, "USD").Value;
        var variant = product.AddVariant("SKU-PRO-01", null, price, 10).Value;

        // Act
        var result = product.UpdateVariantStock(variant.Id, 25);

        // Assert
        result.IsSuccess.Should().BeTrue();
        variant.StockQuantity.Should().Be(25);
        product.DomainEvents.Should().Contain(e => e is ProductStockUpdatedDomainEvent);
    }

    [Fact]
    public void UpdateVariantStock_WithNonExistentVariant_ReturnsNotFound()
    {
        // Arrange
        var product = CreateValidProduct();

        // Act
        var result = product.UpdateVariantStock(Guid.NewGuid(), 5);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("ProductVariant.NotFound");
    }

    [Fact]
    public void AddImage_WithIsMain_SetsOtherImagesMainToFalse()
    {
        // Arrange
        var product = CreateValidProduct();

        // Act
        product.AddImage("https://img1.test", "Image 1", 1, isMain: true);
        product.AddImage("https://img2.test", "Image 2", 2, isMain: true);

        // Assert
        product.Images.Should().HaveCount(2);
        product.Images.First(i => i.Url == "https://img1.test").IsMain.Should().BeFalse();
        product.Images.First(i => i.Url == "https://img2.test").IsMain.Should().BeTrue();
    }

    [Fact]
    public void StatusTransitions_PublishAndArchive_WorkCorrectly()
    {
        // Arrange
        var product = CreateValidProduct();
        product.Status.Should().Be(ProductStatus.Draft);

        // Act & Assert Publish
        var pubResult = product.Publish();
        pubResult.IsSuccess.Should().BeTrue();
        product.Status.Should().Be(ProductStatus.Published);

        // Act & Assert Archive
        var arcResult = product.Archive();
        arcResult.IsSuccess.Should().BeTrue();
        product.Status.Should().Be(ProductStatus.Archived);

        // Further modifications should be rejected
        var price = Money.Create(100m, "USD").Value;
        var updateResult = product.Update("Name", "slug", "desc", product.CategoryId, product.BrandId, price, product.Version);
        updateResult.IsFailure.Should().BeTrue();
        updateResult.Error.Code.Should().Be("Product.Archived");
    }
}
