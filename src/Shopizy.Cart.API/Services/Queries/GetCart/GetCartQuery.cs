using ErrorOr;
using MediatR;
using Shopizy.Cart.API.Aggregates;
using Shopizy.Security.Permissions;
using Shopizy.Security.Policies;
using Shopizy.Security.Request;

namespace Shopizy.Cart.API.Services.Queries.GetCart;

[Authorize(Permissions = Permissions.Cart.Get, Policies = Policy.SelfOrAdmin)]
public record GetCartQuery(Guid UserId) : IRequest<ErrorOr<CustomerCart?>>;
