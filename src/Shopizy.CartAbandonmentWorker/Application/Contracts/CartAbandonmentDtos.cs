namespace Shopizy.CartAbandonmentWorker.Application.Contracts;

public record CartSnapshotDto(
    Guid CartId,
    Guid CustomerId,
    string CustomerEmail,
    decimal CartTotal,
    int ItemCount,
    DateTime LastActivityUtc,
    string ItemsJson);

public record AbandonmentSweepResult(
    int CartsEvaluated,
    int RecoveriesDispatched,
    int SuppressedByCooldown,
    DateTime TimestampUtc);

public record CartRecoveryRecordResponse(
    Guid Id,
    Guid CartId,
    Guid CustomerId,
    string CustomerEmail,
    decimal CartTotal,
    DateTime LastActivityUtc,
    string RecoveryToken,
    DateTime DispatchedAtUtc,
    bool IsRestored,
    DateTime? RestoredAtUtc);

public record RestoreCartResponse(
    Guid CartId,
    Guid CustomerId,
    string ItemsJson,
    bool Expired);
