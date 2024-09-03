using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Shopizy.Security.TokenGenerator;

public class JwtTokenGenerator(IOptions<JwtSettings> jwtOptoins) : IJwtTokenGenerator
{
    private readonly JwtSettings _jwtSettings = jwtOptoins.Value;

    public string GenerateToken(
        Guid userId,
        string firstName,
        string LastName,
        string phone,
        IList<string> roles,
        IList<string> Permissions
    )
    {
        _ = Guard.Against.Null(roles);
        _ = Guard.Against.Null(Permissions);

        var claims = new List<Claim>
        {
            new("id", userId.ToString()),
            new(JwtRegisteredClaimNames.Name, firstName),
            new(ClaimTypes.Surname, LastName),
            new(ClaimTypes.MobilePhone, phone),
        };

        foreach (string role in roles)
        {
            claims.Add(new(ClaimTypes.Role, role));
        }

        foreach (string permission in Permissions)
        {
            claims.Add(new("permissions", permission));
        }

        var creds = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret)),
            SecurityAlgorithms.HmacSha256
        );

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            IssuedAt = DateTime.UtcNow,
            NotBefore = DateTime.UtcNow,
            Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.TokenExpirationMinutes),
            Subject = new ClaimsIdentity(claims),
            SigningCredentials = creds,
        };

        var jwtTokenHandler = new JwtSecurityTokenHandler();
        JwtSecurityToken jwtToken = jwtTokenHandler.CreateJwtSecurityToken(tokenDescriptor);
        string token = jwtTokenHandler.WriteToken(jwtToken);
        return token;
    }
}
