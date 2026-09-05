using Shopizy.PaymentService.Domain.ValueObjects;

namespace Shopizy.PaymentService.Application.Interfaces;

public sealed record GatewayChargeResult(bool IsSuccess, string? GatewayTransactionId, string? ErrorMessage);
public sealed record GatewayRefundResult(bool IsSuccess, string? RefundReference, string? ErrorMessage);

public interface IPaymentGatewayProvider
{
    Task<GatewayChargeResult> ChargeAsync(string token, Money amount, CancellationToken cancellationToken = default);
    Task<GatewayRefundResult> RefundAsync(string gatewayTransactionId, Money amount, string reason, CancellationToken cancellationToken = default);
}
