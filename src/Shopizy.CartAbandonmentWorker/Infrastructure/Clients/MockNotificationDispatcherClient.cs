using Shopizy.CartAbandonmentWorker.Application.Interfaces;

namespace Shopizy.CartAbandonmentWorker.Infrastructure.Clients;

public class MockNotificationDispatcherClient : INotificationDispatcherClient
{
    private readonly List<(string Email, string RestoreUrl, decimal CartTotal)> _dispatched = new();

    public IReadOnlyList<(string Email, string RestoreUrl, decimal CartTotal)> DispatchedNotifications => _dispatched.AsReadOnly();

    public Task<bool> DispatchRecoveryEmailAsync(string email, string restoreUrl, decimal cartTotal)
    {
        _dispatched.Add((email, restoreUrl, cartTotal));
        return Task.FromResult(true);
    }
}
