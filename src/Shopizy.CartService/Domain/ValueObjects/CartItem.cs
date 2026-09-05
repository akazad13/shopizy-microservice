namespace Shopizy.CartService.Domain.ValueObjects;

/// <summary>Immutable cart line item (snapshot-priced).</summary>
public sealed class CartItem
{
    public Guid ProductId { get; }
    public Guid VariantId { get; }
    public string ProductName { get; }
    public string VariantSku { get; }
    public IReadOnlyDictionary<string, string> Attributes { get; }
    public int Quantity { get; private set; }
    public Money SnapshotPrice { get; private set; }
    public DateTimeOffset AddedAtUtc { get; }

    public CartItem(
        Guid productId,
        Guid variantId,
        string productName,
        string variantSku,
        Dictionary<string, string>? attributes,
        int quantity,
        Money snapshotPrice,
        DateTimeOffset addedAtUtc)
    {
        if (productId == Guid.Empty)
            throw new Shopizy.SharedKernel.Domain.DomainException("CartItem.InvalidProductId", "ProductId must not be empty.");
        if (variantId == Guid.Empty)
            throw new Shopizy.SharedKernel.Domain.DomainException("CartItem.InvalidVariantId", "VariantId must not be empty.");
        if (string.IsNullOrWhiteSpace(productName))
            throw new Shopizy.SharedKernel.Domain.DomainException("CartItem.InvalidProductName", "Product name must not be empty.");
        if (string.IsNullOrWhiteSpace(variantSku))
            throw new Shopizy.SharedKernel.Domain.DomainException("CartItem.InvalidVariantSku", "Variant SKU must not be empty.");
        if (quantity is < 1 or > 99)
            throw new Shopizy.SharedKernel.Domain.DomainException("CartItem.InvalidQuantity", "Quantity must be between 1 and 99.");

        ProductId = productId;
        VariantId = variantId;
        ProductName = productName;
        VariantSku = variantSku;
        Attributes = (attributes ?? new Dictionary<string, string>()).AsReadOnly();
        Quantity = quantity;
        SnapshotPrice = snapshotPrice;
        AddedAtUtc = addedAtUtc;
    }

    public Money LineTotal => SnapshotPrice.Multiply(Quantity);

    public void SetQuantity(int quantity)
    {
        if (quantity is < 1 or > 99)
            throw new Shopizy.SharedKernel.Domain.DomainException("CartItem.InvalidQuantity", "Quantity must be between 1 and 99.");
        Quantity = quantity;
    }

    public void UpdateSnapshotPrice(Money newPrice)
    {
        ArgumentNullException.ThrowIfNull(newPrice);
        SnapshotPrice = newPrice;
    }
}
