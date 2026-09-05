using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Shopizy.CartAbandonmentWorker.Application.Contracts;
using Shopizy.CartAbandonmentWorker.Application.Interfaces;
using Shopizy.CartAbandonmentWorker.Infrastructure.Clients;
using Xunit;

namespace Shopizy.CartAbandonmentWorker.E2ETests;

public class CartAbandonmentE2ETests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public CartAbandonmentE2ETests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private static string GenerateJwt(string role, Guid? userId = null, string name = "TestUser")
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("ShopizySecretKeyForDevelopmentPurposesOnly1234567890!"));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var subjectId = userId ?? Guid.NewGuid();
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, subjectId.ToString()),
            new Claim("sub", subjectId.ToString()),
            new Claim(ClaimTypes.Name, name),
            new Claim(ClaimTypes.Role, role)
        };
        var token = new JwtSecurityToken(claims: claims, expires: DateTime.UtcNow.AddHours(1), signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    [Fact]
    public async Task E2E_1_AdminTriggersSweep_AbandonedCartGeneratesRecordAndDispatchesEmail()
    {
        var cartSnapshotClient = _factory.Services.GetRequiredService<ICartSnapshotClient>() as MockCartSnapshotClient;
        var notificationClient = _factory.Services.GetRequiredService<INotificationDispatcherClient>() as MockNotificationDispatcherClient;
        cartSnapshotClient.Should().NotBeNull();
        notificationClient.Should().NotBeNull();

        var cartId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        cartSnapshotClient!.RegisterCartSnapshot(new CartSnapshotDto(
            cartId,
            customerId,
            "abandoned@example.com",
            150m,
            2,
            DateTime.UtcNow.AddHours(-3), // Inactive 3 hours (>= 2h)
            "[{\"productId\":\"prod1\",\"quantity\":2}]"
        ));

        var req = new HttpRequestMessage(HttpMethod.Post, "/api/v1/cart-abandonment/sweep");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", GenerateJwt("StoreAdmin"));

        var res = await _client.SendAsync(req);
        res.StatusCode.Should().Be(HttpStatusCode.OK);

        var sweepResult = await res.Content.ReadFromJsonAsync<AbandonmentSweepResult>();
        sweepResult.Should().NotBeNull();
        sweepResult!.RecoveriesDispatched.Should().BeGreaterThanOrEqualTo(1);

        // Verify notification was dispatched
        notificationClient!.DispatchedNotifications.Should().Contain(n => n.Email == "abandoned@example.com");
    }

    [Fact]
    public async Task E2E_2_ImmediateSecondSweep_IsSuppressedByCooldown()
    {
        var cartSnapshotClient = _factory.Services.GetRequiredService<ICartSnapshotClient>() as MockCartSnapshotClient;
        var notificationClient = _factory.Services.GetRequiredService<INotificationDispatcherClient>() as MockNotificationDispatcherClient;

        var cartId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        cartSnapshotClient!.RegisterCartSnapshot(new CartSnapshotDto(
            cartId,
            customerId,
            "cooldown@example.com",
            200m,
            1,
            DateTime.UtcNow.AddHours(-4),
            "[{\"productId\":\"prod2\",\"quantity\":1}]"
        ));

        // 1st sweep
        var req1 = new HttpRequestMessage(HttpMethod.Post, "/api/v1/cart-abandonment/sweep");
        req1.Headers.Authorization = new AuthenticationHeaderValue("Bearer", GenerateJwt("StoreAdmin"));
        var res1 = await _client.SendAsync(req1);
        res1.StatusCode.Should().Be(HttpStatusCode.OK);

        var initialNotificationCount = notificationClient!.DispatchedNotifications.Count(n => n.Email == "cooldown@example.com");
        initialNotificationCount.Should().Be(1);

        // 2nd sweep immediately
        var req2 = new HttpRequestMessage(HttpMethod.Post, "/api/v1/cart-abandonment/sweep");
        req2.Headers.Authorization = new AuthenticationHeaderValue("Bearer", GenerateJwt("StoreAdmin"));
        var res2 = await _client.SendAsync(req2);
        res2.StatusCode.Should().Be(HttpStatusCode.OK);

        var sweepResult2 = await res2.Content.ReadFromJsonAsync<AbandonmentSweepResult>();
        sweepResult2.Should().NotBeNull();
        sweepResult2!.SuppressedByCooldown.Should().BeGreaterThanOrEqualTo(1);

        var totalDispatched = notificationClient.DispatchedNotifications.Count(n => n.Email == "cooldown@example.com");
        totalDispatched.Should().Be(1); // No new email
    }

    [Fact]
    public async Task E2E_3_CartUpdatedRecently_IsIgnoredBySweep()
    {
        var cartSnapshotClient = _factory.Services.GetRequiredService<ICartSnapshotClient>() as MockCartSnapshotClient;
        var notificationClient = _factory.Services.GetRequiredService<INotificationDispatcherClient>() as MockNotificationDispatcherClient;

        var cartId = Guid.NewGuid();
        cartSnapshotClient!.RegisterCartSnapshot(new CartSnapshotDto(
            cartId,
            Guid.NewGuid(),
            "active@example.com",
            50m,
            1,
            DateTime.UtcNow.AddMinutes(-30), // Only 30 min ago (< 2h)
            "[{\"productId\":\"prod3\",\"quantity\":1}]"
        ));

        var req = new HttpRequestMessage(HttpMethod.Post, "/api/v1/cart-abandonment/sweep");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", GenerateJwt("StoreAdmin"));
        var res = await _client.SendAsync(req);
        res.StatusCode.Should().Be(HttpStatusCode.OK);

        notificationClient!.DispatchedNotifications.Should().NotContain(n => n.Email == "active@example.com");
    }

    [Fact]
    public async Task E2E_4_RestoreCart_WithValidToken_MarksRestoredAndReturnsDetails()
    {
        var cartSnapshotClient = _factory.Services.GetRequiredService<ICartSnapshotClient>() as MockCartSnapshotClient;
        var notificationClient = _factory.Services.GetRequiredService<INotificationDispatcherClient>() as MockNotificationDispatcherClient;

        var cartId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        cartSnapshotClient!.RegisterCartSnapshot(new CartSnapshotDto(
            cartId,
            customerId,
            "restore_user@example.com",
            85m,
            1,
            DateTime.UtcNow.AddHours(-3),
            "[{\"productId\":\"itemX\",\"quantity\":1}]"
        ));

        var sweepReq = new HttpRequestMessage(HttpMethod.Post, "/api/v1/cart-abandonment/sweep");
        sweepReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", GenerateJwt("StoreAdmin"));
        await _client.SendAsync(sweepReq);

        var sentEmail = notificationClient!.DispatchedNotifications.FirstOrDefault(n => n.Email == "restore_user@example.com");
        sentEmail.Should().NotBeNull();

        // Extract token from restore URL
        var token = sentEmail.RestoreUrl.Split('/').Last();

        var restoreRes = await _client.GetAsync($"/api/v1/cart-abandonment/restore/{token}");
        restoreRes.StatusCode.Should().Be(HttpStatusCode.OK);

        var restored = await restoreRes.Content.ReadFromJsonAsync<RestoreCartResponse>();
        restored.Should().NotBeNull();
        restored!.CartId.Should().Be(cartId);
        restored.CustomerId.Should().Be(customerId);
        restored.Expired.Should().BeFalse();
    }

    [Fact]
    public async Task E2E_5_RestoreCart_WithInvalidToken_Returns404NotFound()
    {
        var res = await _client.GetAsync("/api/v1/cart-abandonment/restore/nonexistent-token-12345");
        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task E2E_6_Security_UnauthorizedOrNonAdmin_SweepBlocked()
    {
        // Unauthenticated -> 401
        var unauthReq = new HttpRequestMessage(HttpMethod.Post, "/api/v1/cart-abandonment/sweep");
        var unauthRes = await _client.SendAsync(unauthReq);
        unauthRes.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        // Customer role -> 403
        var customerReq = new HttpRequestMessage(HttpMethod.Post, "/api/v1/cart-abandonment/sweep");
        customerReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", GenerateJwt("Customer"));
        var customerRes = await _client.SendAsync(customerReq);
        customerRes.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
