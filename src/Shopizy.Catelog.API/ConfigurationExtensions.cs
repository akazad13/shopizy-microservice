using CloudinaryDotNet;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Shopizy.Catelog.API.ExternalServices.MediaUploader;
using Shopizy.Catelog.API.ExternalServices.MediaUploader.CloudinaryService;
using Shopizy.Catelog.API.Persistence;
using Shopizy.Catelog.API.Persistence.Categories;
using Shopizy.Catelog.API.Persistence.ProductReviews;
using Shopizy.Catelog.API.Persistence.Products;
using Shopizy.Domain.Models.Persistence;

namespace Shopizy.Catelog.API;

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
            .AddServices(configuration)
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
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        _ = services
            .AddScoped<IAppDbContext, CatelogDbContext>()
            .AddScoped<DbMigrationsHelper>();

        _ = services.Configure<CloudinarySettings>(
            configuration.GetSection(CloudinarySettings.Section)
        );

        _ = services.AddTransient<ICloudinary, Cloudinary>(sp =>
        {
            var acc = new CloudinaryDotNet.Account(
                configuration.GetValue<string>("CloudinarySettings:CloudName"),
                configuration.GetValue<string>("CloudinarySettings:ApiKey"),
                configuration.GetValue<string>("CloudinarySettings:ApiSecret")
            );
            var cloudinary = new Cloudinary(acc);
            cloudinary.Api.Secure = configuration.GetValue<bool>("CloudinarySettings:Secure");
            return cloudinary;
        });
        _ = services.AddScoped<IMediaUploader, CloudinaryMediaUploader>();

        return services;
    }

    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        _ = services.AddDbContext<CatelogDbContext>(
            options => options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"))
        );
        return services;
    }

    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        _ = services
            .AddScoped<ICategoryRepository, CategoryRepository>()
            .AddScoped<IProductReviewRepository, ProductReviewRepository>()
            .AddScoped<IProductRepository, ProductRepository>();

        return services;
    }
}
