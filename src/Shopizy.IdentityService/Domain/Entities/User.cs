using Shopizy.IdentityService.Domain.Enums;
using Shopizy.IdentityService.Domain.Events;
using Shopizy.IdentityService.Domain.ValueObjects;
using Shopizy.SharedKernel.Domain;

namespace Shopizy.IdentityService.Domain.Entities;

public sealed class User : AggregateRoot<Guid>
{
    private readonly List<RefreshToken> _refreshTokens = new();

    public Email Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = string.Empty;
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public UserRole Role { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }

    public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();

    private User() { } // EF Core

    private User(
        Guid id,
        string firstName,
        string lastName,
        Email email,
        string passwordHash,
        UserRole role,
        bool isActive,
        DateTime createdAtUtc) : base(id)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        PasswordHash = passwordHash;
        Role = role;
        IsActive = isActive;
        CreatedAtUtc = createdAtUtc;
    }

    public static User Create(
        string firstName,
        string lastName,
        Email email,
        string passwordHash,
        UserRole role = UserRole.Customer)
    {
        var user = new User(
            Guid.NewGuid(),
            firstName,
            lastName,
            email,
            passwordHash,
            role,
            isActive: true,
            DateTime.UtcNow);

        user.RaiseDomainEvent(new UserRegisteredDomainEvent(
            user.Id,
            user.Email.Value,
            user.FirstName,
            user.LastName,
            user.Role));

        return user;
    }

    public RefreshToken AddRefreshToken(string token, DateTime expiresAtUtc)
    {
        var refreshToken = RefreshToken.Create(Id, token, expiresAtUtc);
        _refreshTokens.Add(refreshToken);
        UpdatedAtUtc = DateTime.UtcNow;
        return refreshToken;
    }

    public void RevokeRefreshToken(string token)
    {
        var target = _refreshTokens.FirstOrDefault(rt => rt.Token == token);
        if (target is not null && !target.IsRevoked)
        {
            target.Revoke();
            UpdatedAtUtc = DateTime.UtcNow;
        }
    }

    public void UpdateProfile(string firstName, string lastName)
    {
        FirstName = firstName;
        LastName = lastName;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void UpdatePassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
