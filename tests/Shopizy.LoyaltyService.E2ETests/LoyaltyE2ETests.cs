using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.IdentityModel.Tokens;
using Shopizy.LoyaltyService.Application.Contracts;
using Shopizy.LoyaltyService.Domain.Enums;
using Xunit;

namespace Shopizy.LoyaltyService.E2ETests;

public class LoyaltyE2ETests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private static readonly Guid _customer1Id = Guid.NewGuid();
    private static readonly Guid _customer2Id = Guid.NewGuid();

    public LoyaltyE2ETests(WebApplicationFactory<Program> factory)
    {
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

    // E2E-01: Admin accrues points for customer order -> balance increases and ledger record created
    [Fact]
    public async Task E2E_1_Admin_AccruesPoints_UpdatesBalanceAndTransactions()
    {
        var req = new HttpRequestMessage(HttpMethod.Post, "/api/v1/loyalty/accrue")
        {
            Content = JsonContent.Create(new AccruePointsRequest(_customer1Id, Guid.NewGuid(), 250.75m))
        };
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", GenerateJwt("StoreAdmin"));

        var res = await _client.SendAsync(req);
        res.StatusCode.Should().Be(HttpStatusCode.OK);

        var account = await res.Content.ReadFromJsonAsync<LoyaltyAccountResponse>();
        account.Should().NotBeNull();
        account!.PointsBalance.Should().Be(250);
        account.CashEquivalentValue.Should().Be(2.50m);
        account.Transactions.Should().ContainSingle(t => t.Type == LoyaltyTransactionType.Accrual && t.Points == 250);
    }

    // E2E-02: Customer redeems points -> discount calculated and balance deducted
    [Fact]
    public async Task E2E_2_Customer_RedeemsPoints_ReturnsDiscountAndDecrementsBalance()
    {
        var customerId = Guid.NewGuid();

        // 1. Accrue 300 points
        var accrueReq = new HttpRequestMessage(HttpMethod.Post, "/api/v1/loyalty/accrue")
        {
            Content = JsonContent.Create(new AccruePointsRequest(customerId, Guid.NewGuid(), 300m))
        };
        accrueReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", GenerateJwt("StoreAdmin"));
        await _client.SendAsync(accrueReq);

        // 2. Customer redeems 100 points
        var redeemReq = new HttpRequestMessage(HttpMethod.Post, "/api/v1/loyalty/redeem")
        {
            Content = JsonContent.Create(new RedeemPointsRequest(100, Guid.NewGuid()))
        };
        redeemReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", GenerateJwt("Customer", customerId));

        var res = await _client.SendAsync(redeemReq);
        res.StatusCode.Should().Be(HttpStatusCode.OK);

        var response = await res.Content.ReadFromJsonAsync<PointsRedemptionResponse>();
        response.Should().NotBeNull();
        response!.PointsRedeemed.Should().Be(100);
        response.DiscountAmount.Should().Be(1.00m);
        response.RemainingPoints.Should().Be(200);
    }

    // E2E-03: Customer attempts over-redemption -> 400 Bad Request
    [Fact]
    public async Task E2E_3_Customer_OverRedeems_ReturnsBadRequest()
    {
        var customerId = Guid.NewGuid();

        var redeemReq = new HttpRequestMessage(HttpMethod.Post, "/api/v1/loyalty/redeem")
        {
            Content = JsonContent.Create(new RedeemPointsRequest(500, Guid.NewGuid()))
        };
        redeemReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", GenerateJwt("Customer", customerId));

        var res = await _client.SendAsync(redeemReq);
        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // E2E-04: Admin creates gift card -> balance check returns 200 with full initial balance
    [Fact]
    public async Task E2E_4_Admin_CreatesGiftCard_AndBalanceCheckSucceeds()
    {
        var createReq = new HttpRequestMessage(HttpMethod.Post, "/api/v1/gift-cards")
        {
            Content = JsonContent.Create(new CreateGiftCardRequest(100.00m, "USD"))
        };
        createReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", GenerateJwt("StoreAdmin"));

        var createRes = await _client.SendAsync(createReq);
        createRes.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createRes.Content.ReadFromJsonAsync<GiftCardResponse>();

        created.Should().NotBeNull();
        created!.CurrentBalance.Should().Be(100.00m);
        created.Status.Should().Be(GiftCardStatus.Active);

        // Public balance check
        var checkRes = await _client.GetAsync($"/api/v1/gift-cards/check/{created.Code}");
        checkRes.StatusCode.Should().Be(HttpStatusCode.OK);
        var balance = await checkRes.Content.ReadFromJsonAsync<GiftCardBalanceResponse>();
        balance.Should().NotBeNull();
        balance!.CurrentBalance.Should().Be(100.00m);
        balance.Status.Should().Be(GiftCardStatus.Active);
    }

    // E2E-05: Gift card partial & full deduction transitions status correctly
    [Fact]
    public async Task E2E_5_GiftCard_PartialAndFullDeduction_TransitionsStatus()
    {
        var createReq = new HttpRequestMessage(HttpMethod.Post, "/api/v1/gift-cards")
        {
            Content = JsonContent.Create(new CreateGiftCardRequest(50.00m, "USD"))
        };
        createReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", GenerateJwt("StoreAdmin"));
        var createRes = await _client.SendAsync(createReq);
        var card = await createRes.Content.ReadFromJsonAsync<GiftCardResponse>();

        // 1. Partial deduction ($20)
        var deduct1Req = new HttpRequestMessage(HttpMethod.Post, "/api/v1/gift-cards/apply")
        {
            Content = JsonContent.Create(new ApplyGiftCardRequest(card!.Code, 20.00m, Guid.NewGuid()))
        };
        deduct1Req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", GenerateJwt("Customer"));
        var res1 = await _client.SendAsync(deduct1Req);
        res1.StatusCode.Should().Be(HttpStatusCode.OK);
        var after1 = await res1.Content.ReadFromJsonAsync<GiftCardDeductionResponse>();
        after1!.RemainingBalance.Should().Be(30.00m);
        after1.Status.Should().Be(GiftCardStatus.Active);

        // 2. Full remaining deduction ($30)
        var deduct2Req = new HttpRequestMessage(HttpMethod.Post, "/api/v1/gift-cards/apply")
        {
            Content = JsonContent.Create(new ApplyGiftCardRequest(card.Code, 30.00m, Guid.NewGuid()))
        };
        deduct2Req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", GenerateJwt("Customer"));
        var res2 = await _client.SendAsync(deduct2Req);
        res2.StatusCode.Should().Be(HttpStatusCode.OK);
        var after2 = await res2.Content.ReadFromJsonAsync<GiftCardDeductionResponse>();
        after2!.RemainingBalance.Should().Be(0.00m);
        after2.Status.Should().Be(GiftCardStatus.Depleted);
    }

    // E2E-06: Customer Isolation & Auth Protection
    [Fact]
    public async Task E2E_6_CustomerIsolation_And_UnauthorizedProtection()
    {
        var userA = Guid.NewGuid();
        var userB = Guid.NewGuid();

        // 1. User A tries to inspect User B's account -> 403 Forbidden
        var breachReq = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/loyalty/account/{userB}");
        breachReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", GenerateJwt("Customer", userA));
        var breachRes = await _client.SendAsync(breachReq);
        breachRes.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        // 2. Unauthenticated access to /my -> 401 Unauthorized
        var unauthRes = await _client.GetAsync("/api/v1/loyalty/my");
        unauthRes.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
