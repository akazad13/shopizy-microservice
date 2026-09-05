namespace Shopizy.CartAbandonmentWorker.Application.Interfaces;

public interface INotificationDispatcherClient
{
    Task<bool> DispatchRecoveryEmailAsync(string email, string restoreUrl, decimal cartTotal);
    IReadOnlyList<(string Email, string RestoreUrl, decimal CartTotal)> DispatchedNotifications { get; }
}
