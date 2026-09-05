using Shopizy.OrderService.Domain.Enums;
using Shopizy.OrderService.Domain.Exceptions;
using Shopizy.OrderService.Domain.ValueObjects;
using Shopizy.SharedKernel.Domain;

namespace Shopizy.OrderService.Domain.Entities;

public sealed class Order : AggregateRoot<Guid>
{
    public string OrderNumber { get; private set; } = string.Empty;
    public Guid CustomerId { get; private set; }
    public OrderStatus Status { get; private set; }
    public ShippingAddress ShippingAddress { get; private set; } = null!;
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset ExpiresAtUtc { get; private set; }
    public DateTimeOffset? PaidAtUtc { get; private set; }
    public DateTimeOffset? ShippedAtUtc { get; private set; }
    public DateTimeOffset? DeliveredAtUtc { get; private set; }
    public DateTimeOffset? CancelledAtUtc { get; private set; }
    public string? CancellationReason { get; private set; }

    private readonly List<OrderItem> _items = [];
    public IReadOnlyList<OrderItem> Items => _items.AsReadOnly();

    private Order() : base(Guid.Empty) { } // For EF Core

    public static Order Create(
        Guid id,
        string orderNumber,
        Guid customerId,
        ShippingAddress shippingAddress,
        TimeSpan? expirationWindow = null)
    {
        if (id == Guid.Empty) throw new OrderDomainException("Order.InvalidId", "Order ID must not be empty.");
        if (string.IsNullOrWhiteSpace(orderNumber)) throw new OrderDomainException("Order.InvalidOrderNumber", "Order number must not be empty.");
        if (customerId == Guid.Empty) throw new OrderDomainException("Order.InvalidCustomerId", "Customer ID must not be empty.");
        ArgumentNullException.ThrowIfNull(shippingAddress);

        var now = DateTimeOffset.UtcNow;
        var order = new Order
        {
            Id = id,
            OrderNumber = orderNumber.Trim(),
            CustomerId = customerId,
            Status = OrderStatus.PendingPayment,
            ShippingAddress = shippingAddress,
            CreatedAtUtc = now,
            ExpiresAtUtc = now.Add(expirationWindow ?? TimeSpan.FromMinutes(15))
        };

        return order;
    }

    public void AddItem(Guid productId, Guid variantId, string productName, string variantSku, int quantity, Money unitPrice)
    {
        if (Status != OrderStatus.PendingPayment)
            throw new OrderDomainException("Order.CannotModifyItems", "Cannot add items to order not in PendingPayment status.");

        var item = new OrderItem(Guid.NewGuid(), Id, productId, variantId, productName, variantSku, quantity, unitPrice);
        _items.Add(item);
    }

    public Money TotalAmount =>
        _items.Aggregate(Money.Zero(), (acc, item) => acc.Add(item.LineTotal));

    public bool IsExpired(DateTimeOffset? asOf = null)
    {
        var time = asOf ?? DateTimeOffset.UtcNow;
        return Status == OrderStatus.PendingPayment && time > ExpiresAtUtc;
    }

    public void MarkAsPaid()
    {
        if (Status != OrderStatus.PendingPayment)
            throw new OrderDomainException("Order.InvalidStateTransition", $"Cannot mark order in status '{Status}' as paid.");

        if (IsExpired())
            throw new OrderDomainException("Order.OrderExpired", "Cannot pay for an order that has already expired.");

        Status = OrderStatus.Processing;
        PaidAtUtc = DateTimeOffset.UtcNow;
    }

    public void MarkAsShipped()
    {
        if (Status != OrderStatus.Processing)
            throw new OrderDomainException("Order.InvalidStateTransition", $"Cannot ship order in status '{Status}'. Order must be in Processing status.");

        Status = OrderStatus.Shipping;
        ShippedAtUtc = DateTimeOffset.UtcNow;
    }

    public void MarkAsDelivered()
    {
        if (Status != OrderStatus.Shipping)
            throw new OrderDomainException("Order.InvalidStateTransition", $"Cannot deliver order in status '{Status}'. Order must be in Shipping status.");

        Status = OrderStatus.Delivered;
        DeliveredAtUtc = DateTimeOffset.UtcNow;
    }

    public void Cancel(string reason)
    {
        if (Status is OrderStatus.Shipping or OrderStatus.Delivered)
            throw new OrderDomainException("Order.CannotCancelShipped", "Cannot cancel order after shipment dispatch.");

        if (Status == OrderStatus.Cancelled)
            return; // Idempotent

        Status = OrderStatus.Cancelled;
        CancelledAtUtc = DateTimeOffset.UtcNow;
        CancellationReason = string.IsNullOrWhiteSpace(reason) ? "CustomerRequested" : reason.Trim();
    }

    public bool ExpireIfUnpaid(DateTimeOffset? asOf = null)
    {
        if (Status != OrderStatus.PendingPayment)
            return false;

        if (IsExpired(asOf))
        {
            Cancel("PaymentExpired");
            return true;
        }

        return false;
    }
}
