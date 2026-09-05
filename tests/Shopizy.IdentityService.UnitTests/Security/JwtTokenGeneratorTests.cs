using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Shopizy.IdentityService.Domain.Entities;
using Shopizy.IdentityService.Domain.Enums;
using Shopizy.IdentityService.Domain.ValueObjects;
using Shopizy.IdentityService.Infrastructure.Security;

namespace Shopizy.IdentityService.UnitTests.Security;

public sealed class JwtTokenGeneratorTests
{
    private readonly JwtTokenGenerator _generator;

    public JwtTokenGeneratorTests()
    {
        var options = Options.Create(new JwtOptions
        {
            Secret = "Unit_Test_Secret_Key_For_Jwt_Generation_2026_Minimum_Length_Key!",
            Issuer = "Shopizy.TestIssuer",
            Audience = "Shopizy.TestAudience",
            AccessTokenLifetimeMinutes = 30
        });

        _generator = new JwtTokenGenerator(options);
    }

    [Fact]
    public void GenerateAccessToken_EmitsValidJwtWithUserClaims()
    {
        // Arrange
        var email = Email.Create("merchant@shopizy.test").Value;
        var user = User.Create("Admin", "User", email, "hash", UserRole.StoreAdmin);

        // Act
        var (token, expiresAtUtc) = _generator.GenerateAccessToken(user);

        // Assert
        token.Should().NotBeNullOrWhiteSpace();
        expiresAtUtc.Should().BeAfter(DateTime.UtcNow);

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        jwt.Issuer.Should().Be("Shopizy.TestIssuer");
        jwt.Audiences.Should().Contain("Shopizy.TestAudience");

        jwt.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == user.Id.ToString());
        jwt.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == "merchant@shopizy.test");
        jwt.Claims.Should().Contain(c => (c.Type == "role" || c.Type == ClaimTypes.Role) && c.Value == "StoreAdmin");
    }

    [Fact]
    public void GenerateRefreshToken_ReturnsHexEncodedCryptographicToken()
    {
        // Act
        var token1 = _generator.GenerateRefreshToken();
        var token2 = _generator.GenerateRefreshToken();

        // Assert
        token1.Should().NotBeNullOrWhiteSpace();
        token1.Length.Should().Be(64); // 32 bytes hex
        token1.Should().NotBe(token2);
    }
}
