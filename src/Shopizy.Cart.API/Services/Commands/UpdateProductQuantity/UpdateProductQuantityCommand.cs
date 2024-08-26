using ErrorOr;
using Shopizy.Security.Permissions;
using Shopizy.Security.Policies;
using Shopizy.Security.Request;

namespace Shopizy.Cart.API.Services.Commands.UpdateProductQuantity;

[Authorize(Permissions = Permissions.Cart.Modify, Policies = Policy.SelfOrAdmin)]
public record UpdateProductQuantityCommand(Guid UserId, Guid CartId, Guid ProductId, int Quantity)
    : IAuthorizeableRequest<ErrorOr<Success>>;
