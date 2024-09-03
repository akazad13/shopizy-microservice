using ErrorOr;
using Shopizy.Domain.Models.Enums;
using Shopizy.Ordering.API.Aggregates;
using Shopizy.Security.Permissions;
using Shopizy.Security.Policies;
using Shopizy.Security.Request;

namespace Shopizy.Ordering.API.Services.Commands.CreateOrder;

[Authorize(Permissions = Permissions.Order.Get, Policies = Policy.SelfOrAdmin)]
public record CreateOrderCommand(
    Guid UserId,
    string PromoCode,
    decimal DeliveryChargeAmount,
    Currency DeliveryChargeCurrency,
    IList<OrderItemCommand> OrderItems,
    AddressCommand ShippingAddress
) : IAuthorizeableRequest<ErrorOr<Order>>;

public record OrderItemCommand(Guid ProductId, int Quantity);

public record AddressCommand(
    string Street,
    string City,
    string State,
    string Country,
    string ZipCode
);
