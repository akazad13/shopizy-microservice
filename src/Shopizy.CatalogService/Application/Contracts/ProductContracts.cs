namespace Shopizy.CatalogService.Application.Contracts;

public sealed record ProductImageDto(
    string Url,
    string? AltText = null,
    int DisplayOrder = 0,
    bool IsMain = false);

public sealed record ProductVariantDto(
    string Sku,
    string? Barcode,
    decimal Price,
    string? Currency,
    int StockQuantity,
    Dictionary<string, string>? Attributes = null);

public sealed record CreateProductRequest(
    string Name,
    string Slug,
    string Description,
    Guid CategoryId,
    Guid BrandId,
    decimal BasePrice,
    string? Currency = "USD",
    List<ProductImageDto>? Images = null,
    List<ProductVariantDto>? Variants = null);

public sealed record UpdateProductRequest(
    string Name,
    string Slug,
    string Description,
    Guid CategoryId,
    Guid BrandId,
    decimal BasePrice,
    string? Currency,
    int ExpectedVersion);

public sealed record StockAdjustmentRequest(
    int NewQuantity);

public sealed record ProductImageResponse(
    Guid Id,
    string Url,
    string? AltText,
    int DisplayOrder,
    bool IsMain);

public sealed record ProductVariantResponse(
    Guid Id,
    string Sku,
    string? Barcode,
    decimal Price,
    string Currency,
    int StockQuantity,
    bool IsInStock,
    Dictionary<string, string> Attributes);

public sealed record ProductDetailResponse(
    Guid Id,
    string Name,
    string Slug,
    string Description,
    string Status,
    decimal BasePrice,
    string Currency,
    int Version,
    CategoryResponse? Category,
    BrandResponse? Brand,
    IReadOnlyCollection<ProductImageResponse> Images,
    IReadOnlyCollection<ProductVariantResponse> Variants,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);

public sealed record ProductListResponse(
    Guid Id,
    string Name,
    string Slug,
    string Description,
    string Status,
    decimal BasePrice,
    string Currency,
    int Version,
    string? CategoryName,
    string? BrandName,
    string? MainImageUrl,
    int TotalStock,
    bool IsInStock);

public sealed record ProductQueryParameters(
    Guid? CategoryId = null,
    Guid? BrandId = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    bool? InStockOnly = null,
    string? SearchTerm = null,
    string? SortBy = null,
    int Page = 1,
    int PageSize = 10);
