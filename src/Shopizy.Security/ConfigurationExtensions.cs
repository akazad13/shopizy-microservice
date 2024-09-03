using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shopizy.Security;
using Shopizy.Security.Policies;
using Shopizy.Security.TokenGenerator;
using Shopizy.Security.TokenValidation;
using Shopizy.Security.User;

namespace Shopizy.Security;

public static class ConfigurationExtensions
{
    public static IServiceCollection AddSecurity(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        ArgumentNullException.ThrowIfNull(services);
        return services
            .AddAuthorization()
            .AddAuthentication(configuration);
    }

    public static IServiceCollection AddAuthorization(this IServiceCollection services)
    {
        _ = services.AddScoped<IAuthorizationService, AuthorizationService>();
        _ = services.AddScoped<ICurrentUserProvider, CurrentUserProvider>();
        _ = services.AddSingleton<IPolicyEnforcer, PolicyEnforcer>();

        return services;
    }

    public static IServiceCollection AddAuthentication(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        _ = services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.Section));

        _ = services
            .ConfigureOptions<JwtBearerToeknValidationConfiguration>()
            .AddAuthentication(defaultScheme: JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();

        return services;
    }
}
