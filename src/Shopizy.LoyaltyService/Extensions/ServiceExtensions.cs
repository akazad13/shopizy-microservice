using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Shopizy.LoyaltyService.Application.Interfaces;
using Shopizy.LoyaltyService.Application.Services;
using Shopizy.LoyaltyService.Infrastructure.Persistence;
using Shopizy.LoyaltyService.Infrastructure.Persistence.Repositories;

namespace Shopizy.LoyaltyService.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddLoyaltyServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("loyaltydb");
        if (string.IsNullOrEmpty(connectionString))
        {
            services.AddDbContext<LoyaltyDbContext>(options =>
                options.UseInMemoryDatabase("LoyaltyDb_InMemory"));
        }
        else
        {
            services.AddDbContext<LoyaltyDbContext>(options =>
                options.UseNpgsql(connectionString));
        }

        // Repositories & Services
        services.AddScoped<ILoyaltyRepository, LoyaltyRepository>();
        services.AddScoped<IGiftCardRepository, GiftCardRepository>();
        services.AddScoped<LoyaltyApplicationService>();

        // JWT Authentication
        var jwtKey = configuration["Jwt:Key"] ?? "ShopizySecretKeyForDevelopmentPurposesOnly1234567890!";
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
                };
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("StoreAdminOnly", policy => policy.RequireRole("StoreAdmin"));
        });

        return services;
    }
}
