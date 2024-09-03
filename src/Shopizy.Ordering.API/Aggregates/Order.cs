using Shopizy.Domain.Models.Base;
using Shopizy.Domain.Models.Enums;
using Shopizy.Domain.Models.ValueObjects;
using Shopizy.Ordering.API.Aggregates.Entities;
using Shopizy.Ordering.API.Aggregates.ValueObjects;

namespace Shopizy.Ordering.API.Aggregates;

public sealed class Order : AggregateRoot<OrderId, Guid>
{
    private readonly IList<OrderItem> _orderItems = [];
    public CustomerId CustomerId { get; private set; }
    public Price DeliveryCharge { get; private set; }
    public OrderStatus OrderStatus { get; private set; }
    public string? CancellationReason { get; private set; }
    public string PromoCode { get; private set; }
    public Address ShippingAddress { get; private set; }
    public PaymentStatus PaymentStatus { get; private set; }
    public DateTime CreatedOn { get; private set; }
    public DateTime? ModifiedOn { get; private set; }
    public IReadOnlyList<OrderItem> OrderItems => _orderItems.AsReadOnly();

    public static Order Create(
        CustomerId customerId,
        string promoCode,
        Price deliveryCharge,
        Address shippingAddress,
        IList<OrderItem> orderItems
    )
    {
        return new Order(
            OrderId.CreateUnique(),
            customerId,
            promoCode,
            deliveryCharge,
            shippingAddress,
            orderItems);
    }
    private Order(
        OrderId orderId,
        CustomerId customerId,
        string promoCode,
        Price deliveryCharge,
        Address shippingAddress,
        IList<OrderItem> orderItems
    ) : base(orderId)
    {
        CustomerId = customerId;
        OrderStatus = OrderStatus.Submitted;
        PromoCode = promoCode;
        DeliveryCharge = deliveryCharge;
        ShippingAddress = shippingAddress;
        _orderItems = orderItems;
        PaymentStatus = PaymentStatus.Pending;
        CreatedOn = DateTime.UtcNow;
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private Order() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    public void CancelOrder(string reason)
    {
        CancellationReason = reason;
        OrderStatus = OrderStatus.Cancelled;
        ModifiedOn = DateTime.UtcNow;
    }

    public Price GetTotal()
    {
        decimal totalAmount = _orderItems.Sum(item => item.TotalPrice().Amount);
        decimal totalDiscount = _orderItems.Sum(item => item.TotalDiscount().Amount);

        decimal chargeAmount = totalAmount - totalDiscount + DeliveryCharge.Amount;

        return Price.CreateNew(chargeAmount, DeliveryCharge.Currency);
    }
}
