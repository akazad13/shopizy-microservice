using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.IdentityModel.Tokens;
using Shopizy.ShippingService.Application.Contracts;
using Shopizy.ShippingService.Domain.Enums;
using Shopizy.ShippingService.Domain.Services;
using Xunit;

namespace Shopizy.ShippingService.E2ETests;

public class ShippingE2ETests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ShippingE2ETests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    private static string GenerateJwt(string role)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("ShopizySecretKeyForDevelopmentPurposesOnly1234567890!"));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, role)
        };
        var token = new JwtSecurityToken(claims: claims, expires: DateTime.UtcNow.AddHours(1), signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    [Fact]
    public async Task E2E_1_CarrierRateCalculation_ReturnsAllFourCarriers()
    {
        var req = new CalculateShippingRatesRequest(60m, 2.5m, "90210", "US");
        var res = await _client.PostAsJsonAsync("/api/v1/shipping/rates", req);

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        var rates = await res.Content.ReadFromJsonAsync<List<ShippingRateQuote>>();
        rates.Should().NotBeNull();
        rates!.Should().HaveCount(4);
        rates.Select(r => r.Carrier).Should().Contain(new[] { "USPS", "UPS", "FedEx", "DHL" });
    }

    [Fact]
    public async Task E2E_2_FreeShippingThreshold_AppliesZeroCostOn80DollarCart()
    {
        var req = new CalculateShippingRatesRequest(80m, 1.5m, "90210", "US");
        var res = await _client.PostAsJsonAsync("/api/v1/shipping/rates", req);

        var rates = await res.Content.ReadFromJsonAsync<List<ShippingRateQuote>>();
        var usps = rates!.First(r => r.Carrier == "USPS");
        usps.Cost.Should().Be(0.00m);
        usps.Description.Should().Contain("Free Ground Shipping");
    }

    [Fact]
    public async Task E2E_3_SubThresholdRate_CalculatesGroundCostOn50DollarCart()
    {
        var req = new CalculateShippingRatesRequest(50m, 1.5m, "90210", "US");
        var res = await _client.PostAsJsonAsync("/api/v1/shipping/rates", req);

        var rates = await res.Content.ReadFromJsonAsync<List<ShippingRateQuote>>();
        var usps = rates!.First(r => r.Carrier == "USPS");
        usps.Cost.Should().BeGreaterThan(0.00m);
    }

    [Fact]
    public async Task E2E_4_AdminShipmentCreation_GeneratesTrackingNumberAndLabelMilestone()
    {
        var adminToken = GenerateJwt("StoreAdmin");
        var req = new HttpRequestMessage(HttpMethod.Post, "/api/v1/shipping/shipments")
        {
            Content = JsonContent.Create(new CreateShipmentRequest(
                Guid.NewGuid(), "UPS", "Ground", 2.0m, "100 Pine St", "94111", 3))
        };
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var res = await _client.SendAsync(req);
        res.StatusCode.Should().Be(HttpStatusCode.Created);
        var shipment = await res.Content.ReadFromJsonAsync<ShipmentResponse>();

        shipment.Should().NotBeNull();
        shipment!.TrackingNumber.Should().StartWith("trk_ups_");
        shipment.Status.Should().Be(ShipmentStatus.LabelCreated);
        shipment.Milestones.Should().HaveCount(1);
    }

    [Fact]
    public async Task E2E_5_MilestoneProgression_AppendsTrackingScans()
    {
        var adminToken = GenerateJwt("StoreAdmin");
        var createReq = new HttpRequestMessage(HttpMethod.Post, "/api/v1/shipping/shipments")
        {
            Content = JsonContent.Create(new CreateShipmentRequest(
                Guid.NewGuid(), "FedEx", "2-Day", 1.8m, "200 Oak Ave", "60611", 2))
        };
        createReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var createRes = await _client.SendAsync(createReq);
        var shipment = await createRes.Content.ReadFromJsonAsync<ShipmentResponse>();

        // Add InTransit milestone
        var inTransitReq = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/shipping/shipments/{shipment!.TrackingNumber}/milestones")
        {
            Content = JsonContent.Create(new AddMilestoneRequest(ShipmentStatus.InTransit, "Hub - Chicago, IL", "Departed origin hub"))
        };
        inTransitReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var inTransitRes = await _client.SendAsync(inTransitReq);
        inTransitRes.StatusCode.Should().Be(HttpStatusCode.OK);
        var inTransitShipment = await inTransitRes.Content.ReadFromJsonAsync<ShipmentResponse>();

        inTransitShipment!.Status.Should().Be(ShipmentStatus.InTransit);
        inTransitShipment.Milestones.Should().HaveCount(2);

        // Add Delivered milestone
        var deliveredReq = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/shipping/shipments/{shipment.TrackingNumber}/milestones")
        {
            Content = JsonContent.Create(new AddMilestoneRequest(ShipmentStatus.Delivered, "Front Porch", "Delivered to recipient"))
        };
        deliveredReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var deliveredRes = await _client.SendAsync(deliveredReq);
        var deliveredShipment = await deliveredRes.Content.ReadFromJsonAsync<ShipmentResponse>();

        deliveredShipment!.Status.Should().Be(ShipmentStatus.Delivered);
        deliveredShipment.Milestones.Should().HaveCount(3);
    }

    [Fact]
    public async Task E2E_6_TrackingLookup_And_CustomerAccessControl()
    {
        // 1. Non-admin cannot create shipment
        var customerToken = GenerateJwt("Customer");
        var unauthReq = new HttpRequestMessage(HttpMethod.Post, "/api/v1/shipping/shipments")
        {
            Content = JsonContent.Create(new CreateShipmentRequest(
                Guid.NewGuid(), "DHL", "Express", 1.0m, "50 Elm St", "02138", 1))
        };
        unauthReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", customerToken);
        var unauthRes = await _client.SendAsync(unauthReq);
        unauthRes.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        // 2. Admin creates shipment
        var adminToken = GenerateJwt("StoreAdmin");
        var authReq = new HttpRequestMessage(HttpMethod.Post, "/api/v1/shipping/shipments")
        {
            Content = JsonContent.Create(new CreateShipmentRequest(
                Guid.NewGuid(), "DHL", "Express", 1.0m, "50 Elm St", "02138", 1))
        };
        authReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var authRes = await _client.SendAsync(authReq);
        var created = await authRes.Content.ReadFromJsonAsync<ShipmentResponse>();

        // 3. Anyone can query tracking status without auth
        var trackingRes = await _client.GetAsync($"/api/v1/shipping/shipments/{created!.TrackingNumber}");
        trackingRes.StatusCode.Should().Be(HttpStatusCode.OK);
        var queried = await trackingRes.Content.ReadFromJsonAsync<ShipmentResponse>();
        queried!.TrackingNumber.Should().Be(created.TrackingNumber);
    }
}
