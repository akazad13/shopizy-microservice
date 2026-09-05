namespace Shopizy.CartAbandonmentWorker.Domain.Services;

public static class AbandonmentPolicy
{
    public static readonly TimeSpan InactivityThreshold = TimeSpan.FromHours(2);
    public static readonly TimeSpan DeduplicationCooldown = TimeSpan.FromHours(24);

    public static bool IsAbandoned(DateTime lastActivityUtc, int itemCount, DateTime nowUtc)
    {
        if (itemCount <= 0) return false;
        return (nowUtc - lastActivityUtc) >= InactivityThreshold;
    }

    public static bool IsInCooldown(DateTime? lastDispatchedUtc, DateTime nowUtc)
    {
        if (!lastDispatchedUtc.HasValue) return false;
        return (nowUtc - lastDispatchedUtc.Value) < DeduplicationCooldown;
    }

    public static string FormatRecoveryUrl(string baseUrl, string recoveryToken)
    {
        var cleanedBase = baseUrl.TrimEnd('/');
        return $"{cleanedBase}/cart/restore/{recoveryToken}";
    }
}
