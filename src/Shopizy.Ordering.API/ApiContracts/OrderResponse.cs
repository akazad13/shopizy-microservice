namespace Shopizy.Ordering.API.ApiContracts;

public record OrderResponse(
    Guid OrderId,
    Guid CustomerId,
    Price DeliveryCharge,
    string OrderStatus,
    string PromoCode,
    Address ShippingAddress,
    string PaymentStatus,
    List<OrderItemResponse> OrderItems,
    DateTime CreatedOn,
    DateTime ModifiedOn
);

public record OrderItemResponse(
    Guid OrderItemId,
    string Name,
    string PictureUrl,
    int Quantity,
    decimal Discount
);
