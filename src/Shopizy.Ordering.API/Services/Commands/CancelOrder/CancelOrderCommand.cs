using ErrorOr;
using Shopizy.Security.Permissions;
using Shopizy.Security.Policies;
using Shopizy.Security.Request;

namespace Shopizy.Ordering.API.Services.Commands.CancelOrder;

[Authorize(Permissions = Permissions.Order.Modify, Policies = Policy.SelfOrAdmin)]
public record CancelOrderCommand(Guid UserId, Guid OrderId, string Reason)
    : IAuthorizeableRequest<ErrorOr<Success>>;
