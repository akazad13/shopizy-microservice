using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Shopizy.ShippingService.Application.Interfaces;
using Shopizy.ShippingService.Application.Services;
using Shopizy.ShippingService.Infrastructure.Persistence;
using Shopizy.ShippingService.Infrastructure.Persistence.Repositories;

namespace Shopizy.ShippingService.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddShippingInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("shippingdb");
        if (string.IsNullOrEmpty(connectionString))
        {
            services.AddDbContext<ShippingDbContext>(options =>
                options.UseInMemoryDatabase("ShippingDb"));
        }
        else
        {
            services.AddDbContext<ShippingDbContext>(options =>
                options.UseNpgsql(connectionString));
        }

        services.AddScoped<IShipmentRepository, ShipmentRepository>();
        services.AddScoped<ShippingApplicationService>();

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
