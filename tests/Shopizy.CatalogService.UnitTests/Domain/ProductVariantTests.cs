using FluentAssertions;
using Shopizy.CatalogService.Domain.Entities;
using Shopizy.CatalogService.Domain.ValueObjects;
using Shopizy.SharedKernel.Results;

namespace Shopizy.CatalogService.UnitTests.Domain;

public class ProductVariantTests
{
    [Fact]
    public void Create_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var price = Money.Create(149.99m, "USD").Value;
        var attributes = new Dictionary<string, string> { { "Size", "M" }, { "Color", "Blue" } };

        // Act
        var result = ProductVariant.Create(productId, "TSHIRT-BLU-M", "1234567890", price, 50, attributes);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Sku.Should().Be("TSHIRT-BLU-M");
        result.Value.Barcode.Should().Be("1234567890");
        result.Value.StockQuantity.Should().Be(50);
        result.Value.IsInStock.Should().BeTrue();
        result.Value.Attributes.Should().ContainKey("Size").WhoseValue.Should().Be("M");
    }

    [Fact]
    public void Create_WithNegativeStockQuantity_ReturnsValidationError()
    {
        // Arrange
        var price = Money.Create(10m, "USD").Value;

        // Act
        var result = ProductVariant.Create(Guid.NewGuid(), "SKU-001", null, price, -5);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("ProductVariant.NegativeStock");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptySku_ReturnsValidationError(string emptySku)
    {
        // Arrange
        var price = Money.Create(10m, "USD").Value;

        // Act
        var result = ProductVariant.Create(Guid.NewGuid(), emptySku, null, price, 10);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("ProductVariant.EmptySku");
    }

    [Fact]
    public void AdjustStock_WithNegativeNumber_ReturnsValidationError()
    {
        // Arrange
        var price = Money.Create(10m, "USD").Value;
        var variant = ProductVariant.Create(Guid.NewGuid(), "SKU-001", null, price, 10).Value;

        // Act
        var result = variant.AdjustStock(-1);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("ProductVariant.NegativeStock");
    }

    [Fact]
    public void AdjustStock_ToZero_SetsIsInStockToFalse()
    {
        // Arrange
        var price = Money.Create(10m, "USD").Value;
        var variant = ProductVariant.Create(Guid.NewGuid(), "SKU-001", null, price, 10).Value;

        // Act
        var result = variant.AdjustStock(0);

        // Assert
        result.IsSuccess.Should().BeTrue();
        variant.StockQuantity.Should().Be(0);
        variant.IsInStock.Should().BeFalse();
    }
}
