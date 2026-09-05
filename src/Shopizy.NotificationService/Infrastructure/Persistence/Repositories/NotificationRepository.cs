using Microsoft.EntityFrameworkCore;
using Shopizy.NotificationService.Application.Interfaces;
using Shopizy.NotificationService.Domain.Entities;

namespace Shopizy.NotificationService.Infrastructure.Persistence.Repositories;

public sealed class NotificationRepository : INotificationRepository
{
    private readonly NotificationDbContext _dbContext;

    public NotificationRepository(NotificationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Notification?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbContext.Notifications.FirstOrDefaultAsync(n => n.Id == id, ct);
    }

    public async Task<IReadOnlyList<Notification>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await _dbContext.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAtUtc)
            .ToListAsync(ct);
    }

    public async Task AddAsync(Notification notification, CancellationToken ct = default)
    {
        await _dbContext.Notifications.AddAsync(notification, ct);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Notification notification, CancellationToken ct = default)
    {
        if (_dbContext.Entry(notification).State == EntityState.Detached)
        {
            _dbContext.Notifications.Update(notification);
        }
        await _dbContext.SaveChangesAsync(ct);
    }
}
