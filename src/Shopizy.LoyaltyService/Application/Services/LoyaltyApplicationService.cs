using Shopizy.LoyaltyService.Application.Contracts;
using Shopizy.LoyaltyService.Application.Interfaces;
using Shopizy.LoyaltyService.Domain.Entities;
using Shopizy.LoyaltyService.Domain.Exceptions;
using Shopizy.LoyaltyService.Domain.Services;

namespace Shopizy.LoyaltyService.Application.Services;

public class LoyaltyApplicationService
{
    private readonly ILoyaltyRepository _loyaltyRepository;
    private readonly IGiftCardRepository _giftCardRepository;

    public LoyaltyApplicationService(
        ILoyaltyRepository loyaltyRepository,
        IGiftCardRepository giftCardRepository)
    {
        _loyaltyRepository = loyaltyRepository;
        _giftCardRepository = giftCardRepository;
    }

    public async Task<LoyaltyAccountResponse> GetOrCreateAccountAsync(Guid customerId)
    {
        var account = await _loyaltyRepository.GetByCustomerIdAsync(customerId);
        if (account == null)
        {
            account = LoyaltyAccount.Create(customerId);
            await _loyaltyRepository.AddAsync(account);
        }

        return MapToAccountResponse(account);
    }

    public async Task<LoyaltyAccountResponse> AccruePointsAsync(AccruePointsRequest request)
    {
        var account = await _loyaltyRepository.GetByCustomerIdAsync(request.CustomerId);
        if (account == null)
        {
            account = LoyaltyAccount.Create(request.CustomerId);
            await _loyaltyRepository.AddAsync(account);
        }

        var points = LoyaltyCalculator.CalculatePointsEarned(request.OrderAmount);
        account.AccruePoints(points, request.OrderId, $"Earned on order {request.OrderId}");
        await _loyaltyRepository.UpdateAsync(account);

        return MapToAccountResponse(account);
    }

    public async Task<PointsRedemptionResponse> RedeemPointsAsync(Guid customerId, RedeemPointsRequest request)
    {
        var account = await _loyaltyRepository.GetByCustomerIdAsync(customerId);
        if (account == null)
        {
            account = LoyaltyAccount.Create(customerId);
            await _loyaltyRepository.AddAsync(account);
        }

        account.RedeemPoints(request.PointsToRedeem, request.OrderId, $"Redeemed on order {request.OrderId}");
        await _loyaltyRepository.UpdateAsync(account);

        var discount = LoyaltyCalculator.CalculateDiscount(request.PointsToRedeem);
        return new PointsRedemptionResponse(request.PointsToRedeem, discount, account.PointsBalance);
    }

    public async Task<GiftCardResponse> CreateGiftCardAsync(CreateGiftCardRequest request)
    {
        var card = GiftCard.Create(request.InitialBalance, request.Currency, request.ExpiresAtUtc, request.CustomCode);
        await _giftCardRepository.AddAsync(card);
        return MapToGiftCardResponse(card);
    }

    public async Task<GiftCardBalanceResponse> CheckGiftCardBalanceAsync(string code)
    {
        var card = await _giftCardRepository.GetByCodeAsync(code)
            ?? throw new LoyaltyDomainException("CARD_NOT_FOUND", "Gift card code not found.");

        return new GiftCardBalanceResponse(card.Code, card.CurrentBalance, card.Currency, card.Status);
    }

    public async Task<GiftCardDeductionResponse> ApplyGiftCardAsync(ApplyGiftCardRequest request)
    {
        var card = await _giftCardRepository.GetByCodeAsync(request.Code)
            ?? throw new LoyaltyDomainException("CARD_NOT_FOUND", "Gift card code not found.");

        card.DeductBalance(request.AmountToDeduct);
        await _giftCardRepository.UpdateAsync(card);

        return new GiftCardDeductionResponse(card.Code, request.AmountToDeduct, card.CurrentBalance, card.Status);
    }

    private static LoyaltyAccountResponse MapToAccountResponse(LoyaltyAccount a)
    {
        var cashEquiv = LoyaltyCalculator.CalculateDiscount(a.PointsBalance);
        var txs = a.Transactions.Select(t =>
            new LoyaltyTransactionResponse(t.Id, t.Type, t.Points, t.OrderId, t.Description, t.CreatedAtUtc))
            .OrderByDescending(t => t.CreatedAtUtc)
            .ToList();

        return new LoyaltyAccountResponse(a.CustomerId, a.PointsBalance, cashEquiv, txs);
    }

    private static GiftCardResponse MapToGiftCardResponse(GiftCard g) =>
        new(g.Id, g.Code, g.InitialBalance, g.CurrentBalance, g.Currency, g.Status, g.CreatedAtUtc, g.ExpiresAtUtc);
}
