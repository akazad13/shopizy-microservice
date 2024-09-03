using Shopizy.Catelog.API.Aggregates.Categories.ValueObjects;
using Shopizy.Catelog.API.Aggregates.ProductReviews;
using Shopizy.Catelog.API.Aggregates.Products.Entities;
using Shopizy.Catelog.API.Aggregates.Products.ValueObjects;
using Shopizy.Domain.Models.Base;
using Shopizy.Domain.Models.ValueObjects;

namespace Shopizy.Catelog.API.Aggregates.Products;

public sealed class Product : AggregateRoot<ProductId, Guid>
{
    private readonly List<ProductImage> _productImages = [];
    private readonly List<ProductReview> _productReviews = [];
    public string Name { get; private set; }
    public string Description { get; private set; }
    public CategoryId CategoryId { get; private set; }
    public string SKU { get; private set; }
    public int StockQuantity { get; private set; }
    public Price UnitPrice { get; private set; }
    public decimal? Discount { get; private set; }
    public string Brand { get; private set; }
    public string Barcode { get; private set; }
    public string Tags { get; private set; }
    public AverageRating AverageRating { get; private set; }
    public string BreadCrums { get; private set; }
    public DateTime CreatedOn { get; private set; }
    public DateTime? ModifiedOn { get; private set; }

    public IReadOnlyList<ProductImage> ProductImages => _productImages.AsReadOnly();
    public IReadOnlyList<ProductReview> ProductReviews => _productReviews.AsReadOnly();

    public static Product Create(
        string name,
        string description,
        CategoryId categoryId,
        string sku,
        Price unitPrice,
        decimal? discount,
        string brand,
        string barcode,
        string tags,
        string breadCrums
    )
    {
        return new Product(
            ProductId.CreateUnique(),
            name,
            description,
            categoryId,
            sku,
            0,
            unitPrice,
            discount,
            brand,
            barcode,
            tags,
            AverageRating.CreateNew(0),
            breadCrums
        );
    }

    public void Update(
        string name,
        string description,
        CategoryId categoryId,
        string sku,
        Price unitPrice,
        decimal? discount,
        string brand,
        string barcode,
        string tags

    )
    {

        Name = name;
        Description = description;
        CategoryId = categoryId;
        SKU = sku;
        UnitPrice = unitPrice;
        Discount = discount;
        Brand = brand;
        Barcode = barcode;
        Tags = tags;
        ModifiedOn = DateTime.UtcNow;
    }

    public void AddProductImages(IList<ProductImage> productImages)
    {
        _productImages.AddRange(productImages);
    }
    public void AddProductImage(ProductImage productImage)
    {
        _productImages.Add(productImage);
    }
    public void RemoveProductImage(ProductImage productImage)
    {
        _productImages.Remove(productImage);
    }
    private Product(
        ProductId productId,
        string name,
        string description,
        CategoryId categoryId,
        string sku,
        int stockQuantity,
        Price unitPrice,
        decimal? discount,
        string brand,
        string barcode,
        string tags,
        AverageRating averageRating,
        string breadCrums
    ) : base(productId)
    {
        Name = name;
        Description = description;
        CategoryId = categoryId;
        SKU = sku;
        StockQuantity = stockQuantity;
        UnitPrice = unitPrice;
        Discount = discount;
        Brand = brand;
        Barcode = barcode;
        Tags = tags;
        AverageRating = averageRating;
        BreadCrums = breadCrums;
        CreatedOn = DateTime.UtcNow;
    }

    private Product() { }
}
