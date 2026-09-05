using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Shopizy.PaymentService.Application.Interfaces;
using Shopizy.PaymentService.Application.Services;
using Shopizy.PaymentService.Infrastructure.Gateway;
using Shopizy.PaymentService.Infrastructure.Persistence;
using Shopizy.PaymentService.Infrastructure.Persistence.Repositories;
using Shopizy.SharedKernel.Middleware.Idempotency;

namespace Shopizy.PaymentService.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddPaymentInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<DbContextOptionsBuilder>? configureDbContext = null)
    {
        if (configureDbContext is not null)
        {
            services.AddDbContext<PaymentDbContext>(configureDbContext);
        }
        else
        {
            services.AddDbContext<PaymentDbContext>(options =>
                options.UseInMemoryDatabase("Shopizy_PaymentDb"));
        }

        // Repositories & Gateway
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddSingleton<IPaymentGatewayProvider, MockPaymentGatewayProvider>();
        services.AddScoped<PaymentApplicationService>();

        // Idempotency
        services.AddSingleton<IIdempotencyStore, InMemoryIdempotencyStore>();

        // JWT Bearer Auth
        var jwtKey = configuration["Jwt:Key"] ?? "shopizy-payment-dev-secret-key-32ch!";
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
