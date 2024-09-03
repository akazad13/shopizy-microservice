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
            .AddBackgroundServices()
            .AddPersistence(configuration)
            .AddRepositories();
    }

    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        _ = services
            .AddMediatR(msc =>
            {
                _ = msc.RegisterServicesFromAssembly(typeof(ConfigurationExtensions).Assembly);
                //msc.AddOpenBehavior(typeof(AuthorizationBehavior<,>));
                //msc.AddOpenBehavior(typeof(ValidationBehavior<,>));
            })
            .AddValidatorsFromAssemblyContaining(typeof(ConfigurationExtensions));

        return services;
    }

    private static IServiceCollection AddBackgroundServices(
        this IServiceCollection services
    )
    {
        return services;
    }

    private static IServiceCollection AddServices(
        this IServiceCollection services
    )
    {
        _ = services
            .AddScoped<IAppDbContext, CartDbContext>()
            .AddScoped<DbMigrationsHelper>();

        return services;
    }

    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        _ = services.AddDbContext<CartDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"))
        );
        return services;
    }

    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        _ = services.AddScoped<ICartRepository, CartRepository>();

        return services;
    }
}
