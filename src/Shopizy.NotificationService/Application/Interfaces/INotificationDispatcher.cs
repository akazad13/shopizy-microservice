using Shopizy.NotificationService.Domain.Entities;

namespace Shopizy.NotificationService.Application.Interfaces;

public interface INotificationDispatcher
{
    Task<bool> DispatchAsync(Notification notification, CancellationToken ct = default);
}
