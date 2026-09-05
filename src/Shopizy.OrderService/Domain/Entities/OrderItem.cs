using Shopizy.OrderService.Domain.Exceptions;
using Shopizy.OrderService.Domain.ValueObjects;
using Shopizy.SharedKernel.Domain;

namespace Shopizy.OrderService.Domain.Entities;

public sealed class OrderItem : Entity<Guid>
{
    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public Guid VariantId { get; private set; }
    public string ProductName { get; private set; } = string.Empty;
    public string VariantSku { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public Money UnitPrice { get; private set; } = null!;
    public Money LineTotal => UnitPrice.Multiply(Quantity);

    private OrderItem() : base(Guid.Empty) { } // For EF Core

    public OrderItem(
        Guid id,
        Guid orderId,
        Guid productId,
        Guid variantId,
        string productName,
        string variantSku,
        int quantity,
        Money unitPrice) : base(id)
    {
        if (id == Guid.Empty) throw new OrderDomainException("OrderItem.InvalidId", "Item ID must not be empty.");
        if (orderId == Guid.Empty) throw new OrderDomainException("OrderItem.InvalidOrderId", "OrderId must not be empty.");
        if (productId == Guid.Empty) throw new OrderDomainException("OrderItem.InvalidProductId", "ProductId must not be empty.");
        if (variantId == Guid.Empty) throw new OrderDomainException("OrderItem.InvalidVariantId", "VariantId must not be empty.");
        if (string.IsNullOrWhiteSpace(productName)) throw new OrderDomainException("OrderItem.InvalidProductName", "Product name must not be empty.");
        if (string.IsNullOrWhiteSpace(variantSku)) throw new OrderDomainException("OrderItem.InvalidVariantSku", "Variant SKU must not be empty.");
        if (quantity is < 1 or > 99) throw new OrderDomainException("OrderItem.InvalidQuantity", "Quantity must be between 1 and 99.");
        ArgumentNullException.ThrowIfNull(unitPrice);

        OrderId = orderId;
        ProductId = productId;
        VariantId = variantId;
        ProductName = productName;
        VariantSku = variantSku;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }
}
