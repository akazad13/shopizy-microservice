using Shopizy.OrderService.Endpoints;
using Shopizy.OrderService.Extensions;
using Shopizy.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddOrderInfrastructure(builder.Configuration);

var app = builder.Build();

app.MapDefaultEndpoints();

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<Shopizy.SharedKernel.Middleware.Idempotency.IdempotencyMiddleware>();

app.MapOrderEndpoints();

app.Run();

public partial class Program { }
