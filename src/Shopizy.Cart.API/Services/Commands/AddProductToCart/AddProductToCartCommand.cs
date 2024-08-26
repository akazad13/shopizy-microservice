using ErrorOr;
using Shopizy.Cart.API.Aggregates;
using Shopizy.Security.Permissions;
using Shopizy.Security.Policies;
using Shopizy.Security.Request;


namespace Shopizy.Cart.API.Services.Commands.AddProductToCart;

[Authorize(Permissions = Permissions.Cart.Modify, Policies = Policy.SelfOrAdmin)]
public record AddProductToCartCommand(Guid UserId, Guid CartId, Guid ProductId)
    : IAuthorizeableRequest<ErrorOr<CustomerCart>>;
