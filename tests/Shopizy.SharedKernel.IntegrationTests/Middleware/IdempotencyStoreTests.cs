using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Shopizy.SharedKernel.Middleware.Idempotency;
using Xunit;

namespace Shopizy.SharedKernel.IntegrationTests.Middleware;

public class IdempotencyStoreTests
{
    private static int _executionCounter = 0;

    private static async Task<(WebApplication App, HttpClient Client)> CreateTestServerAsync()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();

        builder.Services.AddSingleton<IIdempotencyStore, InMemoryIdempotencyStore>();
        builder.Services.AddRouting();

        var app = builder.Build();
        app.UseMiddleware<IdempotencyMiddleware>();
        app.UseRouting();

        app.MapPost("/api/orders", async context =>
        {
            Interlocked.Increment(ref _executionCounter);
            context.Response.StatusCode = 201;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync("{\"orderId\":\"12345\",\"status\":\"Created\"}");
        });

        await app.StartAsync();
        var client = app.GetTestClient();
        return (app, client);
    }

    [Fact]
    public async Task WhenSameIdempotencyKeySentTwice_ReturnsCachedPayloadWithoutReExecutingHandler()
    {
        Interlocked.Exchange(ref _executionCounter, 0);

        var (app, client) = await CreateTestServerAsync();
        await using var _ = app;

        var idempotencyKey = Guid.NewGuid().ToString();

        // Request 1
        var request1 = new HttpRequestMessage(HttpMethod.Post, "/api/orders");
        request1.Headers.Add("Idempotency-Key", idempotencyKey);
        var response1 = await client.SendAsync(request1);

        response1.StatusCode.Should().Be(HttpStatusCode.Created);
        var body1 = await response1.Content.ReadAsStringAsync();
        body1.Should().Contain("12345");
        response1.Headers.Contains("X-Cache-Lookup").Should().BeFalse();
        _executionCounter.Should().Be(1);

        // Request 2 (Duplicate)
        var request2 = new HttpRequestMessage(HttpMethod.Post, "/api/orders");
        request2.Headers.Add("Idempotency-Key", idempotencyKey);
        var response2 = await client.SendAsync(request2);

        response2.StatusCode.Should().Be(HttpStatusCode.Created);
        var body2 = await response2.Content.ReadAsStringAsync();
        body2.Should().Be(body1);
        response2.Headers.GetValues("X-Cache-Lookup").Should().Contain("HIT");

        // Assert handler was only executed once
        _executionCounter.Should().Be(1);
    }
}
