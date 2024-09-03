using ErrorOr;
using Shopizy.Catelog.API.Aggregates.Products;
using Shopizy.Domain.Models.Enums;
using Shopizy.Security.Permissions;
using Shopizy.Security.Policies;
using Shopizy.Security.Request;

namespace Shopizy.Catelog.API.Services.Products.Commands.CreateProduct;

[Authorize(Permissions = Permissions.Product.Create, Policies = Policy.SelfOrAdmin)]
public record CreateProductCommand(
    Guid UserId,
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
