namespace Shopizy.Contracts.Product;

public record ProductResponse(
    Guid ProductId,
    string Name,
    string Description,
    Guid CategoryId,
    string Price,
    decimal Discount,
    string Sku,
    string Brand,
    string Tags,
    string Barcode,
    int StockQuantity,
    List<Guid>? SpecificationIds,
    List<ProductImageResponse> ProductImages
);

public record ProductImageResponse(Guid ProductImageId, string ImageUrl, int Seq, string PublicId);
