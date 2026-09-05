using Shopizy.LoyaltyService.Domain.Entities;

namespace Shopizy.LoyaltyService.Application.Interfaces;

public interface IGiftCardRepository
{
    Task<GiftCard?> GetByCodeAsync(string code);
    Task<GiftCard?> GetByIdAsync(Guid id);
    Task AddAsync(GiftCard giftCard);
    Task UpdateAsync(GiftCard giftCard);
}
