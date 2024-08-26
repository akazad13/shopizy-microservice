using ErrorOr;
using Shopizy.Security.Permissions;
using Shopizy.Security.Policies;
using Shopizy.Security.Request;

namespace Shopizy.Catelog.API.Services.Products.Commands.DeleteProduct;

[Authorize(Permissions = Permissions.Product.Delete, Policies = Policy.SelfOrAdmin)]
public record DeleteProductCommand(Guid UserId, Guid ProductId)
    : IAuthorizeableRequest<ErrorOr<Success>>;
