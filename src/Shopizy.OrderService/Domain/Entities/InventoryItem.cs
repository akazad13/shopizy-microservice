using Shopizy.OrderService.Domain.Exceptions;
using Shopizy.SharedKernel.Domain;

namespace Shopizy.OrderService.Domain.Entities;

public sealed class InventoryItem : Entity<Guid>
{
    public Guid VariantId => Id;
    public int AvailableStock { get; private set; }
    public int ReservedStock { get; private set; }
    public uint Version { get; private set; }

    private InventoryItem() : base(Guid.Empty) { } // For EF Core

    public InventoryItem(Guid variantId, int initialStock) : base(variantId)
    {
        if (variantId == Guid.Empty)
            throw new OrderDomainException("InventoryItem.InvalidVariantId", "VariantId must not be empty.");
        if (initialStock < 0)
            throw new OrderDomainException("InventoryItem.InvalidStock", "Initial stock cannot be negative.");

        AvailableStock = initialStock;
        ReservedStock = 0;
    }

    /// <summary>Atomically reserves stock for checkout. Throws if insufficient available stock.</summary>
    public void ReserveStock(int quantity)
    {
        if (quantity <= 0)
            throw new OrderDomainException("InventoryItem.InvalidQuantity", "Quantity to reserve must be positive.");

        if (AvailableStock < quantity)
            throw new OrderDomainException("InventoryItem.InsufficientStock", $"Insufficient stock for variant {Id}. Available: {AvailableStock}, Requested: {quantity}.");

        AvailableStock -= quantity;
        ReservedStock += quantity;
    }

    /// <summary>Releases previously reserved stock (e.g. on order cancellation or expiration).</summary>
    public void ReleaseReservation(int quantity)
    {
        if (quantity <= 0)
            throw new OrderDomainException("InventoryItem.InvalidQuantity", "Quantity to release must be positive.");

        int toRelease = Math.Min(quantity, ReservedStock);
        ReservedStock -= toRelease;
        AvailableStock += toRelease;
    }

    /// <summary>Commits reserved stock upon successful payment / shipment.</summary>
    public void CommitReservation(int quantity)
    {
        if (quantity <= 0)
            throw new OrderDomainException("InventoryItem.InvalidQuantity", "Quantity to commit must be positive.");

        int toCommit = Math.Min(quantity, ReservedStock);
        ReservedStock -= toCommit;
    }

    /// <summary>Directly restocks available stock (e.g. inventory arrival or return).</summary>
    public void Restock(int quantity)
    {
        if (quantity <= 0)
            throw new OrderDomainException("InventoryItem.InvalidQuantity", "Quantity to restock must be positive.");

        AvailableStock += quantity;
    }
}
