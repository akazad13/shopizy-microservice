using Shopizy.CartService.Endpoints;
using Shopizy.CartService.Extensions;
using Shopizy.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddCartInfrastructure(builder.Configuration);

var app = builder.Build();

app.MapDefaultEndpoints();

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<Shopizy.SharedKernel.Middleware.Idempotency.IdempotencyMiddleware>();

app.MapCartEndpoints();

app.Run();

public partial class Program { }
