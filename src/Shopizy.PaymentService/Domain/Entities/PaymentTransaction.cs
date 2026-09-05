using Shopizy.PaymentService.Domain.Enums;
using Shopizy.PaymentService.Domain.Exceptions;
using Shopizy.PaymentService.Domain.ValueObjects;
using Shopizy.SharedKernel.Domain;

namespace Shopizy.PaymentService.Domain.Entities;

public sealed class PaymentTransaction : AggregateRoot<Guid>
{
    public Guid OrderId { get; private set; }
    public Guid CustomerId { get; private set; }
    public Money Amount { get; private set; } = null!;
    public PaymentMethod PaymentMethod { get; private set; } = null!;
    public PaymentStatus Status { get; private set; }
    public string? GatewayTransactionId { get; private set; }
    public string? FailureReason { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset? SucceededAtUtc { get; private set; }
    public DateTimeOffset? RefundedAtUtc { get; private set; }

    public RefundRecord? Refund { get; private set; }

    private PaymentTransaction() : base(Guid.Empty) { } // For EF Core

    public static PaymentTransaction Create(
        Guid id,
        Guid orderId,
        Guid customerId,
        Money amount,
        PaymentMethod paymentMethod)
    {
        if (id == Guid.Empty) throw new PaymentDomainException("Payment.InvalidId", "Payment ID must not be empty.");
        if (orderId == Guid.Empty) throw new PaymentDomainException("Payment.InvalidOrderId", "Order ID must not be empty.");
        if (customerId == Guid.Empty) throw new PaymentDomainException("Payment.InvalidCustomerId", "Customer ID must not be empty.");
        ArgumentNullException.ThrowIfNull(amount);
        ArgumentNullException.ThrowIfNull(paymentMethod);

        return new PaymentTransaction
        {
            Id = id,
            OrderId = orderId,
            CustomerId = customerId,
            Amount = amount,
            PaymentMethod = paymentMethod,
            Status = PaymentStatus.Initiated,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };
    }

    public void MarkSucceeded(string gatewayTransactionId)
    {
        if (Status != PaymentStatus.Initiated)
            throw new PaymentDomainException("Payment.InvalidStateTransition", $"Cannot mark payment in status '{Status}' as succeeded.");

        if (string.IsNullOrWhiteSpace(gatewayTransactionId))
            throw new PaymentDomainException("Payment.InvalidGatewayId", "Gateway transaction ID must not be empty.");

        Status = PaymentStatus.Succeeded;
        GatewayTransactionId = gatewayTransactionId.Trim();
        SucceededAtUtc = DateTimeOffset.UtcNow;
    }

    public void MarkFailed(string reason)
    {
        if (Status != PaymentStatus.Initiated)
            throw new PaymentDomainException("Payment.InvalidStateTransition", $"Cannot mark payment in status '{Status}' as failed.");

        Status = PaymentStatus.Failed;
        FailureReason = string.IsNullOrWhiteSpace(reason) ? "PaymentDeclined" : reason.Trim();
    }

    public void ApplyRefund(string refundReference, Money? refundAmount = null, string? reason = null)
    {
        if (Status != PaymentStatus.Succeeded)
            throw new PaymentDomainException("Payment.InvalidStateTransition", $"Cannot refund payment in status '{Status}'. Only succeeded payments can be refunded.");

        var amountToRefund = refundAmount ?? Amount;
        if (amountToRefund.Amount > Amount.Amount)
            throw new PaymentDomainException("Payment.ExcessiveRefund", "Refund amount exceeds transaction balance.");

        Refund = new RefundRecord(Guid.NewGuid(), Id, refundReference, amountToRefund, reason ?? "OrderCancelled");
        Status = PaymentStatus.Refunded;
        RefundedAtUtc = DateTimeOffset.UtcNow;
    }
}
