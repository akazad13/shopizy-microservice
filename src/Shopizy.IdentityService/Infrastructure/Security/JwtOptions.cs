namespace Shopizy.IdentityService.Infrastructure.Security;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Secret { get; set; } = "Shopizy_Super_Secret_Jwt_Signing_Key_2026_Secure_Key!";
    public string Issuer { get; set; } = "Shopizy.IdentityService";
    public string Audience { get; set; } = "Shopizy.Clients";
    public int AccessTokenLifetimeMinutes { get; set; } = 60;
}
