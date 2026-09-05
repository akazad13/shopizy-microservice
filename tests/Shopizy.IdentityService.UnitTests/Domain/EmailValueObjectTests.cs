using FluentAssertions;
using Shopizy.IdentityService.Domain.ValueObjects;

namespace Shopizy.IdentityService.UnitTests.Domain;

public sealed class EmailValueObjectTests
{
    [Theory]
    [InlineData("test@example.com")]
    [InlineData("USER.NAME@DOMAIN.CO.UK")]
    [InlineData("customer+filter@shopizy.store")]
    public void Create_WhenEmailIsValid_ReturnsSuccessWithNormalizedLowercase(string input)
    {
        // Act
        var result = Email.Create(input);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(input.Trim().ToLowerInvariant());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WhenEmailIsEmpty_ReturnsFailure(string? input)
    {
        // Act
        var result = Email.Create(input);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Email.Empty");
    }

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("missingdomain@")]
    [InlineData("@missinguser.com")]
    [InlineData("spaces in@email.com")]
    public void Create_WhenEmailIsInvalidFormat_ReturnsFailure(string input)
    {
        // Act
        var result = Email.Create(input);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Email.InvalidFormat");
    }

    [Fact]
    public void Equals_WhenValuesMatch_AreStructurallyEqual()
    {
        // Arrange
        var email1 = Email.Create("user@example.com").Value;
        var email2 = Email.Create("USER@EXAMPLE.COM").Value;

        // Assert
        email1.Should().Be(email2);
        (email1 == email2).Should().BeTrue();
    }
}
