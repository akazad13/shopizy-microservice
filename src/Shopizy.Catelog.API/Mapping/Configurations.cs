using Mapster;
using MapsterMapper;

namespace Shopizy.Catelog.API.Mapping;

public static class Configurations
{
    public static IServiceCollection AddMappings(this IServiceCollection services)
    {
        TypeAdapterConfig config = TypeAdapterConfig.GlobalSettings;
        _ = config.Scan(typeof(Configurations).Assembly);

        _ = services
            .AddSingleton(config)
            .AddScoped<IMapper, ServiceMapper>();
        return services;
    }
}
