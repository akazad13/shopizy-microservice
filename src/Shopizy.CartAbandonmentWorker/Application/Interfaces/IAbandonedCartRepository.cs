using Shopizy.CartAbandonmentWorker.Domain.Entities;

namespace Shopizy.CartAbandonmentWorker.Application.Interfaces;

public interface IAbandonedCartRepository
{
    Task<AbandonedCartRecord?> GetByTokenAsync(string token);
    Task<AbandonedCartRecord?> GetLatestByCartIdAsync(Guid cartId);
    Task<List<AbandonedCartRecord>> GetByCustomerIdAsync(Guid customerId);
    Task AddAsync(AbandonedCartRecord record);
    Task UpdateAsync(AbandonedCartRecord record);
}
