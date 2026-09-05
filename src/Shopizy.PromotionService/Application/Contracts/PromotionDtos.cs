using Shopizy.PromotionService.Domain.Enums;

namespace Shopizy.PromotionService.Application.Contracts;

public sealed record CartItemDto(
    Guid ProductId,
    string Title,
    string Category,
    decimal UnitPrice,
    int Quantity);

public sealed record ApplyPromotionRequest(
    string CouponCode,
    decimal Subtotal,
    string Currency,
    List<CartItemDto> Items);

public sealed record PromotionEvaluationResult(
    bool IsValid,
    decimal DiscountAmount,
    string? FailureReason,
    string? AppliedRuleDescription);

public sealed record CreateCampaignRequest(
    string Code,
    string Description,
    DiscountType DiscountType,
    decimal DiscountValue,
    decimal? MinimumSpend,
    decimal? MaxDiscountCap,
    string? EligibleCategory,
    int? MaxGlobalUsages,
    DateTimeOffset StartsAtUtc,
    DateTimeOffset EndsAtUtc);

public sealed record CampaignResponse(
    Guid Id,
    string Code,
    string Description,
    DiscountType DiscountType,
    decimal DiscountValue,
    decimal? MinimumSpend,
    decimal? MaxDiscountCap,
    string? EligibleCategory,
    int? MaxGlobalUsages,
    int CurrentUsageCount,
    DateTimeOffset StartsAtUtc,
    DateTimeOffset EndsAtUtc,
    bool IsActive);
