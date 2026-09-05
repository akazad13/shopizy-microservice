using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Shopizy.NotificationService.Application.Interfaces;
using Shopizy.NotificationService.Application.Services;
using Shopizy.NotificationService.Infrastructure.Dispatchers;
using Shopizy.NotificationService.Infrastructure.Persistence;
using Shopizy.NotificationService.Infrastructure.Persistence.Repositories;

namespace Shopizy.NotificationService.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddNotificationInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("notificationdb");
        if (string.IsNullOrEmpty(connectionString))
        {
            services.AddDbContext<NotificationDbContext>(options =>
                options.UseInMemoryDatabase("NotificationDb"));
        }
        else
        {
            services.AddDbContext<NotificationDbContext>(options =>
                options.UseNpgsql(connectionString));
        }

        services.AddSignalR();

        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddSingleton<INotificationDispatcher, MockEmailDispatcher>();
        services.AddScoped<NotificationApplicationService>();

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

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                        {
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorizationBuilder()
            .AddPolicy("AdminOnly", policy => policy.RequireRole("StoreAdmin"));

        return services;
    }
}
