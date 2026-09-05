using FluentAssertions;
using Shopizy.IdentityService.Infrastructure.Security;

namespace Shopizy.IdentityService.UnitTests.Security;

public sealed class PasswordHasherTests
{
    private readonly PasswordHasher _hasher = new();

    [Fact]
    public void HashPassword_ProducesSaltedPbkdf2Hash()
    {
        // Act
        var hash = _hasher.HashPassword("SuperSecretPassword123!");

        // Assert
        hash.Should().NotBeNullOrWhiteSpace();
        var parts = hash.Split('.');
        parts.Length.Should().Be(3);
        parts[0].Should().Be("100000"); // Iterations
    }

    [Fact]
    public void HashPassword_WhenCalledTwiceWithSamePassword_GeneratesDifferentHashes()
    {
        // Act
        var hash1 = _hasher.HashPassword("SuperSecretPassword123!");
        var hash2 = _hasher.HashPassword("SuperSecretPassword123!");

        // Assert
        hash1.Should().NotBe(hash2);
    }

    [Fact]
    public void VerifyPassword_WhenPasswordMatches_ReturnsTrue()
    {
        // Arrange
        const string password = "SuperSecretPassword123!";
        var hash = _hasher.HashPassword(password);

        // Act
        var result = _hasher.VerifyPassword(password, hash);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_WhenPasswordDoesNotMatch_ReturnsFalse()
    {
        // Arrange
        var hash = _hasher.HashPassword("SuperSecretPassword123!");

        // Act
        var result = _hasher.VerifyPassword("WrongPassword123!", hash);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not-a-valid-hash")]
    [InlineData("100.invalidbase64.invalidbase64")]
    public void VerifyPassword_WhenHashIsMalformed_ReturnsFalse(string? malformedHash)
    {
        // Act
        var result = _hasher.VerifyPassword("AnyPassword123!", malformedHash!);

        // Assert
        result.Should().BeFalse();
    }
}
