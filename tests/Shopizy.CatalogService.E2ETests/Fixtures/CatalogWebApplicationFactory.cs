using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Shopizy.CatalogService.Infrastructure.Persistence;

namespace Shopizy.CatalogService.E2ETests.Fixtures;

public class CatalogWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = $"CatalogE2EDb_{Guid.NewGuid()}";
    public const string TestSecret = "Shopizy_Super_Secret_Jwt_Signing_Key_2026_Secure_Key!";
    public const string TestIssuer = "Shopizy.IdentityService";
    public const string TestAudience = "Shopizy.Clients";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<CatalogDbContext>));
            if (descriptor is not null)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<CatalogDbContext>(options =>
            {
                options.UseInMemoryDatabase(_dbName);
            });
        });
    }

    public static string GenerateJwtToken(string role, Guid? userId = null)
    {
        var id = userId ?? Guid.NewGuid();
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(TestSecret);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, id.ToString()),
                new Claim("sub", id.ToString()),
                new Claim(ClaimTypes.Role, role),
                new Claim("role", role)
            ]),
            Expires = DateTime.UtcNow.AddHours(2),
            Issuer = TestIssuer,
            Audience = TestAudience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
