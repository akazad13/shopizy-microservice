using Shopizy.Ordering.API.Aggregates;
using Shopizy.Ordering.API.Aggregates.ValueObjects;

namespace Shopizy.Ordering.API.Persistence;

public interface IOrderRepository
{
    Task<List<Order>> GetOrdersAsync();
    Task<Order?> GetOrderByIdAsync(OrderId id);
    Task AddAsync(Order order);
    void Update(Order order);
    Task<int> Commit(CancellationToken cancellationToken);
}
