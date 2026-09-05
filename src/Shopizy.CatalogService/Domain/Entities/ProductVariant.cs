using Shopizy.CatalogService.Domain.ValueObjects;
using Shopizy.SharedKernel.Domain;
using Shopizy.SharedKernel.Results;

namespace Shopizy.CatalogService.Domain.Entities;

public sealed class ProductVariant : Entity<Guid>
{
    public Guid ProductId { get; private set; }
    public string Sku { get; private set; } = string.Empty;
    public string? Barcode { get; private set; }
    public Money Price { get; private set; } = null!;
    public int StockQuantity { get; private set; }
    public Dictionary<string, string> Attributes { get; private set; } = [];
    public bool IsActive { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }

    public bool IsInStock => StockQuantity > 0;

    private ProductVariant() { }

    private ProductVariant(
        Guid id,
        Guid productId,
        string sku,
        string? barcode,
        Money price,
        int stockQuantity,
        Dictionary<string, string>? attributes) : base(id)
    {
        ProductId = productId;
        Sku = sku;
        Barcode = barcode;
        Price = price;
        StockQuantity = stockQuantity;
        Attributes = attributes != null ? new Dictionary<string, string>(attributes) : [];
        IsActive = true;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public static Result<ProductVariant> Create(
        Guid productId,
        string sku,
        string? barcode,
        Money price,
        int stockQuantity,
        Dictionary<string, string>? attributes = null)
    {
        if (string.IsNullOrWhiteSpace(sku))
        {
            return Result.Failure<ProductVariant>(Error.Validation("ProductVariant.EmptySku", "Product variant SKU is required."));
        }

        if (sku.Length > 64)
        {
            return Result.Failure<ProductVariant>(Error.Validation("ProductVariant.SkuTooLong", "SKU cannot exceed 64 characters."));
        }

        if (price == null)
        {
            return Result.Failure<ProductVariant>(Error.Validation("ProductVariant.NullPrice", "Variant price is required."));
        }

        if (stockQuantity < 0)
        {
            return Result.Failure<ProductVariant>(Error.Validation("ProductVariant.NegativeStock", "Stock quantity cannot be negative."));
        }

        var normalizedSku = sku.Trim().ToUpperInvariant();

        return Result.Success(new ProductVariant(
            Guid.NewGuid(),
            productId,
            normalizedSku,
            barcode?.Trim(),
            price,
            stockQuantity,
            attributes));
    }

    public Result<ProductVariant> AdjustStock(int newQuantity)
    {
        if (newQuantity < 0)
        {
            return Result.Failure<ProductVariant>(Error.Validation("ProductVariant.NegativeStock", "Stock quantity cannot be negative."));
        }

        StockQuantity = newQuantity;
        UpdatedAtUtc = DateTime.UtcNow;

        return Result.Success(this);
    }

    public Result<ProductVariant> UpdatePrice(Money newPrice)
    {
        if (newPrice == null)
        {
            return Result.Failure<ProductVariant>(Error.Validation("ProductVariant.NullPrice", "Variant price is required."));
        }

        Price = newPrice;
        UpdatedAtUtc = DateTime.UtcNow;

        return Result.Success(this);
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
