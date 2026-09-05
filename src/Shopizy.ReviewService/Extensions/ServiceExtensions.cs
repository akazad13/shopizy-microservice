using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Shopizy.ReviewService.Application.Interfaces;
using Shopizy.ReviewService.Application.Services;
using Shopizy.ReviewService.Infrastructure.Clients;
using Shopizy.ReviewService.Infrastructure.Persistence;
using Shopizy.ReviewService.Infrastructure.Persistence.Repositories;

namespace Shopizy.ReviewService.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddReviewServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Persistence
        var connectionString = configuration.GetConnectionString("reviewdb");
        if (string.IsNullOrEmpty(connectionString))
        {
            services.AddDbContext<ReviewDbContext>(options =>
                options.UseInMemoryDatabase("ReviewDb_InMemory"));
        }
        else
        {
            services.AddDbContext<ReviewDbContext>(options =>
                options.UseNpgsql(connectionString));
        }

        // Repositories & Services
        services.AddScoped<IReviewRepository, ReviewRepository>();
        services.AddScoped<IWishlistRepository, WishlistRepository>();
        services.AddSingleton<IOrderVerificationClient, MockOrderVerificationClient>();
        services.AddScoped<ReviewApplicationService>();

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
