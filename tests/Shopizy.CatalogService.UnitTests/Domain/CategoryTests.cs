using FluentAssertions;
using Shopizy.CatalogService.Domain.Entities;
using Shopizy.SharedKernel.Results;

namespace Shopizy.CatalogService.UnitTests.Domain;

public class CategoryTests
{
    [Fact]
    public void Create_WithValidData_ReturnsSuccess()
    {
        // Act
        var result = Category.Create("Electronics", "electronics", "Gadgets and devices");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Electronics");
        result.Value.Slug.Should().Be("electronics");
        result.Value.Description.Should().Be("Gadgets and devices");
        result.Value.IsActive.Should().BeTrue();
        result.Value.ParentCategoryId.Should().BeNull();
    }

    [Fact]
    public void Create_WithUppercaseSlug_NormalizesToLowercase()
    {
        // Act
        var result = Category.Create("Audio & Sound", "AUDIO-SOUND");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Slug.Should().Be("audio-sound");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyName_ReturnsValidationError(string emptyName)
    {
        // Act
        var result = Category.Create(emptyName, "valid-slug");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Validation);
        result.Error.Code.Should().Be("Category.EmptyName");
    }

    [Fact]
    public void Create_WithNameExceeding100Chars_ReturnsValidationError()
    {
        // Arrange
        var longName = new string('A', 101);

        // Act
        var result = Category.Create(longName, "slug");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Category.NameTooLong");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptySlug_ReturnsValidationError(string emptySlug)
    {
        // Act
        var result = Category.Create("Name", emptySlug);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Category.EmptySlug");
    }

    [Fact]
    public void Update_WhenSelfReferencingParent_ReturnsValidationError()
    {
        // Arrange
        var category = Category.Create("Smartphones", "smartphones").Value;

        // Act
        var result = category.Update("Smartphones", "smartphones", null, category.Id, true);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Category.SelfReferencingParent");
    }

    [Fact]
    public void Deactivate_SetsIsActiveToFalse()
    {
        // Arrange
        var category = Category.Create("Laptops", "laptops").Value;

        // Act
        category.Deactivate();

        // Assert
        category.IsActive.Should().BeFalse();
        category.UpdatedAtUtc.Should().NotBeNull();
    }
}
