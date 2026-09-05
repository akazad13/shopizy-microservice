using Shopizy.CartAbandonmentWorker.Endpoints;
using Shopizy.CartAbandonmentWorker.Extensions;
using Shopizy.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations
builder.AddServiceDefaults();

// Add Cart Abandonment services
builder.Services.AddCartAbandonmentServices(builder.Configuration);

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
app.MapCartAbandonmentEndpoints();

app.Run();

public partial class Program { }
