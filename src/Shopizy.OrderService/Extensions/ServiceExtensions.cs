using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Shopizy.OrderService.Application.Interfaces;
using Shopizy.OrderService.Application.Services;
using Shopizy.OrderService.Infrastructure.Persistence;
using Shopizy.OrderService.Infrastructure.Persistence.Repositories;
using Shopizy.SharedKernel.Middleware.Idempotency;
using System.Text;

namespace Shopizy.OrderService.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddOrderInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<DbContextOptionsBuilder>? configureDbContext = null)
    {
        if (configureDbContext is not null)
        {
            services.AddDbContext<OrderDbContext>(configureDbContext);
        }
        else
        {
            services.AddDbContext<OrderDbContext>(options =>
                options.UseInMemoryDatabase("Shopizy_OrderDb"));
        }

        // Repositories & Services
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IInventoryRepository, InventoryRepository>();
        services.AddScoped<OrderApplicationService>();

        // Idempotency store
        services.AddSingleton<IIdempotencyStore, InMemoryIdempotencyStore>();

        // JWT Bearer auth
        var jwtKey = configuration["Jwt:Key"] ?? "shopizy-order-dev-secret-key-32ch!";
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
