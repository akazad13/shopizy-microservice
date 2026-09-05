using Microsoft.EntityFrameworkCore;
using Shopizy.LoyaltyService.Application.Interfaces;
using Shopizy.LoyaltyService.Domain.Entities;

namespace Shopizy.LoyaltyService.Infrastructure.Persistence.Repositories;

public class GiftCardRepository : IGiftCardRepository
{
    private readonly LoyaltyDbContext _context;

    public GiftCardRepository(LoyaltyDbContext context)
    {
        _context = context;
    }

    public async Task<GiftCard?> GetByCodeAsync(string code)
    {
        var normalized = code.ToUpperInvariant().Trim();
        return await _context.GiftCards.FirstOrDefaultAsync(g => g.Code == normalized);
    }

    public async Task<GiftCard?> GetByIdAsync(Guid id)
    {
        return await _context.GiftCards.FirstOrDefaultAsync(g => g.Id == id);
    }

    public async Task AddAsync(GiftCard giftCard)
    {
        await _context.GiftCards.AddAsync(giftCard);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(GiftCard giftCard)
    {
        _context.GiftCards.Update(giftCard);
        await _context.SaveChangesAsync();
    }
}
