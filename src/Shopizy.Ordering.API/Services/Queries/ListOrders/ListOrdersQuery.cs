using ErrorOr;
using Shopizy.Ordering.API.Aggregates;
using Shopizy.Security.Permissions;
using Shopizy.Security.Policies;
using Shopizy.Security.Request;

namespace Shopizy.Ordering.API.Services.Queries.ListOrders;

[Authorize(Permissions = Permissions.Order.Get, Policies = Policy.SelfOrAdmin)]
public record ListOrdersQuery(Guid UserId) : IAuthorizeableRequest<ErrorOr<List<Order>>>;
