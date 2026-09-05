using Shopizy.PromotionService.Application.Contracts;
using Shopizy.PromotionService.Application.Interfaces;
using Shopizy.PromotionService.Domain.Entities;
using Shopizy.PromotionService.Domain.Enums;

namespace Shopizy.PromotionService.Infrastructure.Calculators;

public sealed class DefaultPromotionCalculator : IPromotionCalculator
{
    public PromotionEvaluationResult Evaluate(PromotionCampaign campaign, ApplyPromotionRequest request)
    {
        var now = DateTimeOffset.UtcNow;

        if (!campaign.IsActive)
            return new PromotionEvaluationResult(false, 0m, "Promotion is inactive.", null);

        if (now < campaign.StartsAtUtc || now > campaign.EndsAtUtc)
            return new PromotionEvaluationResult(false, 0m, "Promotion is outside its active window.", null);

        if (campaign.MaxGlobalUsages.HasValue && campaign.CurrentUsageCount >= campaign.MaxGlobalUsages.Value)
            return new PromotionEvaluationResult(false, 0m, "Promotion usage limit has been reached.", null);

        if (campaign.MinimumSpend.HasValue && request.Subtotal < campaign.MinimumSpend.Value)
            return new PromotionEvaluationResult(false, 0m, $"Minimum subtotal of {campaign.MinimumSpend.Value:C} required.", null);

        // Filter eligible items
        var eligibleItems = string.IsNullOrWhiteSpace(campaign.EligibleCategory)
            ? request.Items
            : request.Items.Where(i => string.Equals(i.Category, campaign.EligibleCategory, StringComparison.OrdinalIgnoreCase)).ToList();

        if (!string.IsNullOrWhiteSpace(campaign.EligibleCategory) && !eligibleItems.Any())
            return new PromotionEvaluationResult(false, 0m, $"No items in basket qualify for category '{campaign.EligibleCategory}'.", null);

        decimal calculatedDiscount = 0m;
        string ruleDesc = string.Empty;

        switch (campaign.DiscountType)
        {
            case DiscountType.Percentage:
                decimal eligibleSubtotal = eligibleItems.Sum(i => i.UnitPrice * i.Quantity);
                calculatedDiscount = Math.Round(eligibleSubtotal * (campaign.DiscountValue / 100m), 2);
                ruleDesc = $"{campaign.DiscountValue}% off eligible subtotal";

                if (campaign.MaxDiscountCap.HasValue && calculatedDiscount > campaign.MaxDiscountCap.Value)
                {
                    calculatedDiscount = campaign.MaxDiscountCap.Value;
                    ruleDesc += $" (capped at {campaign.MaxDiscountCap.Value:C})";
                }
                break;

            case DiscountType.FixedAmount:
                calculatedDiscount = Math.Min(campaign.DiscountValue, request.Subtotal);
                ruleDesc = $"{campaign.DiscountValue:C} fixed discount";
                break;

            case DiscountType.Bogo:
                // Buy 2 get 1 free on eligible items
                var expandedUnits = eligibleItems.SelectMany(i => Enumerable.Repeat(i.UnitPrice, i.Quantity)).OrderBy(p => p).ToList();
                int freeCount = expandedUnits.Count / 3;
                if (freeCount > 0)
                {
                    calculatedDiscount = expandedUnits.Take(freeCount).Sum();
                    ruleDesc = $"BOGO: {freeCount} free item(s)";
                }
                else
                {
                    return new PromotionEvaluationResult(false, 0m, "Need at least 3 qualifying items for Buy-2-Get-1 offer.", null);
                }
                break;
        }

        return new PromotionEvaluationResult(true, calculatedDiscount, null, ruleDesc);
    }
}
