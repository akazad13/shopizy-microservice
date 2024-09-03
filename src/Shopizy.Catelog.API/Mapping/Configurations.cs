using Mapster;
using MapsterMapper;

namespace Shopizy.Catelog.API.Mapping;

public static class Configurations
{
    public static IServiceCollection AddMappings(this IServiceCollection services)
    {
        TypeAdapterConfig config = TypeAdapterConfig.GlobalSettings;
        config.Scan(typeof(Configurations).Assembly);

        services.AddSingleton(config);
        services.AddScoped<IMapper, ServiceMapper>();
        return services;
    }
}
