using Shopizy.PromotionService.Application.Contracts;
using Shopizy.PromotionService.Application.Interfaces;
using Shopizy.PromotionService.Domain.Entities;
using Shopizy.PromotionService.Domain.Exceptions;

namespace Shopizy.PromotionService.Application.Services;

public sealed class PromotionApplicationService
{
    private readonly IPromotionRepository _repo;
    private readonly IPromotionCalculator _calculator;

    public PromotionApplicationService(IPromotionRepository repo, IPromotionCalculator calculator)
    {
        _repo = repo;
        _calculator = calculator;
    }

    public async Task<PromotionEvaluationResult> ApplyPromotionAsync(ApplyPromotionRequest request, CancellationToken ct = default)
    {
        var campaign = await _repo.GetByCodeAsync(request.CouponCode, ct);
        if (campaign is null)
        {
            return new PromotionEvaluationResult(false, 0m, $"Coupon code '{request.CouponCode}' is invalid.", null);
        }

        var result = _calculator.Evaluate(campaign, request);
        return result;
    }

    public async Task<CampaignResponse> CreateCampaignAsync(CreateCampaignRequest request, CancellationToken ct = default)
    {
        var existing = await _repo.GetByCodeAsync(request.Code, ct);
        if (existing is not null)
            throw new PromotionDomainException("Promotion.DuplicateCode", $"Coupon code '{request.Code}' already exists.");

        var campaign = PromotionCampaign.Create(
            Guid.NewGuid(),
            request.Code,
            request.Description,
            request.DiscountType,
            request.DiscountValue,
            request.MinimumSpend,
            request.MaxDiscountCap,
            request.EligibleCategory,
            request.MaxGlobalUsages,
            request.StartsAtUtc,
            request.EndsAtUtc);

        await _repo.AddAsync(campaign, ct);
        return ToResponse(campaign);
    }

    public async Task RecordUsageAsync(string code, CancellationToken ct = default)
    {
        var campaign = await _repo.GetByCodeAsync(code, ct);
        if (campaign is not null)
        {
            campaign.IncrementUsage();
            await _repo.UpdateAsync(campaign, ct);
        }
    }

    public async Task<IReadOnlyList<CampaignResponse>> GetCampaignsAsync(CancellationToken ct = default)
    {
        var list = await _repo.GetAllAsync(ct);
        return list.Select(ToResponse).ToList();
    }

    public async Task DeactivateCampaignAsync(Guid id, CancellationToken ct = default)
    {
        var campaign = await _repo.GetByIdAsync(id, ct)
            ?? throw new PromotionDomainException("Promotion.NotFound", "Campaign not found.");

        campaign.Deactivate();
        await _repo.UpdateAsync(campaign, ct);
    }

    private static CampaignResponse ToResponse(PromotionCampaign c) => new(
        c.Id,
        c.Code,
        c.Description,
        c.DiscountType,
        c.DiscountValue,
        c.MinimumSpend,
        c.MaxDiscountCap,
        c.EligibleCategory,
        c.MaxGlobalUsages,
        c.CurrentUsageCount,
        c.StartsAtUtc,
        c.EndsAtUtc,
        c.IsActive);
}
