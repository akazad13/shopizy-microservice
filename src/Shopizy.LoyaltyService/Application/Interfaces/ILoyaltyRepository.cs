using Shopizy.LoyaltyService.Domain.Entities;

namespace Shopizy.LoyaltyService.Application.Interfaces;

public interface ILoyaltyRepository
{
    Task<LoyaltyAccount?> GetByCustomerIdAsync(Guid customerId);
    Task AddAsync(LoyaltyAccount account);
    Task UpdateAsync(LoyaltyAccount account);
}
