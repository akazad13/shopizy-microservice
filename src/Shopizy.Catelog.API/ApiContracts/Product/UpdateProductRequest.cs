namespace Shopizy.Catelog.API.ApiContracts.Product;

public record UpdateProductRequest(
    string Name,
    string Description,
    Guid CategoryId,
    decimal UnitPrice,
    int Currency,
    decimal Discount,
    string Sku,
    string Brand,
    string Tags,
    string Barcode,
    IList<Guid>? SpecificationIds
);
