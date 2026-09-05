using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Shopizy.OrderService.Application.Contracts;
using Shopizy.OrderService.Infrastructure.Persistence;

namespace Shopizy.OrderService.E2ETests;

public sealed class OrderE2ETests : IClassFixture<OrderWebFactory>
{
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };
    private readonly OrderWebFactory _factory;

    public OrderE2ETests(OrderWebFactory factory) => _factory = factory;

    // ─── Scenario E2E-01: Successful Order Checkout & Stock Reservation ───────

    [Fact]
    public async Task E2E01_SuccessfulCheckout_ReservesStockAndSetsPendingPayment()
    {
        var client = _factory.CreateClient();
        var customerId = Guid.NewGuid();
        var variantId = Guid.NewGuid();

        // Seed inventory: 10 units
        await SeedInventory(variantId, 10);

        var request = BuildOrderRequest(variantId, quantity: 2, price: 99.99m);
        var token = BuildJwt(customerId);

        var response = await client.PostAsJsonAsync("/api/v1/orders", request, bearerToken: token);
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var order = await ReadOrder(response);
        order.Status.Should().Be("PendingPayment");
        order.TotalAmount.Amount.Should().Be(199.98m);
        order.ExpiresAtUtc.Should().BeAfter(order.CreatedAtUtc);

        // Check inventory: 8 available, 2 reserved
        var invResp = await client.GetAsync($"/api/v1/inventory/{variantId}");
        var inv = await ReadInventory(invResp);
        inv.AvailableStock.Should().Be(8);
        inv.ReservedStock.Should().Be(2);
    }

    // ─── Scenario E2E-02: Zero-Overselling Stock Depletion Rejection ──────────

    [Fact]
    public async Task E2E02_ZeroOverselling_RejectsWhenStockInsufficient()
    {
        var client = _factory.CreateClient();
        var customerId = Guid.NewGuid();
        var variantId = Guid.NewGuid();

        // Seed inventory: only 1 unit
        await SeedInventory(variantId, 1);

        // Request 3 units
        var request = BuildOrderRequest(variantId, quantity: 3, price: 40m);
        var token = BuildJwt(customerId);

        var response = await client.PostAsJsonAsync("/api/v1/orders", request, bearerToken: token);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // Inventory must remain untouched: 1 available, 0 reserved
        var invResp = await client.GetAsync($"/api/v1/inventory/{variantId}");
        var inv = await ReadInventory(invResp);
        inv.AvailableStock.Should().Be(1);
        inv.ReservedStock.Should().Be(0);
    }

    // ─── Scenario E2E-03: 15-Minute Unpaid Expiration & Auto-Restock ──────────

    [Fact]
    public async Task E2E03_UnpaidExpiration_CancelsOrderAndRestocksInventory()
    {
        var client = _factory.CreateClient();
        var customerId = Guid.NewGuid();
        var variantId = Guid.NewGuid();

        await SeedInventory(variantId, 5);

        // Create order for 3 units
        var request = BuildOrderRequest(variantId, quantity: 3, price: 50m);
        var token = BuildJwt(customerId);
        var createResp = await client.PostAsJsonAsync("/api/v1/orders", request, bearerToken: token);
        var order = await ReadOrder(createResp);

        // Trigger expiration worker advancing clock 20 minutes past expiry
        var future = DateTimeOffset.UtcNow.AddMinutes(20);
        var expireResp = await client.PostAsync($"/api/v1/orders/{order.Id}/expire?asOf={Uri.EscapeDataString(future.ToString("o"))}", null);
        expireResp.StatusCode.Should().Be(HttpStatusCode.OK);

        var expiredOrder = await ReadOrder(expireResp);
        expiredOrder.Status.Should().Be("Cancelled");
        expiredOrder.CancellationReason.Should().Be("PaymentExpired");

        // Inventory must be fully released back: 5 available, 0 reserved
        var invResp = await client.GetAsync($"/api/v1/inventory/{variantId}");
        var inv = await ReadInventory(invResp);
        inv.AvailableStock.Should().Be(5);
        inv.ReservedStock.Should().Be(0);
    }

    // ─── Scenario E2E-04: Order Cancellation & Restocking Prior to Shipment ───

    [Fact]
    public async Task E2E04_OrderCancellation_RestocksStockPriorToShipment()
    {
        var client = _factory.CreateClient();
        var customerId = Guid.NewGuid();
        var variantId = Guid.NewGuid();

        await SeedInventory(variantId, 10);

        // Create order
        var request = BuildOrderRequest(variantId, quantity: 4, price: 25m);
        var token = BuildJwt(customerId);
        var createResp = await client.PostAsJsonAsync("/api/v1/orders", request, bearerToken: token);
        var order = await ReadOrder(createResp);

        // Pay order -> Processing
        var payResp = await client.PostAsync($"/api/v1/orders/{order.Id}/pay", null);
        payResp.StatusCode.Should().Be(HttpStatusCode.OK);

        // Customer cancels before shipping
        var cancelResp = await client.PostAsync($"/api/v1/orders/{order.Id}/cancel", null, bearerToken: token);
        cancelResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var cancelled = await ReadOrder(cancelResp);
        cancelled.Status.Should().Be("Cancelled");

        // Inventory is restocked back to 10
        var invResp = await client.GetAsync($"/api/v1/inventory/{variantId}");
        var inv = await ReadInventory(invResp);
        inv.AvailableStock.Should().Be(10);
    }

    // ─── Scenario E2E-05: Customer Multi-Tenant Isolation ─────────────────────

    [Fact]
    public async Task E2E05_MultiTenantIsolation_CustomerCannotViewOtherCustomerOrder()
    {
        var client = _factory.CreateClient();
        var customerA = Guid.NewGuid();
        var customerB = Guid.NewGuid();
        var variantId = Guid.NewGuid();

        await SeedInventory(variantId, 10);

        // Customer A creates order
        var tokenA = BuildJwt(customerA);
        var createResp = await client.PostAsJsonAsync("/api/v1/orders",
            BuildOrderRequest(variantId, 1, 30m), bearerToken: tokenA);
        var orderA = await ReadOrder(createResp);

        // Customer B attempts to get Customer A's order
        var tokenB = BuildJwt(customerB);
        var getResp = await client.GetAsync($"/api/v1/orders/{orderA.Id}", bearerToken: tokenB);
        getResp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ─── Scenario E2E-06: Idempotent Checkout Protection ─────────────────────

    [Fact]
    public async Task E2E06_IdempotencyKey_PreventsDuplicateOrderOrReservation()
    {
        var client = _factory.CreateClient();
        var customerId = Guid.NewGuid();
        var variantId = Guid.NewGuid();
        var idempotencyKey = Guid.NewGuid().ToString();

        await SeedInventory(variantId, 10);

        var request = BuildOrderRequest(variantId, quantity: 2, price: 50m);
        var token = BuildJwt(customerId);

        // First checkout request
        var resp1 = await client.PostAsJsonAsync("/api/v1/orders", request,
            bearerToken: token, idempotencyKey: idempotencyKey);
        resp1.StatusCode.Should().Be(HttpStatusCode.Created);

        // Retry with same Idempotency-Key
        var resp2 = await client.PostAsJsonAsync("/api/v1/orders", request,
            bearerToken: token, idempotencyKey: idempotencyKey);
        resp2.StatusCode.Should().Be(HttpStatusCode.Created);

        // Only 2 units reserved, not 4!
        var invResp = await client.GetAsync($"/api/v1/inventory/{variantId}");
        var inv = await ReadInventory(invResp);
        inv.AvailableStock.Should().Be(8);
        inv.ReservedStock.Should().Be(2);
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private async Task SeedInventory(Guid variantId, int count)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
        var existing = await db.Inventory.FindAsync(variantId);
        if (existing is not null)
        {
            db.Inventory.Remove(existing);
            await db.SaveChangesAsync();
        }
        db.Inventory.Add(new Shopizy.OrderService.Domain.Entities.InventoryItem(variantId, count));
        await db.SaveChangesAsync();
    }

    private static CreateOrderRequest BuildOrderRequest(Guid variantId, int quantity, decimal price) =>
        new(
            Items: [new CreateOrderItemDto(Guid.NewGuid(), variantId, "Product", "SKU-01", quantity, new MoneyDto(price))],
            ShippingAddress: new ShippingAddressDto("John Doe", "123 Main St", "Metropolis", "NY", "10001", "USA"));

    private static string BuildJwt(Guid customerId, string role = "Customer")
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("shopizy-order-dev-secret-key-32ch!"));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            claims:
            [
                new Claim(ClaimTypes.NameIdentifier, customerId.ToString()),
                new Claim(ClaimTypes.Role, role)
            ],
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static async Task<OrderResponse> ReadOrder(HttpResponseMessage resp)
    {
        var json = await resp.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<OrderResponse>(json, JsonOpts)!;
    }

    private static async Task<InventoryResponse> ReadInventory(HttpResponseMessage resp)
    {
        var json = await resp.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<InventoryResponse>(json, JsonOpts)!;
    }
}

// ─── HttpClient Helpers ───────────────────────────────────────────────────────

file static class HttpClientExtensions
{
    public static async Task<HttpResponseMessage> PostAsJsonAsync<T>(
        this HttpClient client, string uri, T body,
        string? bearerToken = null,
        string? idempotencyKey = null)
    {
        var req = new HttpRequestMessage(HttpMethod.Post, uri);
        req.Content = JsonContent.Create(body);
        if (bearerToken is not null) req.Headers.Authorization = new("Bearer", bearerToken);
        if (idempotencyKey is not null) req.Headers.Add("Idempotency-Key", idempotencyKey);
        return await client.SendAsync(req);
    }

    public static async Task<HttpResponseMessage> PostAsync(
        this HttpClient client, string uri, HttpContent? content,
        string? bearerToken = null)
    {
        var req = new HttpRequestMessage(HttpMethod.Post, uri) { Content = content };
        if (bearerToken is not null) req.Headers.Authorization = new("Bearer", bearerToken);
        return await client.SendAsync(req);
    }

    public static async Task<HttpResponseMessage> GetAsync(
        this HttpClient client, string uri,
        string? bearerToken = null)
    {
        var req = new HttpRequestMessage(HttpMethod.Get, uri);
        if (bearerToken is not null) req.Headers.Authorization = new("Bearer", bearerToken);
        return await client.SendAsync(req);
    }
}

// ─── Test Factory ─────────────────────────────────────────────────────────────

public sealed class OrderWebFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = Guid.NewGuid().ToString();

    protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<OrderDbContext>));
            if (descriptor is not null) services.Remove(descriptor);

            services.AddDbContext<OrderDbContext>(options =>
                options.UseInMemoryDatabase(_dbName));
        });
    }
}
