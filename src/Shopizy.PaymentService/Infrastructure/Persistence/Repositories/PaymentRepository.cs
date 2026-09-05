using Microsoft.EntityFrameworkCore;
using Shopizy.PaymentService.Application.Interfaces;
using Shopizy.PaymentService.Domain.Entities;

namespace Shopizy.PaymentService.Infrastructure.Persistence.Repositories;

public sealed class PaymentRepository : IPaymentRepository
{
    private readonly PaymentDbContext _dbContext;

    public PaymentRepository(PaymentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PaymentTransaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Payments
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<PaymentTransaction?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Payments
            .FirstOrDefaultAsync(p => p.OrderId == orderId, cancellationToken);
    }

    public async Task<IReadOnlyList<PaymentTransaction>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Payments
            .Where(p => p.CustomerId == customerId)
            .OrderByDescending(p => p.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PaymentTransaction>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Payments
            .OrderByDescending(p => p.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(PaymentTransaction transaction, CancellationToken cancellationToken = default)
    {
        await _dbContext.Payments.AddAsync(transaction, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(PaymentTransaction transaction, CancellationToken cancellationToken = default)
    {
        if (_dbContext.Entry(transaction).State == EntityState.Detached)
        {
            _dbContext.Payments.Update(transaction);
        }
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
