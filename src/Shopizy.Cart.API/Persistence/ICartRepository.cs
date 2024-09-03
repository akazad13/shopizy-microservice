using Shopizy.Cart.API.Aggregates;
using Shopizy.Cart.API.Aggregates.ValueObjects;

namespace Shopizy.Cart.API.Persistence;

public interface ICartRepository
{
    Task<List<CustomerCart>> GetCartsAsync();
    Task<CustomerCart?> GetCartByIdAsync(CartId id);
    Task<CustomerCart?> GetCartByUserIdAsync(CustomerId id);
    Task AddAsync(CustomerCart cart);
    void Update(CustomerCart cart);
    void Remove(CustomerCart cart);
    Task<int> Commit(CancellationToken cancellationToken);
}
