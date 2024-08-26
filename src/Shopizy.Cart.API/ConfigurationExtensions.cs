using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Shopizy.Cart.API.Persistence;
using Shopizy.Domain.Models.Persistence;

namespace Shopizy.Cart.API;

public static class ConfigurationExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        ArgumentNullException.ThrowIfNull(services);
        return services
            .AddHttpContextAccessor()
            .AddServices()
            .AddBackgroundServices(configuration)
            .AddPersistence(configuration)
            .AddRepositories();
    }

    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(msc =>
        {
            msc.RegisterServicesFromAssembly(typeof(ConfigurationExtensions).Assembly);
            //msc.AddOpenBehavior(typeof(AuthorizationBehavior<,>));
            //msc.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });
        services.AddValidatorsFromAssemblyContaining(typeof(ConfigurationExtensions));

        return services;
    }

    private static IServiceCollection AddBackgroundServices(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        return services;
    }

    private static IServiceCollection AddServices(
        this IServiceCollection services
    )
    {
        services.AddScoped<IAppDbContext, CartDbContext>();
        services.AddScoped<DbMigrationsHelper>();

        return services;
    }

    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddDbContext<CartDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"))
        );
        return services;
    }

    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<ICartRepository, CartRepository>();

        return services;
    }
}
