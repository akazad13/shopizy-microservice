using Shopizy.Cart.API.Aggregates.ValueObjects;
using Shopizy.Domain.Models.Base;

namespace Shopizy.Cart.API.Aggregates.Entities;

public sealed class CartItem : Entity<CartItemId>
{
    public ProductId ProductId { get; private set; }
    public int Quantity { get; private set; }

    public static CartItem Create(ProductId productId)
    {
        return new CartItem(CartItemId.CreateUnique(), productId);
    }

    public void UpdateQuantity(int quantity)
    {
        Quantity = quantity;
    }

    private CartItem(CartItemId cartItemId, ProductId productId) : base(cartItemId)
    {
        ProductId = productId;
        Quantity = 1;
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private CartItem() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
}

