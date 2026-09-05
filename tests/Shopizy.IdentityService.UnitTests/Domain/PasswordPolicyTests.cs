using FluentAssertions;
using Shopizy.IdentityService.Domain.Rules;

namespace Shopizy.IdentityService.UnitTests.Domain;

public sealed class PasswordPolicyTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Validate_WhenPasswordIsNullOrEmpty_ReturnsFailure(string? password)
    {
        // Act
        var result = PasswordPolicy.Validate(password);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Password.Empty");
    }

    [Theory]
    [InlineData("Ab1!short")]
    [InlineData("Passw0rd!12")] // 11 characters
    public void Validate_WhenPasswordShorterThan12Characters_ReturnsTooShort(string password)
    {
        // Act
        var result = PasswordPolicy.Validate(password);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Password.TooShort");
    }

    [Fact]
    public void Validate_WhenMissingUppercase_ReturnsMissingUppercase()
    {
        // Act (12+ characters, has lower, digit, special, but NO upper)
        var result = PasswordPolicy.Validate("password123456!");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Password.MissingUppercase");
    }

    [Fact]
    public void Validate_WhenMissingLowercase_ReturnsMissingLowercase()
    {
        // Act (12+ characters, has upper, digit, special, but NO lower)
        var result = PasswordPolicy.Validate("PASSWORD123456!");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Password.MissingLowercase");
    }

    [Fact]
    public void Validate_WhenMissingDigit_ReturnsMissingDigit()
    {
        // Act (12+ characters, has upper, lower, special, but NO digit)
        var result = PasswordPolicy.Validate("PasswordSpecial!");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Password.MissingDigit");
    }

    [Fact]
    public void Validate_WhenMissingSpecialCharacter_ReturnsMissingSpecial()
    {
        // Act (12+ characters, has upper, lower, digit, but NO special)
        var result = PasswordPolicy.Validate("Password1234567");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Password.MissingSpecial");
    }

    [Theory]
    [InlineData("SuperSecretPassword123!")]
    [InlineData("ValidPassw0rd!123")]
    [InlineData("C0mpl3x#P@ssword2026")]
    public void Validate_WhenAllCriteriaSatisfied_ReturnsSuccess(string strongPassword)
    {
        // Act
        var result = PasswordPolicy.Validate(strongPassword);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }
}
