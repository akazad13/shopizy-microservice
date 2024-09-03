using Shopizy.Catelog.API.Aggregates.Products.ValueObjects;
using Shopizy.Domain.Models.Base;

namespace Shopizy.Catelog.API.Aggregates.Products.Entities;

public sealed class ProductImage : Entity<ProductImageId>
{
    public string ImageUrl { get; set; }
    public int Seq { get; set; }
    public string PublicId { get; set; }

    public static ProductImage Create(string productUrl, int seq, string publicId)
    {
        return new ProductImage(ProductImageId.CreateUnique(), productUrl, seq, publicId);
    }

    private ProductImage(ProductImageId productImageId, string imageUrl, int seq, string publicId)
        : base(productImageId)
    {
        ImageUrl = imageUrl;
        Seq = seq;
        PublicId = publicId;
    }

    private ProductImage() { }
}

