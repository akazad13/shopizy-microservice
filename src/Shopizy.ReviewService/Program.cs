using Shopizy.ReviewService.Endpoints;
using Shopizy.ReviewService.Extensions;
using Shopizy.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations
builder.AddServiceDefaults();

// Add Review & Wishlist services
builder.Services.AddReviewServices(builder.Configuration);

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
app.MapReviewEndpoints();

app.Run();

public partial class Program { }
