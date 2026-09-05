using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Testing;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Shopizy.SharedKernel.IntegrationTests.AppHost;

public class AspireAppHostTests
{
    [Fact]
    public async Task AppHost_ConfiguresPostgresRedisAndRabbitMqResources()
    {
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.Shopizy_AppHost>();
        await using var app = await appHost.BuildAsync();

        var appModel = app.Services.GetRequiredService<DistributedApplicationModel>();

        appModel.Resources.Should().Contain(r => r.Name == "shopizy-postgres");
        appModel.Resources.Should().Contain(r => r.Name == "shopizy-redis");
        appModel.Resources.Should().Contain(r => r.Name == "shopizy-rabbitmq");
    }
}
