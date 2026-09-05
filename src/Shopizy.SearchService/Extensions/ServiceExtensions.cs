using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Shopizy.SearchService.Application.Interfaces;
using Shopizy.SearchService.Application.Services;
using Shopizy.SearchService.Infrastructure.Indexing;
using Shopizy.SearchService.Infrastructure.Synonyms;

namespace Shopizy.SearchService.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddSearchInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<ISearchIndexStore, InMemorySearchIndexStore>();
        services.AddSingleton<ISynonymProvider, RetailSynonymProvider>();
        services.AddScoped<SearchApplicationService>();

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
