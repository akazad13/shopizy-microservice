using FluentAssertions;
using Shopizy.OrderService.Domain.Entities;
using Shopizy.OrderService.Domain.Enums;
using Shopizy.OrderService.Domain.Exceptions;
using Shopizy.OrderService.Domain.ValueObjects;

namespace Shopizy.OrderService.UnitTests;

public sealed class OrderAggregateTests
{
    private static ShippingAddress ValidAddress() =>
        new("Alex Mercer", "123 Market St", "San Francisco", "CA", "94103", "USA");

    // ─── Order Creation ───────────────────────────────────────────────────────

    [Fact]
    public void Create_ValidParameters_SetsInitialState()
    {
        var id = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var order = Order.Create(id, "ORD-001", customerId, ValidAddress());

        order.Id.Should().Be(id);
        order.OrderNumber.Should().Be("ORD-001");
        order.CustomerId.Should().Be(customerId);
        order.Status.Should().Be(OrderStatus.PendingPayment);
        order.Items.Should().BeEmpty();
        order.ExpiresAtUtc.Should().BeAfter(order.CreatedAtUtc);
    }

    [Fact]
    public void Create_EmptyId_ThrowsDomainException()
    {
        var act = () => Order.Create(Guid.Empty, "ORD-001", Guid.NewGuid(), ValidAddress());
        act.Should().Throw<OrderDomainException>().WithMessage("*Order ID*");
    }

    [Fact]
    public void Create_EmptyOrderNumber_ThrowsDomainException()
    {
        var act = () => Order.Create(Guid.NewGuid(), "", Guid.NewGuid(), ValidAddress());
        act.Should().Throw<OrderDomainException>().WithMessage("*Order number*");
    }

    // ─── Items & Totals ───────────────────────────────────────────────────────

    [Fact]
    public void AddItem_ValidItem_RecalculatesTotalAmount()
    {
        var order = Order.Create(Guid.NewGuid(), "ORD-001", Guid.NewGuid(), ValidAddress());
        order.AddItem(Guid.NewGuid(), Guid.NewGuid(), "Headphones", "SKU-1", 2, Money.Create(50m));
        order.AddItem(Guid.NewGuid(), Guid.NewGuid(), "Cable", "SKU-2", 1, Money.Create(15m));

        order.Items.Should().HaveCount(2);
        order.TotalAmount.Amount.Should().Be(115m);
    }

    [Fact]
    public void AddItem_WhenNotInPendingPayment_ThrowsDomainException()
    {
        var order = Order.Create(Guid.NewGuid(), "ORD-001", Guid.NewGuid(), ValidAddress());
        order.MarkAsPaid();

        var act = () => order.AddItem(Guid.NewGuid(), Guid.NewGuid(), "Item", "SKU", 1, Money.Create(10m));
        act.Should().Throw<OrderDomainException>().WithMessage("*PendingPayment*");
    }

    // ─── State Machine ────────────────────────────────────────────────────────

    [Fact]
    public void MarkAsPaid_FromPendingPayment_TransitionsToProcessing()
    {
        var order = Order.Create(Guid.NewGuid(), "ORD-001", Guid.NewGuid(), ValidAddress());
        order.MarkAsPaid();

        order.Status.Should().Be(OrderStatus.Processing);
        order.PaidAtUtc.Should().NotBeNull();
    }

    [Fact]
    public void MarkAsPaid_WhenAlreadyPaid_ThrowsDomainException()
    {
        var order = Order.Create(Guid.NewGuid(), "ORD-001", Guid.NewGuid(), ValidAddress());
        order.MarkAsPaid();

        var act = () => order.MarkAsPaid();
        act.Should().Throw<OrderDomainException>().WithMessage("*Cannot mark*");
    }

    [Fact]
    public void MarkAsShipped_FromProcessing_TransitionsToShipping()
    {
        var order = Order.Create(Guid.NewGuid(), "ORD-001", Guid.NewGuid(), ValidAddress());
        order.MarkAsPaid();
        order.MarkAsShipped();

        order.Status.Should().Be(OrderStatus.Shipping);
        order.ShippedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public void MarkAsShipped_FromPendingPayment_ThrowsDomainException()
    {
        var order = Order.Create(Guid.NewGuid(), "ORD-001", Guid.NewGuid(), ValidAddress());
        var act = () => order.MarkAsShipped();
        act.Should().Throw<OrderDomainException>().WithMessage("*Processing*");
    }

    [Fact]
    public void MarkAsDelivered_FromShipping_TransitionsToDelivered()
    {
        var order = Order.Create(Guid.NewGuid(), "ORD-001", Guid.NewGuid(), ValidAddress());
        order.MarkAsPaid();
        order.MarkAsShipped();
        order.MarkAsDelivered();

        order.Status.Should().Be(OrderStatus.Delivered);
        order.DeliveredAtUtc.Should().NotBeNull();
    }

    // ─── Cancellation & Expiration ────────────────────────────────────────────

    [Fact]
    public void Cancel_WhenPendingPayment_TransitionsToCancelled()
    {
        var order = Order.Create(Guid.NewGuid(), "ORD-001", Guid.NewGuid(), ValidAddress());
        order.Cancel("CustomerChangedMind");

        order.Status.Should().Be(OrderStatus.Cancelled);
        order.CancelledAtUtc.Should().NotBeNull();
        order.CancellationReason.Should().Be("CustomerChangedMind");
    }

    [Fact]
    public void Cancel_WhenProcessing_TransitionsToCancelled()
    {
        var order = Order.Create(Guid.NewGuid(), "ORD-001", Guid.NewGuid(), ValidAddress());
        order.MarkAsPaid();
        order.Cancel("RefundRequested");

        order.Status.Should().Be(OrderStatus.Cancelled);
    }

    [Fact]
    public void Cancel_WhenShipping_ThrowsDomainException()
    {
        var order = Order.Create(Guid.NewGuid(), "ORD-001", Guid.NewGuid(), ValidAddress());
        order.MarkAsPaid();
        order.MarkAsShipped();

        var act = () => order.Cancel("TooLate");
        act.Should().Throw<OrderDomainException>().WithMessage("*Cannot cancel*");
    }

    [Fact]
    public void ExpireIfUnpaid_WhenPastExpiry_TransitionsToCancelled()
    {
        var order = Order.Create(Guid.NewGuid(), "ORD-001", Guid.NewGuid(), ValidAddress(), TimeSpan.FromMinutes(15));
        var future = DateTimeOffset.UtcNow.AddMinutes(16);

        var expired = order.ExpireIfUnpaid(future);

        expired.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Cancelled);
        order.CancellationReason.Should().Be("PaymentExpired");
    }

    [Fact]
    public void ExpireIfUnpaid_WhenNotPastExpiry_ReturnsFalse()
    {
        var order = Order.Create(Guid.NewGuid(), "ORD-001", Guid.NewGuid(), ValidAddress(), TimeSpan.FromMinutes(15));
        var withinWindow = DateTimeOffset.UtcNow.AddMinutes(5);

        var expired = order.ExpireIfUnpaid(withinWindow);

        expired.Should().BeFalse();
        order.Status.Should().Be(OrderStatus.PendingPayment);
    }
}
