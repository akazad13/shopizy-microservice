using Shopizy.PaymentService.Endpoints;
using Shopizy.PaymentService.Extensions;
using Shopizy.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddPaymentInfrastructure(builder.Configuration);

var app = builder.Build();

app.MapDefaultEndpoints();

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<Shopizy.SharedKernel.Middleware.Idempotency.IdempotencyMiddleware>();

app.MapPaymentEndpoints();

app.Run();

public partial class Program { }
