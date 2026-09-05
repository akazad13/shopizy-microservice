using Microsoft.EntityFrameworkCore;
using Shopizy.OrderService.Application.Interfaces;
using Shopizy.OrderService.Domain.Entities;

namespace Shopizy.OrderService.Infrastructure.Persistence.Repositories;

public sealed class InventoryRepository : IInventoryRepository
{
    private readonly OrderDbContext _dbContext;

    public InventoryRepository(OrderDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<InventoryItem?> GetByVariantIdAsync(Guid variantId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Inventory.FirstOrDefaultAsync(i => i.Id == variantId, cancellationToken);
    }

    public async Task AddAsync(InventoryItem item, CancellationToken cancellationToken = default)
    {
        await _dbContext.Inventory.AddAsync(item, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(InventoryItem item, CancellationToken cancellationToken = default)
    {
        _dbContext.Inventory.Update(item);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
