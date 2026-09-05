using Shopizy.SharedKernel.Domain;
using Shopizy.SharedKernel.Results;

namespace Shopizy.CatalogService.Domain.Entities;

public sealed class ProductImage : Entity<Guid>
{
    public Guid ProductId { get; private set; }
    public string Url { get; private set; } = string.Empty;
    public string? AltText { get; private set; }
    public int DisplayOrder { get; private set; }
    public bool IsMain { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private ProductImage() { }

    private ProductImage(Guid id, Guid productId, string url, string? altText, int displayOrder, bool isMain) : base(id)
    {
        ProductId = productId;
        Url = url;
        AltText = altText;
        DisplayOrder = displayOrder;
        IsMain = isMain;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public static Result<ProductImage> Create(Guid productId, string url, string? altText = null, int displayOrder = 0, bool isMain = false)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return Result.Failure<ProductImage>(Error.Validation("ProductImage.EmptyUrl", "Image URL is required."));
        }

        if (url.Length > 1000)
        {
            return Result.Failure<ProductImage>(Error.Validation("ProductImage.UrlTooLong", "Image URL cannot exceed 1000 characters."));
        }

        return Result.Success(new ProductImage(Guid.NewGuid(), productId, url.Trim(), altText?.Trim(), displayOrder, isMain));
    }

    public void SetMain(bool isMain)
    {
        IsMain = isMain;
    }

    public void SetDisplayOrder(int order)
    {
        DisplayOrder = order;
    }
}
