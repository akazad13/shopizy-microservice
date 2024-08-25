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

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private ProductImage() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
}

