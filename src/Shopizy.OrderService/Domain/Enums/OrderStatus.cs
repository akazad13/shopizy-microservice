namespace Shopizy.OrderService.Domain.Enums;

public enum OrderStatus
{
    PendingPayment = 0,
    Processing = 1,
    Shipping = 2,
    Delivered = 3,
    Cancelled = 4
}
