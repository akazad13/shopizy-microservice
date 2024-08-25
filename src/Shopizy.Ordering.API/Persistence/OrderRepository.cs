using Microsoft.EntityFrameworkCore;
using Shopizy.Domain.Models.Specifications;
using Shopizy.Ordering.API.Aggregates;
using Shopizy.Ordering.API.Aggregates.ValueObjects;
using Shopizy.Ordering.API.Persistence.Specifications;

namespace Shopizy.Ordering.API.Persistence;

public class OrderRepository(OrderDbContext dbContext) : IOrderRepository
{
    private readonly OrderDbContext _dbContext = dbContext;
    public Task<List<Order>> GetOrdersAsync()
    {
        return _dbContext.Orders.AsNoTracking().ToListAsync();
    }
    public Task<Order?> GetOrderByIdAsync(OrderId id)
    {
        return ApplySpec(new OrderByIdSpec(id)).FirstOrDefaultAsync();
    }
    public async Task AddAsync(Order order)
    {
        await _dbContext.Orders.AddAsync(order);
    }
    public void Update(Order order)
    {
        _dbContext.Update(order);
    }

    public Task<int> Commit(CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<Order> ApplySpec(Specification<Order> spec)
    {
        return SpecificationEvaluator.GetQuery(_dbContext.Orders, spec);
    }
}