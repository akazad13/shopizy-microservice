using FluentAssertions;
using Shopizy.IdentityService.Domain.Entities;
using Shopizy.IdentityService.Domain.Enums;
using Shopizy.IdentityService.Domain.Events;
using Shopizy.IdentityService.Domain.ValueObjects;

namespace Shopizy.IdentityService.UnitTests.Domain;

public sealed class UserAggregateTests
{
    [Fact]
    public void Create_WhenValidParameters_InitializesUserAndRaisesDomainEvent()
    {
        // Arrange
        var email = Email.Create("customer@shopizy.test").Value;
        const string passwordHash = "hash123";

        // Act
        var user = User.Create("Alice", "Smith", email, passwordHash, UserRole.Customer);

        // Assert
        user.Id.Should().NotBeEmpty();
        user.FirstName.Should().Be("Alice");
        user.LastName.Should().Be("Smith");
        user.Email.Should().Be(email);
        user.PasswordHash.Should().Be(passwordHash);
        user.Role.Should().Be(UserRole.Customer);
        user.IsActive.Should().BeTrue();
        user.CreatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));

        user.DomainEvents.Should().ContainSingle();
        var domainEvent = user.DomainEvents.Single().Should().BeOfType<UserRegisteredDomainEvent>().Subject;
        domainEvent.UserId.Should().Be(user.Id);
        domainEvent.Email.Should().Be("customer@shopizy.test");
        domainEvent.Role.Should().Be(UserRole.Customer);
    }

    [Fact]
    public void AddRefreshToken_AddsTokenToCollection()
    {
        // Arrange
        var email = Email.Create("customer@shopizy.test").Value;
        var user = User.Create("Alice", "Smith", email, "hash123");
        var expiresAt = DateTime.UtcNow.AddDays(7);

        // Act
        var refreshToken = user.AddRefreshToken("sample-token-123", expiresAt);

        // Assert
        refreshToken.Should().NotBeNull();
        refreshToken.Token.Should().Be("sample-token-123");
        refreshToken.ExpiresAtUtc.Should().Be(expiresAt);
        user.RefreshTokens.Should().Contain(refreshToken);
        refreshToken.IsActive.Should().BeTrue();
    }

    [Fact]
    public void RevokeRefreshToken_MarksTokenAsRevoked()
    {
        // Arrange
        var email = Email.Create("customer@shopizy.test").Value;
        var user = User.Create("Alice", "Smith", email, "hash123");
        user.AddRefreshToken("token-to-revoke", DateTime.UtcNow.AddDays(7));

        // Act
        user.RevokeRefreshToken("token-to-revoke");

        // Assert
        var token = user.RefreshTokens.Single(rt => rt.Token == "token-to-revoke");
        token.IsRevoked.Should().BeTrue();
        token.IsActive.Should().BeFalse();
        token.RevokedAtUtc.Should().NotBeNull();
    }
}
