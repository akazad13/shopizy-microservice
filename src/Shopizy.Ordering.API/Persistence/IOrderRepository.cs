using Shopizy.Ordering.API.Aggregates.ValueObjects;
using Shopizy.Ordering.API.Aggregates;

namespace Shopizy.Ordering.API.Persistence;

public interface IOrderRepository
{
    Task<List<Order>> GetOrdersAsync();
    Task<Order?> GetOrderByIdAsync(OrderId id);
    Task AddAsync(Order order);
    void Update(Order order);
    Task<int> Commit(CancellationToken cancellationToken);
}