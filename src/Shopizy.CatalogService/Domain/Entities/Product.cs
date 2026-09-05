using Shopizy.CatalogService.Domain.Enums;
using Shopizy.CatalogService.Domain.Events;
using Shopizy.CatalogService.Domain.ValueObjects;
using Shopizy.SharedKernel.Domain;
using Shopizy.SharedKernel.Results;

namespace Shopizy.CatalogService.Domain.Entities;

public sealed class Product : AggregateRoot<Guid>
{
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public Guid CategoryId { get; private set; }
    public Guid BrandId { get; private set; }
    public Money BasePrice { get; private set; } = null!;
    public ProductStatus Status { get; private set; }
    public int Version { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }

    private readonly List<ProductImage> _images = [];
    public IReadOnlyCollection<ProductImage> Images => _images.AsReadOnly();

    private readonly List<ProductVariant> _variants = [];
    public IReadOnlyCollection<ProductVariant> Variants => _variants.AsReadOnly();

    private Product() { }

    private Product(
        Guid id,
        string name,
        string slug,
        string description,
        Guid categoryId,
        Guid brandId,
        Money basePrice) : base(id)
    {
        Name = name;
        Slug = slug;
        Description = description;
        CategoryId = categoryId;
        BrandId = brandId;
        BasePrice = basePrice;
        Status = ProductStatus.Draft;
        Version = 1;
        CreatedAtUtc = DateTime.UtcNow;

        RaiseDomainEvent(new ProductCreatedDomainEvent(Id, Name, Slug));
    }

    public static Result<Product> Create(
        string name,
        string slug,
        string description,
        Guid categoryId,
        Guid brandId,
        Money basePrice)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<Product>(Error.Validation("Product.EmptyName", "Product name is required."));
        }

        if (name.Length > 200)
        {
            return Result.Failure<Product>(Error.Validation("Product.NameTooLong", "Product name cannot exceed 200 characters."));
        }

        if (string.IsNullOrWhiteSpace(slug))
        {
            return Result.Failure<Product>(Error.Validation("Product.EmptySlug", "Product slug is required."));
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            return Result.Failure<Product>(Error.Validation("Product.EmptyDescription", "Product description is required."));
        }

        if (categoryId == Guid.Empty)
        {
            return Result.Failure<Product>(Error.Validation("Product.InvalidCategory", "Valid category ID is required."));
        }

        if (brandId == Guid.Empty)
        {
            return Result.Failure<Product>(Error.Validation("Product.InvalidBrand", "Valid brand ID is required."));
        }

        if (basePrice == null)
        {
            return Result.Failure<Product>(Error.Validation("Product.NullPrice", "Base price is required."));
        }

        var normalizedSlug = slug.Trim().ToLowerInvariant();

        return Result.Success(new Product(
            Guid.NewGuid(),
            name.Trim(),
            normalizedSlug,
            description.Trim(),
            categoryId,
            brandId,
            basePrice));
    }

    public Result<Product> Update(
        string name,
        string slug,
        string description,
        Guid categoryId,
        Guid brandId,
        Money basePrice,
        int expectedVersion)
    {
        if (Status == ProductStatus.Archived)
        {
            return Result.Failure<Product>(Error.Validation("Product.Archived", "Cannot update an archived product."));
        }

        if (expectedVersion != Version)
        {
            return Result.Failure<Product>(Error.Conflict(
                "Product.ConcurrencyConflict",
                $"Product has been modified by another process. Expected version {expectedVersion}, current version {Version}."));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<Product>(Error.Validation("Product.EmptyName", "Product name is required."));
        }

        if (name.Length > 200)
        {
            return Result.Failure<Product>(Error.Validation("Product.NameTooLong", "Product name cannot exceed 200 characters."));
        }

        if (string.IsNullOrWhiteSpace(slug))
        {
            return Result.Failure<Product>(Error.Validation("Product.EmptySlug", "Product slug is required."));
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            return Result.Failure<Product>(Error.Validation("Product.EmptyDescription", "Product description is required."));
        }

        if (categoryId == Guid.Empty)
        {
            return Result.Failure<Product>(Error.Validation("Product.InvalidCategory", "Valid category ID is required."));
        }

        if (brandId == Guid.Empty)
        {
            return Result.Failure<Product>(Error.Validation("Product.InvalidBrand", "Valid brand ID is required."));
        }

        if (basePrice == null)
        {
            return Result.Failure<Product>(Error.Validation("Product.NullPrice", "Base price is required."));
        }

        Name = name.Trim();
        Slug = slug.Trim().ToLowerInvariant();
        Description = description.Trim();
        CategoryId = categoryId;
        BrandId = brandId;
        BasePrice = basePrice;
        Version++;
        UpdatedAtUtc = DateTime.UtcNow;

        RaiseDomainEvent(new ProductUpdatedDomainEvent(Id, Name, Version));

        return Result.Success(this);
    }

    public Result<ProductVariant> AddVariant(
        string sku,
        string? barcode,
        Money price,
        int stockQuantity,
        Dictionary<string, string>? attributes = null)
    {
        if (Status == ProductStatus.Archived)
        {
            return Result.Failure<ProductVariant>(Error.Validation("Product.Archived", "Cannot add variant to an archived product."));
        }

        var normalizedSku = sku?.Trim().ToUpperInvariant() ?? string.Empty;
        if (_variants.Any(v => v.Sku.Equals(normalizedSku, StringComparison.OrdinalIgnoreCase)))
        {
            return Result.Failure<ProductVariant>(Error.Conflict(
                "Product.DuplicateSku",
                $"A variant with SKU '{normalizedSku}' already exists for this product."));
        }

        var variantResult = ProductVariant.Create(Id, sku ?? string.Empty, barcode, price, stockQuantity, attributes);
        if (variantResult.IsFailure)
        {
            return variantResult;
        }

        _variants.Add(variantResult.Value);
        UpdatedAtUtc = DateTime.UtcNow;

        return Result.Success(variantResult.Value);
    }

    public Result<ProductVariant> UpdateVariantStock(Guid variantId, int newQuantity)
    {
        var variant = _variants.FirstOrDefault(v => v.Id == variantId);
        if (variant == null)
        {
            return Result.Failure<ProductVariant>(Error.NotFound("ProductVariant.NotFound", $"Variant with ID '{variantId}' not found."));
        }

        var oldStock = variant.StockQuantity;
        var adjustResult = variant.AdjustStock(newQuantity);
        if (adjustResult.IsFailure)
        {
            return adjustResult;
        }

        UpdatedAtUtc = DateTime.UtcNow;
        RaiseDomainEvent(new ProductStockUpdatedDomainEvent(Id, variantId, variant.Sku, oldStock, newQuantity));

        return Result.Success(variant);
    }

    public Result<ProductImage> AddImage(string url, string? altText = null, int displayOrder = 0, bool isMain = false)
    {
        if (isMain)
        {
            foreach (var img in _images)
            {
                img.SetMain(false);
            }
        }

        var imageResult = ProductImage.Create(Id, url, altText, displayOrder, isMain);
        if (imageResult.IsFailure)
        {
            return imageResult;
        }

        _images.Add(imageResult.Value);
        UpdatedAtUtc = DateTime.UtcNow;

        return Result.Success(imageResult.Value);
    }

    public Result<Product> Publish()
    {
        if (Status == ProductStatus.Archived)
        {
            return Result.Failure<Product>(Error.Validation("Product.Archived", "Cannot publish an archived product."));
        }

        Status = ProductStatus.Published;
        UpdatedAtUtc = DateTime.UtcNow;

        return Result.Success(this);
    }

    public Result<Product> Archive()
    {
        Status = ProductStatus.Archived;
        UpdatedAtUtc = DateTime.UtcNow;

        return Result.Success(this);
    }
}
