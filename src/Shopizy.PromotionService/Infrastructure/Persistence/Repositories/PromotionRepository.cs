using Microsoft.EntityFrameworkCore;
using Shopizy.PromotionService.Application.Interfaces;
using Shopizy.PromotionService.Domain.Entities;

namespace Shopizy.PromotionService.Infrastructure.Persistence.Repositories;

public sealed class PromotionRepository : IPromotionRepository
{
    private readonly PromotionDbContext _dbContext;

    public PromotionRepository(PromotionDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PromotionCampaign?> GetByCodeAsync(string code, CancellationToken ct = default)
    {
        var normalized = code.Trim().ToUpperInvariant();
        return await _dbContext.Campaigns.FirstOrDefaultAsync(c => c.Code == normalized, ct);
    }

    public async Task<PromotionCampaign?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbContext.Campaigns.FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public async Task<IReadOnlyList<PromotionCampaign>> GetAllAsync(CancellationToken ct = default)
    {
        return await _dbContext.Campaigns.AsNoTracking().ToListAsync(ct);
    }

    public async Task AddAsync(PromotionCampaign campaign, CancellationToken ct = default)
    {
        await _dbContext.Campaigns.AddAsync(campaign, ct);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(PromotionCampaign campaign, CancellationToken ct = default)
    {
        if (_dbContext.Entry(campaign).State == EntityState.Detached)
        {
            _dbContext.Campaigns.Update(campaign);
        }
        await _dbContext.SaveChangesAsync(ct);
    }
}
