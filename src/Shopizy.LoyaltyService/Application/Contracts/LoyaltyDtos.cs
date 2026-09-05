using Shopizy.LoyaltyService.Domain.Enums;

namespace Shopizy.LoyaltyService.Application.Contracts;

public record LoyaltyTransactionResponse(
    Guid Id,
    LoyaltyTransactionType Type,
    int Points,
    Guid? OrderId,
    string Description,
    DateTime CreatedAtUtc);

public record LoyaltyAccountResponse(
    Guid CustomerId,
    int PointsBalance,
    decimal CashEquivalentValue,
    List<LoyaltyTransactionResponse> Transactions);

public record AccruePointsRequest(
    Guid CustomerId,
    Guid OrderId,
    decimal OrderAmount);

public record RedeemPointsRequest(
    int PointsToRedeem,
    Guid OrderId);

public record PointsRedemptionResponse(
    int PointsRedeemed,
    decimal DiscountAmount,
    int RemainingPoints);

public record CreateGiftCardRequest(
    decimal InitialBalance,
    string Currency = "USD",
    DateTime? ExpiresAtUtc = null,
    string? CustomCode = null);

public record GiftCardResponse(
    Guid Id,
    string Code,
    decimal InitialBalance,
    decimal CurrentBalance,
    string Currency,
    GiftCardStatus Status,
    DateTime CreatedAtUtc,
    DateTime? ExpiresAtUtc);

public record GiftCardBalanceResponse(
    string Code,
    decimal CurrentBalance,
    string Currency,
    GiftCardStatus Status);

public record ApplyGiftCardRequest(
    string Code,
    decimal AmountToDeduct,
    Guid OrderId);

public record GiftCardDeductionResponse(
    string Code,
    decimal AmountDeducted,
    decimal RemainingBalance,
    GiftCardStatus Status);
