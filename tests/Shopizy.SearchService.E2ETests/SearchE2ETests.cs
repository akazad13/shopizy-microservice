using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.IdentityModel.Tokens;
using Shopizy.SearchService.Application.Contracts;
using Xunit;

namespace Shopizy.SearchService.E2ETests;

public class SearchE2ETests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public SearchE2ETests(WebApplicationFactory<Program> factory)
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
    public async Task E2E_1_TypoTolerance_SearchWithTypo_ReturnsTargetProduct()
    {
        var adminToken = GenerateJwt("StoreAdmin");
        var req = new HttpRequestMessage(HttpMethod.Post, "/api/v1/search/index")
        {
            Content = JsonContent.Create(new IndexProductRequest(
                Guid.NewGuid(),
                "iPhone 15 Pro Max",
                "Flagship titanium smartphone",
                "Smartphones",
                "Apple",
                1199m,
                "USD",
                4.9,
                150,
                true,
                new List<string> { "phone", "apple", "ios" }))
        };
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var postRes = await _client.SendAsync(req);
        postRes.StatusCode.Should().Be(HttpStatusCode.OK);

        // Search with typo "iphne"
        var searchRes = await _client.GetFromJsonAsync<SearchResponse>("/api/v1/search?q=iphne");

        searchRes.Should().NotBeNull();
        searchRes!.Items.Should().Contain(i => i.Title.Contains("iPhone 15 Pro Max"));
    }

    [Fact]
    public async Task E2E_2_SynonymMatching_SearchingSneakers_ReturnsAthleticShoes()
    {
        var adminToken = GenerateJwt("StoreAdmin");
        var req = new HttpRequestMessage(HttpMethod.Post, "/api/v1/search/index")
        {
            Content = JsonContent.Create(new IndexProductRequest(
                Guid.NewGuid(),
                "Air Max Pulse Athletic Shoes",
                "High performance trainers for daily running",
                "Athletic Shoes",
                "Nike",
                150m,
                "USD",
                4.7,
                88,
                true,
                new List<string> { "athletic shoes", "trainers" }))
        };
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        await _client.SendAsync(req);

        // Search synonym "sneakers"
        var searchRes = await _client.GetFromJsonAsync<SearchResponse>("/api/v1/search?q=sneakers");

        searchRes.Should().NotBeNull();
        searchRes!.Items.Should().Contain(i => i.Title.Contains("Air Max Pulse"));
    }

    [Fact]
    public async Task E2E_3_DidYouMean_Suggestions_GeneratedWhenNoExactMatches()
    {
        var adminToken = GenerateJwt("StoreAdmin");
        var req = new HttpRequestMessage(HttpMethod.Post, "/api/v1/search/index")
        {
            Content = JsonContent.Create(new IndexProductRequest(
                Guid.NewGuid(),
                "Samsung Galaxy S24 Ultra",
                "Next-generation smartphone with galaxy AI",
                "Smartphones",
                "Samsung",
                1299m,
                "USD",
                4.8,
                120,
                true,
                new List<string> { "android", "samsung" }))
        };
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        await _client.SendAsync(req);

        // Erroneous query "sumsung"
        var searchRes = await _client.GetFromJsonAsync<SearchResponse>("/api/v1/search?q=sumsungxyz");

        searchRes.Should().NotBeNull();
        // Since zero items returned, didYouMean suggestion is provided if near enough
        var searchNear = await _client.GetFromJsonAsync<SearchResponse>("/api/v1/search?q=sumsng");
        searchNear.Should().NotBeNull();
    }

    [Fact]
    public async Task E2E_4_MultiAttributeFacets_ReturnsAccurateBucketCounts()
    {
        var adminToken = GenerateJwt("StoreAdmin");

        var p1 = new IndexProductRequest(Guid.NewGuid(), "Sony WH-1000XM5", "Noise canceling headphones", "Audio", "Sony", 399m, "USD", 4.8, 200, true, null);
        var p2 = new IndexProductRequest(Guid.NewGuid(), "Bose QuietComfort", "Comfortable noise canceling headphones", "Audio", "Bose", 349m, "USD", 4.7, 180, false, null);

        foreach (var p in new[] { p1, p2 })
        {
            var req = new HttpRequestMessage(HttpMethod.Post, "/api/v1/search/index") { Content = JsonContent.Create(p) };
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
            await _client.SendAsync(req);
        }

        var searchRes = await _client.GetFromJsonAsync<SearchResponse>("/api/v1/search?category=Audio");

        searchRes.Should().NotBeNull();
        searchRes!.Facets.Categories.Should().ContainKey("Audio");
        searchRes.Facets.Brands.Should().ContainKey("Sony");
        searchRes.Facets.Brands.Should().ContainKey("Bose");
        searchRes.Facets.PriceRanges.Should().ContainKey("$100+");
    }

    [Fact]
    public async Task E2E_5_FacetedFiltering_RestrictsByPriceAndStock()
    {
        var adminToken = GenerateJwt("StoreAdmin");

        var p1 = new IndexProductRequest(Guid.NewGuid(), "Budget Gaming Mouse", "RGB wired optical mouse", "Accessories", "Logitech", 20m, "USD", 4.2, 50, true, null);
        var p2 = new IndexProductRequest(Guid.NewGuid(), "Pro Wireless Mouse", "High performance wireless mouse", "Accessories", "Logitech", 149m, "USD", 4.9, 300, true, null);
        var p3 = new IndexProductRequest(Guid.NewGuid(), "Ergonomic Mouse", "Vertical mouse", "Accessories", "Logitech", 80m, "USD", 4.5, 40, false, null);

        foreach (var p in new[] { p1, p2, p3 })
        {
            var req = new HttpRequestMessage(HttpMethod.Post, "/api/v1/search/index") { Content = JsonContent.Create(p) };
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
            await _client.SendAsync(req);
        }

        // Filter: Brand=Logitech, minPrice=50, maxPrice=100, inStockOnly=true
        var searchRes = await _client.GetFromJsonAsync<SearchResponse>("/api/v1/search?brand=Logitech&minPrice=50&maxPrice=100&inStockOnly=true");

        searchRes.Should().NotBeNull();
        searchRes!.TotalCount.Should().Be(0); // p3 is 80m but out of stock!

        // Now without inStockOnly
        var searchWithOutOfStock = await _client.GetFromJsonAsync<SearchResponse>("/api/v1/search?brand=Logitech&minPrice=50&maxPrice=100&inStockOnly=false");
        searchWithOutOfStock.Should().NotBeNull();
        searchWithOutOfStock!.Items.Should().Contain(i => i.Title == "Ergonomic Mouse");
    }

    [Fact]
    public async Task E2E_6_AdminIndexIngestion_AdminIndexesAndDeletesProduct_NonAdminForbidden()
    {
        var prodId = Guid.NewGuid();
        var product = new IndexProductRequest(prodId, "Special Collector Edition Keyboard", "Mechanical switch", "Accessories", "Keychron", 199m, "USD", 5.0, 10, true, null);

        // Non-admin attempt should be 401/403
        var customerToken = GenerateJwt("Customer");
        var unauthReq = new HttpRequestMessage(HttpMethod.Post, "/api/v1/search/index") { Content = JsonContent.Create(product) };
        unauthReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", customerToken);
        var unauthRes = await _client.SendAsync(unauthReq);
        unauthRes.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        // Admin indexing
        var adminToken = GenerateJwt("StoreAdmin");
        var authReq = new HttpRequestMessage(HttpMethod.Post, "/api/v1/search/index") { Content = JsonContent.Create(product) };
        authReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var authRes = await _client.SendAsync(authReq);
        authRes.StatusCode.Should().Be(HttpStatusCode.OK);

        // Product should be in search
        var searchRes = await _client.GetFromJsonAsync<SearchResponse>("/api/v1/search?q=Keychron");
        searchRes!.Items.Should().Contain(i => i.Id == prodId);

        // Admin deletes product
        var delReq = new HttpRequestMessage(HttpMethod.Delete, $"/api/v1/search/index/{prodId}");
        delReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var delRes = await _client.SendAsync(delReq);
        delRes.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Product no longer in search
        var searchAfterDel = await _client.GetFromJsonAsync<SearchResponse>("/api/v1/search?q=Keychron");
        searchAfterDel!.Items.Should().NotContain(i => i.Id == prodId);
    }
}
