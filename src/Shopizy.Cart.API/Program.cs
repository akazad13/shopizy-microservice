using System.Text.Json.Serialization;
using Dapr.Client;
using Shopizy.Cart.API;
using Shopizy.Cart.API.Mapping;
using Shopizy.Dapr.QueryServices;
using Shopizy.Dapr.QueryServices.Products;
using Shopizy.Security;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
_ = builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddSecurity(builder.Configuration)
    .AddControllers().AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(System.Text.Json.JsonNamingPolicy.CamelCase));
    })
    .AddDapr(options =>
    {
        options.UseJsonSerializationOptions(new System.Text.Json.JsonSerializerOptions
        {
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters =
                {
                    new JsonStringEnumConverter(System.Text.Json.JsonNamingPolicy.CamelCase)
                }
        });
    });

_ = builder.Services.AddScoped<IQueryService<IsProductExistQuery, bool>, IsProductExistQueryService>(sp =>
{
    DaprClient dapr = sp.GetRequiredService<DaprClient>();
    return new IsProductExistQueryService(dapr);
});


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
_ = builder.Services
    .AddEndpointsApiExplorer()
    .AddSwaggerGen()
    .AddMappings();

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    _ = app
        .UseSwagger()
        .UseSwaggerUI();
}

_ = app
    .UseHttpsRedirection()
    .UseAuthorization();

_ = app.MapControllers();

using (IServiceScope scope = app.Services.CreateScope())
{
    DbMigrationsHelper initialiser = scope.ServiceProvider.GetRequiredService<DbMigrationsHelper>();
    await initialiser.MigrateAsync();
}

await app.RunAsync();
