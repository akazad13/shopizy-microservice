namespace Shopizy.SharedKernel.Contracts.Payments;

public sealed record PaymentCompletedIntegrationEvent(
    Guid PaymentId,
    Guid OrderId,
    string TransactionId,
    decimal AmountPaid,
    string Currency,
    DateTime PaidAtUtc);

public sealed record PaymentFailedIntegrationEvent(
    Guid PaymentId,
    Guid OrderId,
    string Reason,
    DateTime FailedAtUtc);
