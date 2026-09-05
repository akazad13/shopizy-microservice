using Shopizy.PromotionService.Domain.Enums;
using Shopizy.PromotionService.Domain.Exceptions;

namespace Shopizy.PromotionService.Domain.Entities;

public sealed class PromotionCampaign
{
    public Guid Id { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public DiscountType DiscountType { get; private set; }
    public decimal DiscountValue { get; private set; } // e.g. 20 for 20%, 15 for $15
    public decimal? MinimumSpend { get; private set; }
    public decimal? MaxDiscountCap { get; private set; } // Safety cap ceiling
    public string? EligibleCategory { get; private set; }
    public int? MaxGlobalUsages { get; private set; }
    public int CurrentUsageCount { get; private set; }
    public DateTimeOffset StartsAtUtc { get; private set; }
    public DateTimeOffset EndsAtUtc { get; private set; }
    public bool IsActive { get; private set; }

    private PromotionCampaign() { }

    public static PromotionCampaign Create(
        Guid id,
        string code,
        string description,
        DiscountType discountType,
        decimal discountValue,
        decimal? minimumSpend,
        decimal? maxDiscountCap,
        string? eligibleCategory,
        int? maxGlobalUsages,
        DateTimeOffset startsAtUtc,
        DateTimeOffset endsAtUtc)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new PromotionDomainException("Promotion.InvalidCode", "Coupon code is required.");

        if (discountValue <= 0)
            throw new PromotionDomainException("Promotion.InvalidValue", "Discount value must be greater than zero.");

        if (endsAtUtc <= startsAtUtc)
            throw new PromotionDomainException("Promotion.InvalidDates", "End date must be after start date.");

        return new PromotionCampaign
        {
            Id = id,
            Code = code.Trim().ToUpperInvariant(),
            Description = description,
            DiscountType = discountType,
            DiscountValue = discountValue,
            MinimumSpend = minimumSpend,
            MaxDiscountCap = maxDiscountCap,
            EligibleCategory = eligibleCategory,
            MaxGlobalUsages = maxGlobalUsages,
            CurrentUsageCount = 0,
            StartsAtUtc = startsAtUtc,
            EndsAtUtc = endsAtUtc,
            IsActive = true
        };
    }

    public void IncrementUsage()
    {
        if (MaxGlobalUsages.HasValue && CurrentUsageCount >= MaxGlobalUsages.Value)
            throw new PromotionDomainException("Promotion.CouponExhausted", "Coupon usage limit reached.");

        CurrentUsageCount++;
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}
