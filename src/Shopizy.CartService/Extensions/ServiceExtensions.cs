using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Shopizy.CartService.Application.Common.Interfaces;
using Shopizy.CartService.Infrastructure.Catalog;
using Shopizy.CartService.Infrastructure.Redis;
using Shopizy.SharedKernel.Middleware.Idempotency;
using System.Text;

namespace Shopizy.CartService.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddCartInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Redis IDistributedCache (Aspire wires the connection string via "shopizy-redis")
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("shopizy-redis")
                ?? "localhost:6379";
        });

        // Repositories & Services
        services.AddScoped<ICartRepository, RedisCartRepository>();
        services.AddSingleton<StubCatalogPriceService>();
        services.AddScoped<ICatalogPriceService>(sp => sp.GetRequiredService<StubCatalogPriceService>());
        services.AddScoped<Application.Services.CartCommandService>();

        // Idempotency store
        services.AddSingleton<IIdempotencyStore, InMemoryIdempotencyStore>();

        // JWT Bearer auth
        var jwtKey = configuration["Jwt:Key"] ?? "shopizy-cart-dev-secret-key-32ch!";
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(opts =>
            {
                opts.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.FromSeconds(30)
                };
            });

        services.AddAuthorization();
        services.AddProblemDetails();

        return services;
    }
}

