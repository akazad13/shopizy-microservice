using Shopizy.CartAbandonmentWorker.Application.Contracts;
using Shopizy.CartAbandonmentWorker.Application.Interfaces;
using Shopizy.CartAbandonmentWorker.Domain.Entities;
using Shopizy.CartAbandonmentWorker.Domain.Services;

namespace Shopizy.CartAbandonmentWorker.Application.Services;

public class CartAbandonmentApplicationService
{
    private readonly IAbandonedCartRepository _repository;
    private readonly ICartSnapshotClient _cartClient;
    private readonly INotificationDispatcherClient _notificationClient;
    private const string BaseStoreUrl = "https://shopizy.com";

    public CartAbandonmentApplicationService(
        IAbandonedCartRepository repository,
        ICartSnapshotClient cartClient,
        INotificationDispatcherClient notificationClient)
    {
        _repository = repository;
        _cartClient = cartClient;
        _notificationClient = notificationClient;
    }

    public async Task<AbandonmentSweepResult> RunAbandonmentSweepAsync(DateTime? nowUtcOverride = null)
    {
        var now = nowUtcOverride ?? DateTime.UtcNow;
        var carts = await _cartClient.GetActiveCartsAsync();

        int evaluated = 0;
        int dispatched = 0;
        int suppressed = 0;

        foreach (var cart in carts)
        {
            evaluated++;

            // 1. Inactivity check (>= 2h) & non-empty items
            if (!AbandonmentPolicy.IsAbandoned(cart.LastActivityUtc, cart.ItemCount, now))
            {
                continue;
            }

            // 2. Cooldown check (< 24h since previous dispatch for this cart)
            var latestRecord = await _repository.GetLatestByCartIdAsync(cart.CartId);
            if (latestRecord != null && AbandonmentPolicy.IsInCooldown(latestRecord.DispatchedAtUtc, now))
            {
                suppressed++;
                continue;
            }

            // 3. Create record & dispatch
            var record = AbandonedCartRecord.Create(
                cart.CartId,
                cart.CustomerId,
                cart.CustomerEmail,
                cart.CartTotal,
                cart.ItemsJson,
                cart.LastActivityUtc);

            await _repository.AddAsync(record);

            var restoreUrl = AbandonmentPolicy.FormatRecoveryUrl(BaseStoreUrl, record.RecoveryToken);
            await _notificationClient.DispatchRecoveryEmailAsync(record.CustomerEmail, restoreUrl, record.CartTotal);
            dispatched++;
        }

        return new AbandonmentSweepResult(evaluated, dispatched, suppressed, now);
    }

    public async Task<RestoreCartResponse?> RestoreCartAsync(string token)
    {
        var record = await _repository.GetByTokenAsync(token);
        if (record == null) return null;

        record.MarkAsRestored();
        await _repository.UpdateAsync(record);

        return new RestoreCartResponse(record.CartId, record.CustomerId, record.ItemsJson, Expired: false);
    }

    public async Task<List<CartRecoveryRecordResponse>> GetRecordsByCustomerIdAsync(Guid customerId)
    {
        var records = await _repository.GetByCustomerIdAsync(customerId);
        return records.Select(r => new CartRecoveryRecordResponse(
            r.Id, r.CartId, r.CustomerId, r.CustomerEmail, r.CartTotal,
            r.LastActivityUtc, r.RecoveryToken, r.DispatchedAtUtc, r.IsRestored, r.RestoredAtUtc)).ToList();
    }
}
