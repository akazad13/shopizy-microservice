using ErrorOr;
using Shopizy.Catelog.API.Aggregates.Products;
using Shopizy.Domain.Models.Enums;
using Shopizy.Security.Permissions;
using Shopizy.Security.Policies;
using Shopizy.Security.Request;

namespace Shopizy.Catelog.API.Services.Products.Commands.UpdateProduct;

[Authorize(Permissions = Permissions.Product.Modify, Policies = Policy.SelfOrAdmin)]
public record UpdateProductCommand(
    Guid UserId,
    Guid ProductId,
    string Name,
    string Description,
    Guid CategoryId,
    decimal UnitPrice,
    Currency Currency,
    decimal Discount,
    string Sku,
    string Brand,
    string Tags,
    string Barcode,
    IList<Guid>? SpecificationIds
) : IAuthorizeableRequest<ErrorOr<Product>>;
