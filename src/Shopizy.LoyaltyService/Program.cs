using Shopizy.LoyaltyService.Endpoints;
using Shopizy.LoyaltyService.Extensions;
using Shopizy.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations
builder.AddServiceDefaults();

// Add Loyalty services
builder.Services.AddLoyaltyServices(builder.Configuration);

// Add OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthentication();
app.UseAuthorization();

// Map endpoints
app.MapLoyaltyEndpoints();

app.Run();

public partial class Program { }
