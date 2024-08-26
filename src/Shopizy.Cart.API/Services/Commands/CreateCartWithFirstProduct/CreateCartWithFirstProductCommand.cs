using ErrorOr;
using Shopizy.Cart.API.Aggregates;
using Shopizy.Security.Permissions;
using Shopizy.Security.Policies;
using Shopizy.Security.Request;

namespace Shopizy.Cart.API.Services.Commands.CreateCartWithFirstProduct;

[Authorize(Permissions = Permissions.Cart.Create, Policies = Policy.SelfOrAdmin)]
public record CreateCartWithFirstProductCommand(Guid UserId, Guid ProductId)
    : IAuthorizeableRequest<ErrorOr<CustomerCart>>;
