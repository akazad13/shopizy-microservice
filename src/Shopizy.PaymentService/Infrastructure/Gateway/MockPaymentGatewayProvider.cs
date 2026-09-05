using Shopizy.PaymentService.Application.Interfaces;
using Shopizy.PaymentService.Domain.ValueObjects;

namespace Shopizy.PaymentService.Infrastructure.Gateway;

/// <summary>
/// Mock payment gateway provider simulating Stripe / payment processor behavior.
/// Tokens ending in '_declined' or 'declined' simulate card rejections.
/// </summary>
public sealed class MockPaymentGatewayProvider : IPaymentGatewayProvider
{
    public Task<GatewayChargeResult> ChargeAsync(string token, Money amount, CancellationToken cancellationToken = default)
    {
        if (token.Contains("declined", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(new GatewayChargeResult(false, null, "Card was declined: Insufficient funds or invalid card details."));
        }

        var gwId = $"ch_{Guid.NewGuid():N}";
        return Task.FromResult(new GatewayChargeResult(true, gwId, null));
    }

    public Task<GatewayRefundResult> RefundAsync(string gatewayTransactionId, Money amount, string reason, CancellationToken cancellationToken = default)
    {
        var refId = $"re_{Guid.NewGuid():N}";
        return Task.FromResult(new GatewayRefundResult(true, refId, null));
    }
}
