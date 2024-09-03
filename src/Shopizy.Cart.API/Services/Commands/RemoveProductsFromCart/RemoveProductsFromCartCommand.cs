using ErrorOr;
using Shopizy.Security.Permissions;
using Shopizy.Security.Policies;
using Shopizy.Security.Request;

namespace Shopizy.Cart.API.Services.Commands.RemoveProductsFromCart;

[Authorize(Permissions = Permissions.Cart.Delete, Policies = Policy.SelfOrAdmin)]
public record RemoveProductFromCartCommand(Guid UserId, Guid CartId, IList<Guid> ProductIds)
    : IAuthorizeableRequest<ErrorOr<Success>>;
