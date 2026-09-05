using Shopizy.CatalogService.Extensions;
using Shopizy.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddCatalogInfrastructure(builder.Configuration);

var app = builder.Build();

app.MapDefaultEndpoints();

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<Shopizy.SharedKernel.Middleware.Idempotency.IdempotencyMiddleware>();

app.MapCatalogEndpoints();

app.Run();

public partial class Program { }
