using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.IdentityModel.Tokens;
using Shopizy.ReviewService.Application.Contracts;
using Xunit;

namespace Shopizy.ReviewService.E2ETests;

public class ReviewE2ETests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private static readonly Guid _customer1Id = Guid.NewGuid();
    private static readonly Guid _customer2Id = Guid.NewGuid();
    private static readonly Guid _testProductId = Guid.NewGuid();

    public ReviewE2ETests(WebApplicationFactory<Program> factory)
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

    // E2E-01: Verified buyer review submission -> 201 Created with IsVerifiedBuyer = true
    [Fact]
    public async Task E2E_1_VerifiedBuyer_SubmitsReview_Returns201AndVerifiedBadge()
    {
        var req = new HttpRequestMessage(HttpMethod.Post, "/api/v1/reviews")
        {
            Content = JsonContent.Create(new CreateReviewRequest(
                _testProductId,
                5,
                "Outstanding Quality!",
                "Exceeded my expectations in every way.",
                new List<string> { "https://cdn.shopizy.com/img1.jpg" },
                VerifiedOrderId: Guid.NewGuid()))
        };
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", GenerateJwt("Customer", _customer1Id, "Alice"));

        var response = await _client.SendAsync(req);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<ReviewResponse>();
        result.Should().NotBeNull();
        result!.IsVerifiedBuyer.Should().BeTrue();
        result.Rating.Should().Be(5);
        result.CustomerName.Should().Be("Alice");
    }

    // E2E-02: Non-verified review submission -> 201 Created with IsVerifiedBuyer = false
    [Fact]
    public async Task E2E_2_UnverifiedBuyer_SubmitsReview_Returns201AndNoVerifiedBadge()
    {
        var req = new HttpRequestMessage(HttpMethod.Post, "/api/v1/reviews")
        {
            Content = JsonContent.Create(new CreateReviewRequest(
                _testProductId,
                4,
                "Good Value",
                "Looks promising on first inspection.",
                null,
                VerifiedOrderId: null))
        };
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", GenerateJwt("Customer", _customer2Id, "Bob"));

        var response = await _client.SendAsync(req);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<ReviewResponse>();
        result.Should().NotBeNull();
        result!.IsVerifiedBuyer.Should().BeFalse();
        result.Rating.Should().Be(4);
    }

    // E2E-03: Aggregate product summary endpoint calculates correct average and total
    [Fact]
    public async Task E2E_3_GetProductSummary_ReturnsAccurateAggregateMetrics()
    {
        var isolatedProductId = Guid.NewGuid();

        // Seed 2 reviews for this isolated product
        var req1 = new HttpRequestMessage(HttpMethod.Post, "/api/v1/reviews")
        {
            Content = JsonContent.Create(new CreateReviewRequest(
                isolatedProductId, 5, "Great", "Loved it", null))
        };
        req1.Headers.Authorization = new AuthenticationHeaderValue("Bearer", GenerateJwt("Customer", Guid.NewGuid()));
        await _client.SendAsync(req1);

        var req2 = new HttpRequestMessage(HttpMethod.Post, "/api/v1/reviews")
        {
            Content = JsonContent.Create(new CreateReviewRequest(
                isolatedProductId, 4, "Good", "Decent", null))
        };
        req2.Headers.Authorization = new AuthenticationHeaderValue("Bearer", GenerateJwt("Customer", Guid.NewGuid()));
        await _client.SendAsync(req2);

        var response = await _client.GetAsync($"/api/v1/reviews/product/{isolatedProductId}/summary");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var summary = await response.Content.ReadFromJsonAsync<ProductReviewSummaryResponse>();
        summary.Should().NotBeNull();
        summary!.TotalReviews.Should().Be(2);
        summary.AverageRating.Should().Be(4.5m);
        summary.RatingDistribution.Should().ContainKey(5);
        summary.RatingDistribution.Should().ContainKey(4);
    }

    // E2E-04: Review helpfulness voting increments counter idempotently
    [Fact]
    public async Task E2E_4_Customer_VotesHelpfulOnReview_IncrementsHelpfulCount()
    {
        var voteProductId = Guid.NewGuid();

        // Create a review
        var createReq = new HttpRequestMessage(HttpMethod.Post, "/api/v1/reviews")
        {
            Content = JsonContent.Create(new CreateReviewRequest(
                voteProductId, 5, "Superb", "Highly recommended", null))
        };
        createReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", GenerateJwt("Customer", Guid.NewGuid()));
        var createRes = await _client.SendAsync(createReq);
        createRes.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createRes.Content.ReadFromJsonAsync<ReviewResponse>();
        var targetReviewId = created!.Id;

        // Vote helpful
        var voteReq = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/reviews/{targetReviewId}/vote")
        {
            Content = JsonContent.Create(new VoteReviewRequest(IsHelpful: true))
        };
        voteReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", GenerateJwt("Customer", Guid.NewGuid()));

        var response = await _client.SendAsync(voteReq);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var voteSummary = await response.Content.ReadFromJsonAsync<ReviewVoteSummaryResponse>();
        voteSummary.Should().NotBeNull();
        voteSummary!.HelpfulVotes.Should().Be(1);
    }

    // E2E-05: Wishlist CRUD and customer isolation (Principle V)
    [Fact]
    public async Task E2E_5_Customer_ManagesWishlist_AndOtherCustomerForbidden()
    {
        var custAId = Guid.NewGuid();
        var custBId = Guid.NewGuid();

        // Customer A adds item to wishlist
        var addReq = new HttpRequestMessage(HttpMethod.Post, "/api/v1/wishlists/items")
        {
            Content = JsonContent.Create(new AddWishlistItemRequest(
                _testProductId,
                "Wireless Noise Cancelling Headphones",
                "AUDIO-NC-001",
                199.99m))
        };
        addReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", GenerateJwt("Customer", custAId));

        var addResp = await _client.SendAsync(addReq);
        addResp.StatusCode.Should().Be(HttpStatusCode.Created);

        // Customer A queries own wishlist
        var myReq = new HttpRequestMessage(HttpMethod.Get, "/api/v1/wishlists/my");
        myReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", GenerateJwt("Customer", custAId));
        var myResp = await _client.SendAsync(myReq);
        myResp.StatusCode.Should().Be(HttpStatusCode.OK);

        var wishlist = await myResp.Content.ReadFromJsonAsync<WishlistResponse>();
        wishlist.Should().NotBeNull();
        wishlist!.Items.Should().ContainSingle(i => i.ProductId == _testProductId);

        // Customer B attempts to inspect Customer A's wishlist -> 403 Forbidden
        var breachReq = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/wishlists/user/{custAId}");
        breachReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", GenerateJwt("Customer", custBId));
        var breachResp = await _client.SendAsync(breachReq);
        breachResp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // E2E-06: Unauthenticated access to review creation or wishlist is rejected (401)
    [Fact]
    public async Task E2E_6_UnauthenticatedRequest_Returns401()
    {
        var request = new CreateReviewRequest(
            _testProductId,
            5,
            "Anonymous Review",
            "Should not be permitted.",
            null);

        var reviewResp = await _client.PostAsJsonAsync("/api/v1/reviews", request);
        reviewResp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var wishlistResp = await _client.GetAsync("/api/v1/wishlists/my");
        wishlistResp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
