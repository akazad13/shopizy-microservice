using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Shopizy.CartService.Application.Common.Interfaces;
using Shopizy.CartService.Application.DTOs;
using Shopizy.CartService.Infrastructure.Catalog;

namespace Shopizy.CartService.E2ETests;

/// <summary>
/// E2E scenarios running against a full WebApplicationFactory host.
/// Replaces Redis with in-memory cache and injects controllable StubCatalogPriceService.
/// </summary>
public sealed class CartE2ETests : IClassFixture<CartWebFactory>
{
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };
    private readonly CartWebFactory _factory;

    public CartE2ETests(CartWebFactory factory) => _factory = factory;

    // ─── Scenario E2E-01: Guest Cart Lifecycle & Quantity Updates ─────────────

    [Fact]
    public async Task E2E01_GuestCartLifecycle_AddUpdateRemove()
    {
        var client = _factory.CreateClient();
        var guestId = Guid.NewGuid().ToString("N");

        // Step 1: Add Variant A (qty 2, $40)
        var variantAId = Guid.NewGuid();
        var addReq = BuildAddRequest(Guid.NewGuid(), variantAId, "Headphones", "SKU-001", 2, 40m);
        var addResp = await client.PostAsJsonAsync("/api/v1/cart/items",
            addReq, header: (GuestHeader, guestId));
        addResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var cart = await ReadCart(addResp);
        cart.Subtotal.Amount.Should().Be(80m);

        // Step 2: Update qty to 3 ($120)
        var updResp = await client.PutAsJsonAsync($"/api/v1/cart/items/{variantAId}",
            new UpdateCartItemRequest(3), header: (GuestHeader, guestId));
        updResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await ReadCart(updResp);
        updated.Subtotal.Amount.Should().Be(120m);

        // Step 3: Get cart
        var getResp = await client.GetAsync("/api/v1/cart", header: (GuestHeader, guestId));
        getResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var fetched = await ReadCart(getResp);
        fetched.Items.Should().HaveCount(1);
        fetched.TotalItemsCount.Should().Be(3);

        // Step 4: Remove the item
        var delResp = await client.DeleteAsync($"/api/v1/cart/items/{variantAId}",
            header: (GuestHeader, guestId));
        delResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var empty = await ReadCart(delResp);
        empty.Items.Should().BeEmpty();
        empty.Subtotal.Amount.Should().Be(0m);
    }

    // ─── Scenario E2E-02: Guest Cart Merging ─────────────────────────────────

    [Fact]
    public async Task E2E02_GuestCartMerge_IntoCustomerCart()
    {
        var client = _factory.CreateClient();
        var guestId = Guid.NewGuid().ToString("N");
        var customerId = Guid.NewGuid();

        // Step 1: Guest adds Variant A
        var variantAId = Guid.NewGuid();
        var addReq = BuildAddRequest(Guid.NewGuid(), variantAId, "Widget", "WGT-01", 2, 15m);
        await client.PostAsJsonAsync("/api/v1/cart/items", addReq, header: (GuestHeader, guestId));

        // Step 2: Merge guest cart into customer
        var token = BuildJwt(customerId);
        var mergeResp = await client.PostAsJsonAsync("/api/v1/cart/merge",
            new MergeCartRequest(guestId), bearerToken: token);
        mergeResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var merged = await ReadCart(mergeResp);
        merged.Items.Should().HaveCount(1);
        merged.Items[0].VariantId.Should().Be(variantAId);
        merged.Items[0].Quantity.Should().Be(2);

        // Step 3: Guest cart should now be empty
        var guestGet = await client.GetAsync("/api/v1/cart", header: (GuestHeader, guestId));
        var guestCart = await ReadCart(guestGet);
        guestCart.Items.Should().BeEmpty();
    }

    // ─── Scenario E2E-03: Price Discrepancy Detection ─────────────────────────

    [Fact]
    public async Task E2E03_PriceDiscrepancy_AlertedOnGet()
    {
        var client = _factory.CreateClient();
        var guestId = Guid.NewGuid().ToString("N");
        var variantId = Guid.NewGuid();

        // Step 1: Add at $100
        var addReq = BuildAddRequest(Guid.NewGuid(), variantId, "Gadget", "GAD-01", 1, 100m);
        await client.PostAsJsonAsync("/api/v1/cart/items", addReq, header: (GuestHeader, guestId));

        // Step 2: Simulate catalog price change to $115
        _factory.PriceService.SetPrice(variantId, 115m);

        // Step 3: GET cart — expect discrepancy alert
        var getResp = await client.GetAsync("/api/v1/cart", header: (GuestHeader, guestId));
        var cart = await ReadCart(getResp);
        cart.HasAnyPriceDiscrepancy.Should().BeTrue();
        var item = cart.Items.First(i => i.VariantId == variantId);
        item.HasPriceChanged.Should().BeTrue();
        item.CurrentCatalogPrice!.Amount.Should().Be(115m);
        item.PriceDifference.Should().Be(15m);
    }

    // ─── Scenario E2E-04: Multi-Tenant Isolation ──────────────────────────────

    [Fact]
    public async Task E2E04_MultiTenantIsolation_CustomerACannotSeeCustomerB()
    {
        var client = _factory.CreateClient();
        var customerA = Guid.NewGuid();
        var customerB = Guid.NewGuid();

        // Customer A adds item
        var tokenA = BuildJwt(customerA);
        var variantId = Guid.NewGuid();
        var addReq = BuildAddRequest(Guid.NewGuid(), variantId, "Secret Item", "SEC-01", 1, 999m);
        await client.PostAsJsonAsync("/api/v1/cart/items", addReq, bearerToken: tokenA);

        // Customer B gets their own cart (isolated, should be empty)
        var tokenB = BuildJwt(customerB);
        var getResp = await client.GetAsync("/api/v1/cart", bearerToken: tokenB);
        getResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var cartB = await ReadCart(getResp);
        cartB.Items.Should().BeEmpty();
        cartB.CustomerId.Should().Be(customerB);
    }

    // ─── Scenario E2E-05: Idempotency Key Protection ─────────────────────────

    [Fact]
    public async Task E2E05_IdempotencyKey_PreventsDuplicateItems()
    {
        var client = _factory.CreateClient();
        var guestId = Guid.NewGuid().ToString("N");
        var idempotencyKey = Guid.NewGuid().ToString();
        var variantId = Guid.NewGuid();
        var addReq = BuildAddRequest(Guid.NewGuid(), variantId, "Gadget", "G-01", 1, 50m);

        // First request
        var resp1 = await client.PostAsJsonAsync("/api/v1/cart/items", addReq,
            header: (GuestHeader, guestId), idempotencyKey: idempotencyKey);
        resp1.StatusCode.Should().Be(HttpStatusCode.OK);

        // Retry with same key — must return cached response, not re-add
        var resp2 = await client.PostAsJsonAsync("/api/v1/cart/items", addReq,
            header: (GuestHeader, guestId), idempotencyKey: idempotencyKey);
        resp2.StatusCode.Should().Be(HttpStatusCode.OK);

        // Cart should only contain qty=1 (not 2)
        var getResp = await client.GetAsync("/api/v1/cart", header: (GuestHeader, guestId));
        var cart = await ReadCart(getResp);
        cart.TotalItemsCount.Should().Be(1);
    }

    // ─── Scenario E2E-06: Cart Clear ─────────────────────────────────────────

    [Fact]
    public async Task E2E06_CartClear_EmptiesCartCompletely()
    {
        var client = _factory.CreateClient();
        var guestId = Guid.NewGuid().ToString("N");

        // Add items
        await client.PostAsJsonAsync("/api/v1/cart/items",
            BuildAddRequest(Guid.NewGuid(), Guid.NewGuid(), "A", "SKU-A", 2, 10m),
            header: (GuestHeader, guestId));

        // Clear
        var delResp = await client.DeleteAsync("/api/v1/cart", header: (GuestHeader, guestId));
        delResp.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // GET should return empty cart
        var getResp = await client.GetAsync("/api/v1/cart", header: (GuestHeader, guestId));
        var cart = await ReadCart(getResp);
        cart.Items.Should().BeEmpty();
        cart.Subtotal.Amount.Should().Be(0m);
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private const string GuestHeader = "X-Guest-Cart-Id";

    private static AddToCartRequest BuildAddRequest(
        Guid productId, Guid variantId, string name, string sku, int qty, decimal price) =>
        new(productId, variantId, name, sku, null, qty, new MoneyDto(price, "USD"));

    private static string BuildJwt(Guid customerId)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("shopizy-cart-dev-secret-key-32ch!"));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            claims: [new Claim(ClaimTypes.NameIdentifier, customerId.ToString())],
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static async Task<CartResponse> ReadCart(HttpResponseMessage resp)
    {
        var json = await resp.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<CartResponse>(json, JsonOpts)!;
    }
}

// ─── Extension helpers for HttpClient ────────────────────────────────────────

file static class HttpClientExtensions
{
    public static async Task<HttpResponseMessage> PostAsJsonAsync<T>(
        this HttpClient client, string uri, T body,
        (string key, string value)? header = null,
        string? bearerToken = null,
        string? idempotencyKey = null)
    {
        var req = new HttpRequestMessage(HttpMethod.Post, uri);
        req.Content = JsonContent.Create(body);
        if (header.HasValue) req.Headers.Add(header.Value.key, header.Value.value);
        if (bearerToken is not null) req.Headers.Authorization = new("Bearer", bearerToken);
        if (idempotencyKey is not null) req.Headers.Add("Idempotency-Key", idempotencyKey);
        return await client.SendAsync(req);
    }

    public static async Task<HttpResponseMessage> PutAsJsonAsync<T>(
        this HttpClient client, string uri, T body,
        (string key, string value)? header = null,
        string? bearerToken = null)
    {
        var req = new HttpRequestMessage(HttpMethod.Put, uri);
        req.Content = JsonContent.Create(body);
        if (header.HasValue) req.Headers.Add(header.Value.key, header.Value.value);
        if (bearerToken is not null) req.Headers.Authorization = new("Bearer", bearerToken);
        return await client.SendAsync(req);
    }

    public static async Task<HttpResponseMessage> DeleteAsync(
        this HttpClient client, string uri,
        (string key, string value)? header = null,
        string? bearerToken = null)
    {
        var req = new HttpRequestMessage(HttpMethod.Delete, uri);
        if (header.HasValue) req.Headers.Add(header.Value.key, header.Value.value);
        if (bearerToken is not null) req.Headers.Authorization = new("Bearer", bearerToken);
        return await client.SendAsync(req);
    }

    public static async Task<HttpResponseMessage> GetAsync(
        this HttpClient client, string uri,
        (string key, string value)? header = null,
        string? bearerToken = null)
    {
        var req = new HttpRequestMessage(HttpMethod.Get, uri);
        if (header.HasValue) req.Headers.Add(header.Value.key, header.Value.value);
        if (bearerToken is not null) req.Headers.Authorization = new("Bearer", bearerToken);
        return await client.SendAsync(req);
    }
}

// ─── Test WebApplicationFactory ──────────────────────────────────────────────

public sealed class CartWebFactory : WebApplicationFactory<Program>
{
    public StubCatalogPriceService PriceService { get; } = new();

    protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Replace Redis with in-memory cache
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IDistributedCache));
            if (descriptor is not null) services.Remove(descriptor);
            services.AddSingleton<IDistributedCache>(_ =>
            {
                var opts = Options.Create(new MemoryDistributedCacheOptions());
                return new MemoryDistributedCache(opts);
            });

            // Replace catalog price service with our controllable stub
            var catalogDescriptors = services.Where(d =>
                d.ServiceType == typeof(ICatalogPriceService) ||
                d.ServiceType == typeof(StubCatalogPriceService)).ToList();
            foreach (var d in catalogDescriptors) services.Remove(d);

            services.AddSingleton(PriceService);
            services.AddSingleton<ICatalogPriceService>(PriceService);
        });
    }
}

