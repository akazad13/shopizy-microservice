using Shopizy.Catelog.API;
using Shopizy.Catelog.API.Mapping;
using Shopizy.Security;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddSecurity(builder.Configuration);

builder.Services.AddControllers();

builder.Services
    .AddEndpointsApiExplorer()
    .AddSwaggerGen();

builder.Services.AddMappings();

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    _ = app
        .UseSwagger()
        .UseSwaggerUI();
}

app
    .UseHttpsRedirection()
    .UseAuthorization();

app.MapControllers();

using (IServiceScope scope = app.Services.CreateScope())
{
    DbMigrationsHelper initialiser = scope.ServiceProvider.GetRequiredService<DbMigrationsHelper>();
    await initialiser.MigrateAsync();
}


await app.RunAsync();
