using Shopizy.PromotionService.Application.Contracts;
using Shopizy.PromotionService.Domain.Entities;

namespace Shopizy.PromotionService.Application.Interfaces;

public interface IPromotionRepository
{
    Task<PromotionCampaign?> GetByCodeAsync(string code, CancellationToken ct = default);
    Task<PromotionCampaign?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<PromotionCampaign>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(PromotionCampaign campaign, CancellationToken ct = default);
    Task UpdateAsync(PromotionCampaign campaign, CancellationToken ct = default);
}

public interface IPromotionCalculator
{
    PromotionEvaluationResult Evaluate(PromotionCampaign campaign, ApplyPromotionRequest request);
}
