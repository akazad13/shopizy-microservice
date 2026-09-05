using Shopizy.OrderService.Domain.Entities;

namespace Shopizy.OrderService.Application.Interfaces;

public interface IInventoryRepository
{
    Task<InventoryItem?> GetByVariantIdAsync(Guid variantId, CancellationToken cancellationToken = default);
    Task AddAsync(InventoryItem item, CancellationToken cancellationToken = default);
    Task UpdateAsync(InventoryItem item, CancellationToken cancellationToken = default);
}
