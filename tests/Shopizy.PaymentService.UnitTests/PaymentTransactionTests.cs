using FluentAssertions;
using Shopizy.PaymentService.Domain.Entities;
using Shopizy.PaymentService.Domain.Enums;
using Shopizy.PaymentService.Domain.Exceptions;
using Shopizy.PaymentService.Domain.ValueObjects;

namespace Shopizy.PaymentService.UnitTests;

public sealed class PaymentTransactionTests
{
    private static PaymentMethod ValidMethod() => new("tok_visa", "Visa", "4242");

    [Fact]
    public void Create_ValidParameters_SetsInitialState()
    {
        var id = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var amount = Money.Create(100m);

        var tx = PaymentTransaction.Create(id, orderId, customerId, amount, ValidMethod());

        tx.Id.Should().Be(id);
        tx.OrderId.Should().Be(orderId);
        tx.CustomerId.Should().Be(customerId);
        tx.Amount.Amount.Should().Be(100m);
        tx.Status.Should().Be(PaymentStatus.Initiated);
        tx.Refund.Should().BeNull();
    }

    [Fact]
    public void MarkSucceeded_FromInitiated_TransitionsToSucceeded()
    {
        var tx = PaymentTransaction.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Money.Create(100m), ValidMethod());
        tx.MarkSucceeded("ch_12345");

        tx.Status.Should().Be(PaymentStatus.Succeeded);
        tx.GatewayTransactionId.Should().Be("ch_12345");
        tx.SucceededAtUtc.Should().NotBeNull();
    }

    [Fact]
    public void MarkFailed_FromInitiated_TransitionsToFailed()
    {
        var tx = PaymentTransaction.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Money.Create(100m), ValidMethod());
        tx.MarkFailed("CardDeclined");

        tx.Status.Should().Be(PaymentStatus.Failed);
        tx.FailureReason.Should().Be("CardDeclined");
    }

    [Fact]
    public void Refund_FromSucceeded_TransitionsToRefunded()
    {
        var tx = PaymentTransaction.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Money.Create(100m), ValidMethod());
        tx.MarkSucceeded("ch_12345");

        tx.ApplyRefund("re_98765", Money.Create(100m), "OrderCancelled");

        tx.Status.Should().Be(PaymentStatus.Refunded);
        tx.Refund.Should().NotBeNull();
        tx.Refund!.RefundReference.Should().Be("re_98765");
        tx.Refund.Amount.Amount.Should().Be(100m);
    }

    [Fact]
    public void Refund_WhenNotSucceeded_ThrowsDomainException()
    {
        var tx = PaymentTransaction.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Money.Create(100m), ValidMethod());
        var act = () => tx.ApplyRefund("re_98765");

        act.Should().Throw<PaymentDomainException>().WithMessage("*Only succeeded*");
    }

    [Fact]
    public void Refund_ExceedingAmount_ThrowsDomainException()
    {
        var tx = PaymentTransaction.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Money.Create(100m), ValidMethod());
        tx.MarkSucceeded("ch_12345");

        var act = () => tx.ApplyRefund("re_98765", Money.Create(150m));
        act.Should().Throw<PaymentDomainException>().WithMessage("*exceeds transaction balance*");
    }
}
