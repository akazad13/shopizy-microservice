namespace Shopizy.Ordering.API.ApiContracts;

public record CreateOrderRequest(
    string PromoCode,
    Price DeliveryCharge,
    IList<OrderItemRequest> OrderItems,
    Address ShippingAddress
);

public record Price(decimal Amount, int Currency);

public record OrderItemRequest(Guid ProductId, int Quantity);

public record Address(string Street, string City, string State, string Country, string ZipCode);
