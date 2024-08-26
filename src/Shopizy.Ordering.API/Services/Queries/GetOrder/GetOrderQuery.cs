using ErrorOr;
using Shopizy.Ordering.API.Aggregates;
using Shopizy.Security.Permissions;
using Shopizy.Security.Policies;
using Shopizy.Security.Request;

namespace Shopizy.Ordering.API.Services.Queries.GetOrder;

[Authorize(Permissions = Permissions.Order.Get, Policies = Policy.SelfOrAdmin)]
public record GetOrderQuery(Guid UserId, Guid OrderId) : IAuthorizeableRequest<ErrorOr<Order>>;
