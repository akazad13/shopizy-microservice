using FluentAssertions;
using Shopizy.CatalogService.Domain.ValueObjects;
using Shopizy.SharedKernel.Results;

namespace Shopizy.CatalogService.UnitTests.Domain;

public class MoneyValueObjectTests
{
    [Fact]
    public void Create_WithValidAmountAndCurrency_ReturnsSuccess()
    {
        // Act
        var result = Money.Create(199.99m, "USD");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Amount.Should().Be(199.99m);
        result.Value.Currency.Should().Be("USD");
    }

    [Fact]
    public void Create_RoundsToTwoDecimalPlaces()
    {
        // Act
        var result = Money.Create(19.999m, "USD");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Amount.Should().Be(20.00m);
    }

    [Fact]
    public void Create_WithNegativeAmount_ReturnsValidationError()
    {
        // Act
        var result = Money.Create(-10.50m, "USD");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Money.NegativeAmount");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("US")]
    [InlineData("USDT")]
    public void Create_WithInvalidCurrencyLength_ReturnsValidationError(string invalidCurrency)
    {
        // Act
        var result = Money.Create(50m, invalidCurrency);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Money.InvalidCurrency");
    }

    [Fact]
    public void Equals_WithSameAmountAndCurrency_ReturnsTrue()
    {
        // Arrange
        var m1 = Money.Create(49.99m, "USD").Value;
        var m2 = Money.Create(49.99m, "USD").Value;

        // Assert
        m1.Should().Be(m2);
        (m1 == m2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentCurrency_ReturnsFalse()
    {
        // Arrange
        var m1 = Money.Create(49.99m, "USD").Value;
        var m2 = Money.Create(49.99m, "EUR").Value;

        // Assert
        m1.Should().NotBe(m2);
    }
}
