using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Shopizy.PromotionService.Application.Interfaces;
using Shopizy.PromotionService.Application.Services;
using Shopizy.PromotionService.Infrastructure.Calculators;
using Shopizy.PromotionService.Infrastructure.Persistence;
using Shopizy.PromotionService.Infrastructure.Persistence.Repositories;

namespace Shopizy.PromotionService.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddPromotionInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("promotiondb");
        if (string.IsNullOrEmpty(connectionString))
        {
            services.AddDbContext<PromotionDbContext>(options =>
                options.UseInMemoryDatabase("PromotionDb"));
        }
        else
        {
            services.AddDbContext<PromotionDbContext>(options =>
                options.UseNpgsql(connectionString));
        }

        services.AddScoped<IPromotionRepository, PromotionRepository>();
        services.AddSingleton<IPromotionCalculator, DefaultPromotionCalculator>();
        services.AddScoped<PromotionApplicationService>();

        var jwtKey = configuration["Jwt:Key"] ?? "ShopizySecretKeyForDevelopmentPurposesOnly1234567890!";
        var key = Encoding.UTF8.GetBytes(jwtKey);

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                };
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("StoreAdminOnly", policy => policy.RequireRole("StoreAdmin"));
        });

        return services;
    }
}
