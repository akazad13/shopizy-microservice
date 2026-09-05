using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.IdentityModel.Tokens;
using Shopizy.PromotionService.Application.Contracts;
using Shopizy.PromotionService.Domain.Enums;
using Xunit;

namespace Shopizy.PromotionService.E2ETests;

public class PromotionE2ETests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public PromotionE2ETests(WebApplicationFactory<Program> factory)
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
    public async Task E2E_1_PercentageDiscountWithSafetyCap_EnforcesCeiling()
    {
        var adminToken = GenerateJwt("StoreAdmin");
        var campaignReq = new CreateCampaignRequest(
            "SAVE20", "20% off with $50 cap", DiscountType.Percentage, 20m, null, 50m, null, 100,
            DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(10));

        var req = new HttpRequestMessage(HttpMethod.Post, "/api/v1/promotions/campaigns") { Content = JsonContent.Create(campaignReq) };
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var postRes = await _client.SendAsync(req);
        postRes.StatusCode.Should().Be(HttpStatusCode.Created);

        var applyReq = new ApplyPromotionRequest("SAVE20", 300m, "USD", new List<CartItemDto>
        {
            new(Guid.NewGuid(), "Laptop Stand", "General", 300m, 1)
        });

        var applyRes = await _client.PostAsJsonAsync("/api/v1/promotions/apply", applyReq);
        applyRes.StatusCode.Should().Be(HttpStatusCode.OK);
        var eval = await applyRes.Content.ReadFromJsonAsync<PromotionEvaluationResult>();

        eval.Should().NotBeNull();
        eval!.IsValid.Should().BeTrue();
        eval.DiscountAmount.Should().Be(50m);
    }

    [Fact]
    public async Task E2E_2_FixedDiscountWithMinimumSpend_CalculatesCorrectly()
    {
        var adminToken = GenerateJwt("StoreAdmin");
        var campaignReq = new CreateCampaignRequest(
            "FLAT15", "$15 off on $50 min spend", DiscountType.FixedAmount, 15m, 50m, null, null, 100,
            DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(10));

        var req = new HttpRequestMessage(HttpMethod.Post, "/api/v1/promotions/campaigns") { Content = JsonContent.Create(campaignReq) };
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        await _client.SendAsync(req);

        var applyReq = new ApplyPromotionRequest("FLAT15", 80m, "USD", new List<CartItemDto>
        {
            new(Guid.NewGuid(), "Sneakers", "Footwear", 80m, 1)
        });

        var applyRes = await _client.PostAsJsonAsync("/api/v1/promotions/apply", applyReq);
        var eval = await applyRes.Content.ReadFromJsonAsync<PromotionEvaluationResult>();

        eval!.IsValid.Should().BeTrue();
        eval.DiscountAmount.Should().Be(15m);
    }

    [Fact]
    public async Task E2E_3_MinimumSpendRejection_FailsWhenSubtotalInsufficient()
    {
        var adminToken = GenerateJwt("StoreAdmin");
        var campaignReq = new CreateCampaignRequest(
            "TIER100", "$20 off on $100 min spend", DiscountType.FixedAmount, 20m, 100m, null, null, 100,
            DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(10));

        var req = new HttpRequestMessage(HttpMethod.Post, "/api/v1/promotions/campaigns") { Content = JsonContent.Create(campaignReq) };
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        await _client.SendAsync(req);

        var applyReq = new ApplyPromotionRequest("TIER100", 75m, "USD", new List<CartItemDto>
        {
            new(Guid.NewGuid(), "T-shirt", "Apparel", 75m, 1)
        });

        var applyRes = await _client.PostAsJsonAsync("/api/v1/promotions/apply", applyReq);
        var eval = await applyRes.Content.ReadFromJsonAsync<PromotionEvaluationResult>();

        eval!.IsValid.Should().BeFalse();
        eval.FailureReason.Should().Contain("Minimum subtotal");
    }

    [Fact]
    public async Task E2E_4_CategoryRestriction_OnlyDiscountsMatchingCategory()
    {
        var adminToken = GenerateJwt("StoreAdmin");
        var campaignReq = new CreateCampaignRequest(
            "SHOES30", "30% off footwear only", DiscountType.Percentage, 30m, null, null, "Footwear", 100,
            DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(10));

        var req = new HttpRequestMessage(HttpMethod.Post, "/api/v1/promotions/campaigns") { Content = JsonContent.Create(campaignReq) };
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        await _client.SendAsync(req);

        // Mixed cart: $100 Footwear and $100 Electronics
        var applyReq = new ApplyPromotionRequest("SHOES30", 200m, "USD", new List<CartItemDto>
        {
            new(Guid.NewGuid(), "Running Shoes", "Footwear", 100m, 1),
            new(Guid.NewGuid(), "Earbuds", "Electronics", 100m, 1)
        });

        var applyRes = await _client.PostAsJsonAsync("/api/v1/promotions/apply", applyReq);
        var eval = await applyRes.Content.ReadFromJsonAsync<PromotionEvaluationResult>();

        eval!.IsValid.Should().BeTrue();
        eval.DiscountAmount.Should().Be(30m); // 30% on $100 footwear, ignores electronics!
    }

    [Fact]
    public async Task E2E_5_BogoOffer_DiscountsLowestItemInTrio()
    {
        var adminToken = GenerateJwt("StoreAdmin");
        var campaignReq = new CreateCampaignRequest(
            "BOGO2026", "Buy 2 Get 1 Free", DiscountType.Bogo, 1m, null, null, null, 100,
            DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(10));

        var req = new HttpRequestMessage(HttpMethod.Post, "/api/v1/promotions/campaigns") { Content = JsonContent.Create(campaignReq) };
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        await _client.SendAsync(req);

        var applyReq = new ApplyPromotionRequest("BOGO2026", 110m, "USD", new List<CartItemDto>
        {
            new(Guid.NewGuid(), "Shirt A", "Apparel", 50m, 1),
            new(Guid.NewGuid(), "Shirt B", "Apparel", 40m, 1),
            new(Guid.NewGuid(), "Shirt C", "Apparel", 20m, 1)
        });

        var applyRes = await _client.PostAsJsonAsync("/api/v1/promotions/apply", applyReq);
        var eval = await applyRes.Content.ReadFromJsonAsync<PromotionEvaluationResult>();

        eval!.IsValid.Should().BeTrue();
        eval.DiscountAmount.Should().Be(20m); // Shirt C is lowest!
    }

    [Fact]
    public async Task E2E_6_AdminCampaignManagement_And_CustomerAccessControl()
    {
        var campaignReq = new CreateCampaignRequest(
            "ADMINEXCLUSIVE", "Admin test", DiscountType.FixedAmount, 10m, null, null, null, 1,
            DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(5));

        // Customer cannot create campaign
        var customerToken = GenerateJwt("Customer");
        var unauthReq = new HttpRequestMessage(HttpMethod.Post, "/api/v1/promotions/campaigns") { Content = JsonContent.Create(campaignReq) };
        unauthReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", customerToken);
        var unauthRes = await _client.SendAsync(unauthReq);
        unauthRes.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        // StoreAdmin creates campaign
        var adminToken = GenerateJwt("StoreAdmin");
        var authReq = new HttpRequestMessage(HttpMethod.Post, "/api/v1/promotions/campaigns") { Content = JsonContent.Create(campaignReq) };
        authReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var authRes = await _client.SendAsync(authReq);
        authRes.StatusCode.Should().Be(HttpStatusCode.Created);

        // Record coupon usage
        var useRes = await _client.PostAsync("/api/v1/promotions/ADMINEXCLUSIVE/use", null);
        useRes.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
