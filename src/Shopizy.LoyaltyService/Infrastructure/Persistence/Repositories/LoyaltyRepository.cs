using Microsoft.EntityFrameworkCore;
using Shopizy.LoyaltyService.Application.Interfaces;
using Shopizy.LoyaltyService.Domain.Entities;

namespace Shopizy.LoyaltyService.Infrastructure.Persistence.Repositories;

public class LoyaltyRepository : ILoyaltyRepository
{
    private readonly LoyaltyDbContext _context;

    public LoyaltyRepository(LoyaltyDbContext context)
    {
        _context = context;
    }

    public async Task<LoyaltyAccount?> GetByCustomerIdAsync(Guid customerId)
    {
        return await _context.LoyaltyAccounts
            .Include(a => a.Transactions)
            .FirstOrDefaultAsync(a => a.CustomerId == customerId);
    }

    public async Task AddAsync(LoyaltyAccount account)
    {
        await _context.LoyaltyAccounts.AddAsync(account);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(LoyaltyAccount account)
    {
        var existingTxIds = await _context.LoyaltyTransactions
            .Where(t => t.LoyaltyAccountId == account.Id)
            .Select(t => t.Id)
            .ToListAsync();

        foreach (var tx in account.Transactions)
        {
            if (!existingTxIds.Contains(tx.Id))
            {
                _context.Entry(tx).State = EntityState.Added;
            }
            else
            {
                _context.Entry(tx).State = EntityState.Unchanged;
            }
        }

        _context.Entry(account).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }
}
