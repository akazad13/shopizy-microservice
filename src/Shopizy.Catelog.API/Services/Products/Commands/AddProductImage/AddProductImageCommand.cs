using ErrorOr;
using Shopizy.Catelog.API.Aggregates.Products.Entities;
using Shopizy.Security.Permissions;
using Shopizy.Security.Policies;
using Shopizy.Security.Request;


namespace Shopizy.Catelog.API.Services.Products.Commands.AddProductImage;

[Authorize(Permissions = Permissions.Product.Create, Policies = Policy.SelfOrAdmin)]
public record AddProductImageCommand(Guid UserId, Guid ProductId, IFormFile File)
    : IAuthorizeableRequest<ErrorOr<ProductImage>>;
