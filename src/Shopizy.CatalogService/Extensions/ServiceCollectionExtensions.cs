using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Shopizy.CatalogService.Application.Interfaces;
using Shopizy.CatalogService.Application.Services;
using Shopizy.CatalogService.Endpoints;
using Shopizy.CatalogService.Infrastructure.Persistence;
using Shopizy.CatalogService.Infrastructure.Persistence.Repositories;
using Shopizy.SharedKernel.Middleware.Idempotency;

namespace Shopizy.CatalogService.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCatalogInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<DbContextOptionsBuilder>? configureDbContext = null)
    {
        var jwtSecret = configuration["Jwt:Secret"] ?? "Shopizy_Super_Secret_Jwt_Signing_Key_2026_Secure_Key!";
        var jwtIssuer = configuration["Jwt:Issuer"] ?? "Shopizy.IdentityService";
        var jwtAudience = configuration["Jwt:Audience"] ?? "Shopizy.Clients";

        if (configureDbContext is not null)
        {
            services.AddDbContext<CatalogDbContext>(configureDbContext);
        }
        else
        {
            services.AddDbContext<CatalogDbContext>(options =>
            {
                options.UseInMemoryDatabase("ShopizyCatalogDb");
            });
        }

        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IBrandRepository, BrandRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICatalogService, Application.Services.CatalogService>();
        services.AddSingleton<IIdempotencyStore, InMemoryIdempotencyStore>();

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
                ValidateIssuer = true,
                ValidIssuer = jwtIssuer,
                ValidateAudience = true,
                ValidAudience = jwtAudience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("StoreAdminOnly", policy =>
                policy.RequireRole("StoreAdmin"));
        });

        return services;
    }

    public static IEndpointRouteBuilder MapCatalogEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapCategoryEndpoints();
        app.MapBrandEndpoints();
        app.MapProductEndpoints();
        return app;
    }
}
