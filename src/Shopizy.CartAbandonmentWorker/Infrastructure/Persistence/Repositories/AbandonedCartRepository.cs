using Microsoft.EntityFrameworkCore;
using Shopizy.CartAbandonmentWorker.Application.Interfaces;
using Shopizy.CartAbandonmentWorker.Domain.Entities;

namespace Shopizy.CartAbandonmentWorker.Infrastructure.Persistence.Repositories;

public class AbandonedCartRepository : IAbandonedCartRepository
{
    private readonly AbandonmentDbContext _context;

    public AbandonedCartRepository(AbandonmentDbContext context)
    {
        _context = context;
    }

    public async Task<AbandonedCartRecord?> GetByTokenAsync(string token)
    {
        return await _context.AbandonedCartRecords.FirstOrDefaultAsync(r => r.RecoveryToken == token);
    }

    public async Task<AbandonedCartRecord?> GetLatestByCartIdAsync(Guid cartId)
    {
        return await _context.AbandonedCartRecords
            .Where(r => r.CartId == cartId)
            .OrderByDescending(r => r.DispatchedAtUtc)
            .FirstOrDefaultAsync();
    }

    public async Task<List<AbandonedCartRecord>> GetByCustomerIdAsync(Guid customerId)
    {
        return await _context.AbandonedCartRecords
            .Where(r => r.CustomerId == customerId)
            .OrderByDescending(r => r.DispatchedAtUtc)
            .ToListAsync();
    }

    public async Task AddAsync(AbandonedCartRecord record)
    {
        await _context.AbandonedCartRecords.AddAsync(record);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(AbandonedCartRecord record)
    {
        _context.AbandonedCartRecords.Update(record);
        await _context.SaveChangesAsync();
    }
}
