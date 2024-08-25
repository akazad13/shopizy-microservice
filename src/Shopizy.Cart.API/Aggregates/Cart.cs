using Shopizy.Cart.API.Aggregates.Entities;
using Shopizy.Cart.API.Aggregates.ValueObjects;
using Shopizy.Domain.Models.Base;

namespace Shopizy.Cart.API.Aggregates;

public sealed class CustomerCart : AggregateRoot<CartId, Guid>
{
    private readonly List<CartItem> _cartItems = [];
    public CustomerId CustomerId { get; }
    public DateTime CreatedOn { get; private set; }
    public DateTime? ModifiedOn { get; private set; }
    public IReadOnlyList<CartItem> CartItems => _cartItems.AsReadOnly();

    public static CustomerCart Create(
        CustomerId customerId
    )
    {
        return new CustomerCart(
            CartId.CreateUnique(),
            customerId);
    }

    public void AddLineItem(CartItem lineItem)
    {
        _cartItems.Add(lineItem);
    }

    public void RemoveLineItem(CartItem lineItem)
    {
        _cartItems.Remove(lineItem);
    }

    public void UpdateLineItem(ProductId productId, int quantity)
    {
        _cartItems.Find(li => li.ProductId == productId)?.UpdateQuantity(quantity);
    }

    private CustomerCart(
        CartId cartId,
        CustomerId customerId
    ) : base(cartId)
    {
        CustomerId = customerId;
        CreatedOn = DateTime.UtcNow;
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private CustomerCart() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
}
