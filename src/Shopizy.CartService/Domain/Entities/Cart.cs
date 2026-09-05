using Shopizy.CartService.Domain.ValueObjects;
using Shopizy.SharedKernel.Domain;

namespace Shopizy.CartService.Domain.Entities;

/// <summary>Shopping Cart aggregate root — Redis-backed, price-snapshot enforced.</summary>
public sealed class Cart
{
    public string Id { get; }
    public Guid? CustomerId { get; }
    private readonly List<CartItem> _items = [];
    public IReadOnlyList<CartItem> Items => _items.AsReadOnly();
    public DateTimeOffset UpdatedAtUtc { get; private set; }

    private Cart(string id, Guid? customerId)
    {
        Id = id;
        CustomerId = customerId;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    // Parameterless ctor for JSON deserialization
    private Cart() { Id = string.Empty; }

    public static Cart CreateForCustomer(Guid customerId)
    {
        if (customerId == Guid.Empty)
            throw new DomainException("Cart.InvalidCustomerId", "Customer ID must not be empty.");
        return new Cart($"cart:customer:{customerId}", customerId);
    }

    public static Cart CreateForGuest(string guestId)
    {
        if (string.IsNullOrWhiteSpace(guestId))
            throw new DomainException("Cart.InvalidGuestId", "Guest cart ID must not be empty.");
        return new Cart($"cart:guest:{guestId}", null);
    }

    public static Cart Restore(string id, Guid? customerId, List<CartItem> items, DateTimeOffset updatedAtUtc)
    {
        var cart = new Cart(id, customerId);
        cart._items.AddRange(items);
        cart.UpdatedAtUtc = updatedAtUtc;
        return cart;
    }

    /// <summary>Add or increment a variant item with price snapshot.</summary>
    public void AddItem(
        Guid productId,
        Guid variantId,
        string productName,
        string variantSku,
        Dictionary<string, string>? attributes,
        int quantity,
        Money snapshotPrice)
    {
        var existing = _items.FirstOrDefault(i => i.VariantId == variantId);
        if (existing is not null)
        {
            int newQty = existing.Quantity + quantity;
            if (newQty > 99) newQty = 99;
            existing.SetQuantity(newQty);
            existing.UpdateSnapshotPrice(snapshotPrice);
        }
        else
        {
            var item = new CartItem(productId, variantId, productName, variantSku, attributes, quantity, snapshotPrice, DateTimeOffset.UtcNow);
            _items.Add(item);
        }
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    /// <summary>Update quantity of an existing item.</summary>
    public void UpdateItemQuantity(Guid variantId, int quantity)
    {
        var item = _items.FirstOrDefault(i => i.VariantId == variantId)
            ?? throw new DomainException("Cart.ItemNotFound", $"Item with variant ID {variantId} not found in cart.");
        item.SetQuantity(quantity);
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    /// <summary>Remove an item by variant.</summary>
    public void RemoveItem(Guid variantId)
    {
        var item = _items.FirstOrDefault(i => i.VariantId == variantId)
            ?? throw new DomainException("Cart.ItemNotFound", $"Item with variant ID {variantId} not found in cart.");
        _items.Remove(item);
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    /// <summary>Clear all items from cart.</summary>
    public void Clear()
    {
        _items.Clear();
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    /// <summary>Merge a guest cart into this customer cart (union by variant, sum quantities up to 99).</summary>
    public void MergeWith(Cart guestCart)
    {
        foreach (var guestItem in guestCart.Items)
        {
            var existing = _items.FirstOrDefault(i => i.VariantId == guestItem.VariantId);
            if (existing is not null)
            {
                int merged = Math.Min(existing.Quantity + guestItem.Quantity, 99);
                existing.SetQuantity(merged);
            }
            else
            {
                _items.Add(new CartItem(
                    guestItem.ProductId,
                    guestItem.VariantId,
                    guestItem.ProductName,
                    guestItem.VariantSku,
                    new Dictionary<string, string>(guestItem.Attributes),
                    guestItem.Quantity,
                    guestItem.SnapshotPrice,
                    guestItem.AddedAtUtc));
            }
        }
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public Money Subtotal =>
        _items.Aggregate(Money.Zero(), (acc, item) => acc.Add(item.LineTotal));

    public int TotalItemsCount => _items.Sum(i => i.Quantity);
}
