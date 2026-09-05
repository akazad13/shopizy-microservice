using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Shopizy.CartAbandonmentWorker.Application.Interfaces;
using Shopizy.CartAbandonmentWorker.Application.Services;
using Shopizy.CartAbandonmentWorker.Infrastructure.Clients;
using Shopizy.CartAbandonmentWorker.Infrastructure.Persistence;
using Shopizy.CartAbandonmentWorker.Infrastructure.Persistence.Repositories;

namespace Shopizy.CartAbandonmentWorker.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddCartAbandonmentServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("abandonmentdb");
        if (string.IsNullOrEmpty(connectionString))
        {
            services.AddDbContext<AbandonmentDbContext>(options =>
                options.UseInMemoryDatabase("AbandonmentDb_InMemory"));
        }
        else
        {
            services.AddDbContext<AbandonmentDbContext>(options =>
                options.UseNpgsql(connectionString));
        }

        // Repositories & Clients
        services.AddScoped<IAbandonedCartRepository, AbandonedCartRepository>();
        services.AddSingleton<ICartSnapshotClient, MockCartSnapshotClient>();
        services.AddSingleton<INotificationDispatcherClient, MockNotificationDispatcherClient>();
        services.AddScoped<CartAbandonmentApplicationService>();

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
