using Shopizy.SharedKernel.Domain;

namespace Shopizy.IdentityService.Domain.Entities;

public sealed class RefreshToken : Entity<Guid>
{
    public Guid UserId { get; private set; }
    public string Token { get; private set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? RevokedAtUtc { get; private set; }

    public bool IsRevoked => RevokedAtUtc.HasValue;
    public bool IsExpired => DateTime.UtcNow >= ExpiresAtUtc;
    public bool IsActive => !IsRevoked && !IsExpired;

    private RefreshToken() { } // EF Core

    public RefreshToken(Guid id, Guid userId, string token, DateTime expiresAtUtc, DateTime createdAtUtc)
        : base(id)
    {
        UserId = userId;
        Token = token;
        ExpiresAtUtc = expiresAtUtc;
        CreatedAtUtc = createdAtUtc;
    }

    public static RefreshToken Create(Guid userId, string token, DateTime expiresAtUtc)
    {
        return new RefreshToken(
            Guid.NewGuid(),
            userId,
            token,
            expiresAtUtc,
            DateTime.UtcNow);
    }

    public void Revoke(DateTime? utcNow = null)
    {
        RevokedAtUtc = utcNow ?? DateTime.UtcNow;
    }
}
