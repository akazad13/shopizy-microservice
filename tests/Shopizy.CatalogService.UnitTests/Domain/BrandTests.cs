using FluentAssertions;
using Shopizy.CatalogService.Domain.Entities;
using Shopizy.SharedKernel.Results;

namespace Shopizy.CatalogService.UnitTests.Domain;

public class BrandTests
{
    [Fact]
    public void Create_WithValidData_ReturnsSuccess()
    {
        // Act
        var result = Brand.Create("AudioTech", "audiotech", "Premium Audio Gear", "https://audiotech.test", "https://audiotech.test/logo.png");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("AudioTech");
        result.Value.Slug.Should().Be("audiotech");
        result.Value.WebsiteUrl.Should().Be("https://audiotech.test");
        result.Value.LogoUrl.Should().Be("https://audiotech.test/logo.png");
        result.Value.IsActive.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyName_ReturnsValidationError(string emptyName)
    {
        // Act
        var result = Brand.Create(emptyName, "audiotech");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Brand.EmptyName");
    }

    [Fact]
    public void Create_WithNameExceeding100Chars_ReturnsValidationError()
    {
        // Arrange
        var longName = new string('B', 101);

        // Act
        var result = Brand.Create(longName, "audiotech");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Brand.NameTooLong");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptySlug_ReturnsValidationError(string emptySlug)
    {
        // Act
        var result = Brand.Create("Brand Name", emptySlug);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Brand.EmptySlug");
    }

    [Fact]
    public void Update_WithValidData_UpdatesFields()
    {
        // Arrange
        var brand = Brand.Create("Brand One", "brand-one").Value;

        // Act
        var result = brand.Update("Brand Updated", "brand-updated", "New Desc", "https://new.test", "https://new.test/logo.png", false);

        // Assert
        result.IsSuccess.Should().BeTrue();
        brand.Name.Should().Be("Brand Updated");
        brand.Slug.Should().Be("brand-updated");
        brand.IsActive.Should().BeFalse();
        brand.UpdatedAtUtc.Should().NotBeNull();
    }
}
