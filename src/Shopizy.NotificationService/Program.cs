using Shopizy.NotificationService.Endpoints;
using Shopizy.NotificationService.Extensions;
using Shopizy.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddOpenApi();
builder.Services.AddNotificationInfrastructure(builder.Configuration);

var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapNotificationEndpoints();

app.Run();

public partial class Program { }
