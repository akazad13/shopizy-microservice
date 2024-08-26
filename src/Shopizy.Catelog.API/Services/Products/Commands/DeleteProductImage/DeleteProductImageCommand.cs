using ErrorOr;
using Shopizy.Security.Permissions;
using Shopizy.Security.Policies;
using Shopizy.Security.Request;

namespace Shopizy.Catelog.API.Services.Products.Commands.DeleteProductImage;

[Authorize(Permissions = Permissions.Product.Delete, Policies = Policy.SelfOrAdmin)]
public record DeleteProductImageCommand(Guid UserId, Guid ProductId, Guid ImageId)
    : IAuthorizeableRequest<ErrorOr<Success>>;
