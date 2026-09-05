using Shopizy.ServiceDefaults;
using Shopizy.ShippingService.Endpoints;
using Shopizy.ShippingService.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddShippingInfrastructure(builder.Configuration);

var app = builder.Build();

app.MapDefaultEndpoints();

app.UseAuthentication();
app.UseAuthorization();

app.MapShippingEndpoints();

app.Run();

public partial class Program { }
