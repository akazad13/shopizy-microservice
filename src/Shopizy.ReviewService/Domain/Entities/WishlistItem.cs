namespace Shopizy.ReviewService.Domain.Entities;

public class WishlistItem
{
    public Guid Id { get; private set; }
    public Guid WishlistId { get; private set; }
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; } = string.Empty;
    public string Sku { get; private set; } = string.Empty;
    public decimal PriceSnapshot { get; private set; }
    public DateTime AddedAtUtc { get; private set; }

    private WishlistItem() { }

    public static WishlistItem Create(Guid wishlistId, Guid productId, string productName, string sku, decimal priceSnapshot)
    {
        return new WishlistItem
        {
            Id = Guid.NewGuid(),
            WishlistId = wishlistId,
            ProductId = productId,
            ProductName = productName,
            Sku = sku,
            PriceSnapshot = priceSnapshot,
            AddedAtUtc = DateTime.UtcNow
        };
    }
}
