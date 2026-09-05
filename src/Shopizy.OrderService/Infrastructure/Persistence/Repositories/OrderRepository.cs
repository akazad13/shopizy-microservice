using Microsoft.EntityFrameworkCore;
using Shopizy.OrderService.Application.Interfaces;
using Shopizy.OrderService.Domain.Entities;
using Shopizy.OrderService.Domain.Enums;

namespace Shopizy.OrderService.Infrastructure.Persistence.Repositories;

public sealed class OrderRepository : IOrderRepository
{
    private readonly OrderDbContext _dbContext;

    public OrderRepository(OrderDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Order>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Orders
            .Include(o => o.Items)
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Order>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Orders
            .Include(o => o.Items)
            .OrderByDescending(o => o.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Order>> GetExpiredPendingOrdersAsync(DateTimeOffset asOf, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Orders
            .Include(o => o.Items)
            .Where(o => o.Status == OrderStatus.PendingPayment && o.ExpiresAtUtc < asOf)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Order order, CancellationToken cancellationToken = default)
    {
        await _dbContext.Orders.AddAsync(order, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Order order, CancellationToken cancellationToken = default)
    {
        _dbContext.Orders.Update(order);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
