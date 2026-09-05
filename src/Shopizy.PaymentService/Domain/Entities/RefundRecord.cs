using Shopizy.PaymentService.Domain.Exceptions;
using Shopizy.PaymentService.Domain.ValueObjects;
using Shopizy.SharedKernel.Domain;

namespace Shopizy.PaymentService.Domain.Entities;

public sealed class RefundRecord : Entity<Guid>
{
    public Guid PaymentTransactionId { get; private set; }
    public string RefundReference { get; private set; } = string.Empty;
    public Money Amount { get; private set; } = null!;
    public string Reason { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAtUtc { get; private set; }

    private RefundRecord() : base(Guid.Empty) { } // For EF Core

    public RefundRecord(
        Guid id,
        Guid paymentTransactionId,
        string refundReference,
        Money amount,
        string reason) : base(id)
    {
        if (id == Guid.Empty) throw new PaymentDomainException("RefundRecord.InvalidId", "Refund ID must not be empty.");
        if (paymentTransactionId == Guid.Empty) throw new PaymentDomainException("RefundRecord.InvalidPaymentId", "Payment ID must not be empty.");
        if (string.IsNullOrWhiteSpace(refundReference)) throw new PaymentDomainException("RefundRecord.InvalidReference", "Refund reference must not be empty.");
        ArgumentNullException.ThrowIfNull(amount);

        PaymentTransactionId = paymentTransactionId;
        RefundReference = refundReference.Trim();
        Amount = amount;
        Reason = string.IsNullOrWhiteSpace(reason) ? "CustomerRequested" : reason.Trim();
        CreatedAtUtc = DateTimeOffset.UtcNow;
    }
}
