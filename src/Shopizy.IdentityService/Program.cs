using Shopizy.IdentityService.Endpoints;
using Shopizy.IdentityService.Extensions;
using Shopizy.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddIdentityInfrastructure(builder.Configuration);

var app = builder.Build();

app.MapDefaultEndpoints();

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<Shopizy.SharedKernel.Middleware.Idempotency.IdempotencyMiddleware>();

app.MapIdentityEndpoints();

app.Run();

public partial class Program { }
