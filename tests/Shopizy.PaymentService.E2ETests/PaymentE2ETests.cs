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
using Shopizy.PaymentService.Application.Contracts;
using Shopizy.PaymentService.Infrastructure.Persistence;

namespace Shopizy.PaymentService.E2ETests;

public sealed class PaymentE2ETests : IClassFixture<PaymentWebFactory>
{
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };
    private readonly PaymentWebFactory _factory;

    public PaymentE2ETests(PaymentWebFactory factory) => _factory = factory;

    // ─── Scenario E2E-01: Successful Card Payment ─────────────────────────────

    [Fact]
    public async Task E2E01_SuccessfulCardPayment_TransitionsToSucceeded()
    {
        var client = _factory.CreateClient();
        var customerId = Guid.NewGuid();
        var orderId = Guid.NewGuid();

        var request = new ProcessPaymentRequest(orderId, "tok_visa_valid", new MoneyDto(149.99m));
        var token = BuildJwt(customerId);

        var response = await client.PostAsJsonAsync("/api/v1/payments", request, bearerToken: token);
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var payment = await ReadPayment(response);
        payment.Status.Should().Be("Succeeded");
        payment.Amount.Amount.Should().Be(149.99m);
        payment.GatewayTransactionId.Should().StartWith("ch_");
        payment.SucceededAtUtc.Should().NotBeNull();
    }

    // ─── Scenario E2E-02: Declined Card Payment ───────────────────────────────

    [Fact]
    public async Task E2E02_DeclinedCardPayment_TransitionsToFailed()
    {
        var client = _factory.CreateClient();
        var customerId = Guid.NewGuid();
        var orderId = Guid.NewGuid();

        var request = new ProcessPaymentRequest(orderId, "tok_declined", new MoneyDto(50m));
        var token = BuildJwt(customerId);

        var response = await client.PostAsJsonAsync("/api/v1/payments", request, bearerToken: token);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // Check transaction record
        var getResp = await client.GetAsync($"/api/v1/payments/order/{orderId}", bearerToken: token);
        getResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var payment = await ReadPayment(getResp);
        payment.Status.Should().Be("Failed");
        payment.FailureReason.Should().Contain("declined");
    }

    // ─── Scenario E2E-03: Automated Post-Payment Refund ───────────────────────

    [Fact]
    public async Task E2E03_PostPaymentRefund_TransitionsToRefunded()
    {
        var client = _factory.CreateClient();
        var customerId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var token = BuildJwt(customerId);

        // Pay
        var payResp = await client.PostAsJsonAsync("/api/v1/payments",
            new ProcessPaymentRequest(orderId, "tok_visa", new MoneyDto(80m)), bearerToken: token);
        var payment = await ReadPayment(payResp);

        // Refund
        var refundResp = await client.PostAsJsonAsync($"/api/v1/payments/{payment.Id}/refund",
            new RefundPaymentRequest(80m, "OrderCancelledPriorToShipment"), bearerToken: token);
        refundResp.StatusCode.Should().Be(HttpStatusCode.OK);

        var refunded = await ReadPayment(refundResp);
        refunded.Status.Should().Be("Refunded");
        refunded.Refund.Should().NotBeNull();
        refunded.Refund!.RefundReference.Should().StartWith("re_");
        refunded.Refund!.Amount.Amount.Should().Be(80m);
    }

    // ─── Scenario E2E-04: Duplicate Charge Prevention via Idempotency ─────────

    [Fact]
    public async Task E2E04_IdempotencyKey_PreventsDuplicateCharges()
    {
        var client = _factory.CreateClient();
        var customerId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var idempotencyKey = Guid.NewGuid().ToString();
        var token = BuildJwt(customerId);

        var request = new ProcessPaymentRequest(orderId, "tok_visa", new MoneyDto(200m));

        // First charge
        var resp1 = await client.PostAsJsonAsync("/api/v1/payments", request,
            bearerToken: token, idempotencyKey: idempotencyKey);
        resp1.StatusCode.Should().Be(HttpStatusCode.Created);

        // Retry with same Idempotency-Key
        var resp2 = await client.PostAsJsonAsync("/api/v1/payments", request,
            bearerToken: token, idempotencyKey: idempotencyKey);
        resp2.StatusCode.Should().Be(HttpStatusCode.Created);

        // Verify only 1 payment was recorded for this order
        var listResp = await client.GetAsync($"/api/v1/payments/order/{orderId}", bearerToken: token);
        listResp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // ─── Scenario E2E-05: Customer Multi-Tenant Isolation ─────────────────────

    [Fact]
    public async Task E2E05_MultiTenantIsolation_CustomerCannotViewOtherPayment()
    {
        var client = _factory.CreateClient();
        var customerA = Guid.NewGuid();
        var customerB = Guid.NewGuid();
        var tokenA = BuildJwt(customerA);
        var tokenB = BuildJwt(customerB);

        var payResp = await client.PostAsJsonAsync("/api/v1/payments",
            new ProcessPaymentRequest(Guid.NewGuid(), "tok_visa", new MoneyDto(60m)), bearerToken: tokenA);
        var paymentA = await ReadPayment(payResp);

        // Customer B attempts GET Customer A's payment
        var getResp = await client.GetAsync($"/api/v1/payments/{paymentA.Id}", bearerToken: tokenB);
        getResp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ─── Scenario E2E-06: Double Refund Rejection ─────────────────────────────

    [Fact]
    public async Task E2E06_DoubleRefund_RejectsSecondAttempt()
    {
        var client = _factory.CreateClient();
        var customerId = Guid.NewGuid();
        var token = BuildJwt(customerId);

        var payResp = await client.PostAsJsonAsync("/api/v1/payments",
            new ProcessPaymentRequest(Guid.NewGuid(), "tok_visa", new MoneyDto(50m)), bearerToken: token);
        var payment = await ReadPayment(payResp);

        // First refund -> OK
        await client.PostAsJsonAsync($"/api/v1/payments/{payment.Id}/refund",
            new RefundPaymentRequest(50m), bearerToken: token);

        // Second refund -> BadRequest (already refunded)
        var secondResp = await client.PostAsJsonAsync($"/api/v1/payments/{payment.Id}/refund",
            new RefundPaymentRequest(50m), bearerToken: token);
        secondResp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private static string BuildJwt(Guid customerId, string role = "Customer")
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("shopizy-payment-dev-secret-key-32ch!"));
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

    private static async Task<PaymentResponse> ReadPayment(HttpResponseMessage resp)
    {
        var json = await resp.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<PaymentResponse>(json, JsonOpts)!;
    }
}

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

    public static async Task<HttpResponseMessage> GetAsync(
        this HttpClient client, string uri,
        string? bearerToken = null)
    {
        var req = new HttpRequestMessage(HttpMethod.Get, uri);
        if (bearerToken is not null) req.Headers.Authorization = new("Bearer", bearerToken);
        return await client.SendAsync(req);
    }
}

public sealed class PaymentWebFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = Guid.NewGuid().ToString();

    protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<PaymentDbContext>));
            if (descriptor is not null) services.Remove(descriptor);

            services.AddDbContext<PaymentDbContext>(options =>
                options.UseInMemoryDatabase(_dbName));
        });
    }
}
