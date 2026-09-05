namespace Shopizy.PaymentService.Application.Contracts;

public sealed record MoneyDto(decimal Amount, string Currency = "USD");

public sealed record ProcessPaymentRequest(
    Guid OrderId,
    string PaymentToken,
    MoneyDto Amount,
    string CardBrand = "Visa",
    string Last4 = "4242");

public sealed record RefundPaymentRequest(
    decimal? Amount = null,
    string Reason = "OrderCancelled");

public sealed record RefundResponse(
    Guid Id,
    string RefundReference,
    MoneyDto Amount,
    string Reason,
    DateTimeOffset CreatedAtUtc);

public sealed record PaymentResponse(
    Guid Id,
    Guid OrderId,
    Guid CustomerId,
    string Status,
    MoneyDto Amount,
    string CardBrand,
    string Last4,
    string? GatewayTransactionId,
    string? FailureReason,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? SucceededAtUtc,
    DateTimeOffset? RefundedAtUtc,
    RefundResponse? Refund);
