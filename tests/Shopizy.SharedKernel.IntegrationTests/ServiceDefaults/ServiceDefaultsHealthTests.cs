using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Shopizy.ServiceDefaults;
using Xunit;

namespace Shopizy.SharedKernel.IntegrationTests.ServiceDefaults;

public class ServiceDefaultsHealthTests
{
    [Fact]
    public void AddServiceDefaults_RegistersCoreServicesWithoutExceptions()
    {
        var builder = WebApplication.CreateBuilder();

        var act = () => builder.AddServiceDefaults();

        act.Should().NotThrow();
    }

    [Fact]
    public async Task HealthAndAliveEndpoints_ReturnHealthyInDevelopment()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Environment.EnvironmentName = Environments.Development;
        builder.WebHost.UseTestServer();

        builder.AddServiceDefaults();

        var app = builder.Build();
        app.MapDefaultEndpoints();

        await app.StartAsync();
        var client = app.GetTestClient();

        var aliveResponse = await client.GetAsync("/alive");
        aliveResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var healthResponse = await client.GetAsync("/health");
        healthResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
