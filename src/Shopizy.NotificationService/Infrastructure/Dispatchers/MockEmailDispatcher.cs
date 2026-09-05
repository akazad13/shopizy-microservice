using Shopizy.NotificationService.Application.Interfaces;
using Shopizy.NotificationService.Domain.Entities;

namespace Shopizy.NotificationService.Infrastructure.Dispatchers;

public sealed class MockEmailDispatcher : INotificationDispatcher
{
    private readonly List<Notification> _dispatched = new();
    public IReadOnlyList<Notification> Dispatched => _dispatched.AsReadOnly();

    public Task<bool> DispatchAsync(Notification notification, CancellationToken ct = default)
    {
        _dispatched.Add(notification);
        return Task.FromResult(true);
    }
}
