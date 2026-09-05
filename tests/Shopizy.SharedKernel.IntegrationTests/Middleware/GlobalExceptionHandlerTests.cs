using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Shopizy.SharedKernel.Domain;
using Shopizy.SharedKernel.Middleware;
using Xunit;

namespace Shopizy.SharedKernel.IntegrationTests.Middleware;

public class GlobalExceptionHandlerTests
{
    private static async Task<(WebApplication App, HttpClient Client)> CreateTestServerAsync()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();

        builder.Services.AddRouting();
        builder.Services.AddProblemDetails();
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

        var app = builder.Build();
        app.UseExceptionHandler();
        app.UseRouting();

        app.MapGet("/test/domain-exception", () =>
        {
            throw new DomainException("Order.InvalidState", "Order cannot be placed without items.");
        });

        app.MapGet("/test/not-found", () =>
        {
            throw new KeyNotFoundException("Product 123 was not found.");
        });

        app.MapGet("/test/system-error", () =>
        {
            throw new InvalidOperationException("Fatal database crash.");
        });

        await app.StartAsync();
        var client = app.GetTestClient();
        return (app, client);
    }

    [Fact]
    public async Task WhenDomainExceptionThrown_Returns400WithRfc7807ProblemDetails()
    {
        var (app, client) = await CreateTestServerAsync();
        await using var _ = app;

        var response = await client.GetAsync("/test/domain-exception");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problem.Should().NotBeNull();
        problem!.Status.Should().Be(400);
        problem.Title.Should().Be("Domain Invariant Violation");
        problem.Detail.Should().Be("Order cannot be placed without items.");
        problem.Extensions.Should().ContainKey("errorCode");
        problem.Extensions["errorCode"]?.ToString().Should().Be("Order.InvalidState");
        problem.Extensions.Should().ContainKey("traceId");
    }

    [Fact]
    public async Task WhenKeyNotFoundThrown_Returns404WithProblemDetails()
    {
        var (app, client) = await CreateTestServerAsync();
        await using var _ = app;

        var response = await client.GetAsync("/test/not-found");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problem.Should().NotBeNull();
        problem!.Status.Should().Be(404);
        problem.Title.Should().Be("Resource Not Found");
    }

    [Fact]
    public async Task WhenGenericExceptionThrown_Returns500WithoutLeakingStacktrace()
    {
        var (app, client) = await CreateTestServerAsync();
        await using var _ = app;

        var response = await client.GetAsync("/test/system-error");

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problem.Should().NotBeNull();
        problem!.Status.Should().Be(500);
        problem.Title.Should().Be("Internal Server Error");
        problem.Detail.Should().NotContain("Fatal database crash");
        problem.Detail.Should().Be("An unexpected server error occurred. Please try again later.");
    }
}
