using Microsoft.EntityFrameworkCore;
using Shopizy.ShippingService.Application.Interfaces;
using Shopizy.ShippingService.Domain.Entities;

namespace Shopizy.ShippingService.Infrastructure.Persistence.Repositories;

public sealed class ShipmentRepository : IShipmentRepository
{
    private readonly ShippingDbContext _dbContext;

    public ShipmentRepository(ShippingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Shipment?> GetByTrackingNumberAsync(string trackingNumber, CancellationToken ct = default)
    {
        return await _dbContext.Shipments
            .Include(s => s.Milestones)
            .FirstOrDefaultAsync(s => s.TrackingNumber == trackingNumber, ct);
    }

    public async Task<Shipment?> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default)
    {
        return await _dbContext.Shipments
            .Include(s => s.Milestones)
            .FirstOrDefaultAsync(s => s.OrderId == orderId, ct);
    }

    public async Task AddAsync(Shipment shipment, CancellationToken ct = default)
    {
        await _dbContext.Shipments.AddAsync(shipment, ct);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Shipment shipment, CancellationToken ct = default)
    {
        var existingMilestoneIds = await _dbContext.ShipmentMilestones
            .Where(m => m.ShipmentId == shipment.Id)
            .Select(m => m.Id)
            .ToListAsync(ct);

        foreach (var milestone in shipment.Milestones)
        {
            if (!existingMilestoneIds.Contains(milestone.Id))
            {
                _dbContext.Entry(milestone).State = EntityState.Added;
            }
            else
            {
                _dbContext.Entry(milestone).State = EntityState.Unchanged;
            }
        }

        _dbContext.Entry(shipment).State = EntityState.Modified;

        await _dbContext.SaveChangesAsync(ct);
    }
}
