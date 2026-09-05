using Shopizy.ReviewService.Domain.Exceptions;

namespace Shopizy.ReviewService.Domain.Entities;

public class Wishlist
{
    private readonly List<WishlistItem> _items = new();

    public Guid Id { get; private set; }
    public Guid CustomerId { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public IReadOnlyCollection<WishlistItem> Items => _items.AsReadOnly();

    private Wishlist() { }

    public static Wishlist Create(Guid customerId)
    {
        if (customerId == Guid.Empty)
            throw new ReviewDomainException("INVALID_CUSTOMER_ID", "CustomerId cannot be empty.");

        return new Wishlist
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public WishlistItem AddItem(Guid productId, string productName, string sku, decimal priceSnapshot)
    {
        if (productId == Guid.Empty)
            throw new ReviewDomainException("INVALID_PRODUCT_ID", "ProductId cannot be empty.");

        var existing = _items.FirstOrDefault(i => i.ProductId == productId);
        if (existing != null)
        {
            return existing; // Idempotent add
        }

        var item = WishlistItem.Create(Id, productId, productName, sku, priceSnapshot);
        _items.Add(item);
        return item;
    }

    public bool RemoveItem(Guid productId)
    {
        var item = _items.FirstOrDefault(i => i.ProductId == productId);
        if (item == null) return false;
        return _items.Remove(item);
    }
}
