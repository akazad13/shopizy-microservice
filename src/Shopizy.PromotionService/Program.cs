using Shopizy.PromotionService.Endpoints;
using Shopizy.PromotionService.Extensions;
using Shopizy.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddPromotionInfrastructure(builder.Configuration);

var app = builder.Build();

app.MapDefaultEndpoints();

app.UseAuthentication();
app.UseAuthorization();

app.MapPromotionEndpoints();

app.Run();

public partial class Program { }
