using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.IdentityModel.Tokens;
using Shopizy.NotificationService.Application.Contracts;
using Shopizy.NotificationService.Domain.Enums;
using Xunit;

namespace Shopizy.NotificationService.E2ETests;

public class NotificationE2ETests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private static readonly Guid _customer1Id = Guid.NewGuid();
    private static readonly Guid _customer2Id = Guid.NewGuid();

    public NotificationE2ETests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    private static string GenerateJwt(string role, Guid? userId = null)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("ShopizySecretKeyForDevelopmentPurposesOnly1234567890!"));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var subjectId = userId ?? Guid.NewGuid();
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, subjectId.ToString()),
            new Claim("sub", subjectId.ToString()),
            new Claim(ClaimTypes.Role, role)
        };
        var token = new JwtSecurityToken(claims: claims, expires: DateTime.UtcNow.AddHours(1), signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    // E2E-01: Admin dispatches transactional notification -> 201 Created, status Sent
    [Fact]
    public async Task E2E_1_Admin_SendsNotification_Returns201AndPersisted()
    {
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", GenerateJwt("StoreAdmin"));

        var request = new SendNotificationRequest(
            _customer1Id,
            "customer@example.com",
            NotificationType.OrderConfirmation,
            NotificationChannel.Email,
            "Order Confirmation",
            "Your order has been placed successfully.");

        var response = await _client.PostAsJsonAsync("/api/v1/notifications/send", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<NotificationResponse>();
        result.Should().NotBeNull();
        result!.Status.Should().Be(NotificationStatus.Sent);
        result.Recipient.Should().Be("customer@example.com");
    }

    // E2E-02: Customer queries own notifications -> returns own notifications
    [Fact]
    public async Task E2E_2_Customer_QueriesOwnNotifications_ReturnsOwnNotifications()
    {
        var customerId = Guid.NewGuid();
        var adminToken = GenerateJwt("StoreAdmin");

        // Admin sends notification to this specific customer
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", adminToken);

        var sendReq = new SendNotificationRequest(
            customerId,
            "myaccount@example.com",
            NotificationType.ShipmentDispatched,
            NotificationChannel.Email,
            "Your package has shipped!",
            "Track your order at https://shopizy.com/track/trk_ups_123");

        await _client.PostAsJsonAsync("/api/v1/notifications/send", sendReq);

        // Customer queries their own history
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", GenerateJwt("Customer", customerId));

        var response = await _client.GetAsync($"/api/v1/notifications/user/{customerId}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var notifications = await response.Content.ReadFromJsonAsync<List<NotificationResponse>>();
        notifications.Should().NotBeNull();
        notifications.Should().ContainSingle(n => n.Recipient == "myaccount@example.com");
    }

    // E2E-03: Customer tries to access another customer's notifications -> 403 Forbidden
    [Fact]
    public async Task E2E_3_Customer_AccessesOtherCustomerNotifications_Returns403()
    {
        var customer1Id = Guid.NewGuid();
        var customer2Id = Guid.NewGuid();

        // Customer 1 auth token but requests Customer 2's notifications
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", GenerateJwt("Customer", customer1Id));

        var response = await _client.GetAsync($"/api/v1/notifications/user/{customer2Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // E2E-04: Unauthenticated user tries to send notification -> 401 Unauthorized
    [Fact]
    public async Task E2E_4_Unauthenticated_SendNotification_Returns401()
    {
        _client.DefaultRequestHeaders.Authorization = null;

        var request = new SendNotificationRequest(
            Guid.NewGuid(),
            "hacker@example.com",
            NotificationType.OrderConfirmation,
            NotificationChannel.Email,
            "Hacked!",
            "Malicious content");

        var response = await _client.PostAsJsonAsync("/api/v1/notifications/send", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // E2E-05: Admin broadcasts live order status update -> 200 OK with broadcasted flag
    [Fact]
    public async Task E2E_5_Admin_PushesOrderStatus_Returns200AndBroadcastedTrue()
    {
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", GenerateJwt("StoreAdmin"));

        var request = new OrderStatusPushRequest(
            Guid.NewGuid(),
            _customer1Id,
            "Shipped",
            "trk_fedex_abc123xyz",
            "FedEx");

        var response = await _client.PostAsJsonAsync("/api/v1/notifications/push/order-status", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<OrderStatusPushResponse>();
        result.Should().NotBeNull();
        result!.Broadcasted.Should().BeTrue();
        result.Status.Should().Be("Shipped");
    }

    // E2E-06: Customer tries to push merchant event -> 403 Forbidden
    [Fact]
    public async Task E2E_6_Customer_PushesMerchantEvent_Returns403()
    {
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", GenerateJwt("Customer"));

        var request = new MerchantEventPushRequest(
            "NewOrderPlaced",
            299.99m,
            "USD",
            "Order for 3 items placed");

        var response = await _client.PostAsJsonAsync("/api/v1/notifications/push/merchant-event", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
