using Shopizy.LoyaltyService.Domain.Enums;
using Shopizy.LoyaltyService.Domain.Exceptions;

namespace Shopizy.LoyaltyService.Domain.Entities;

public class LoyaltyAccount
{
    private readonly List<LoyaltyTransaction> _transactions = new();

    public Guid Id { get; private set; }
    public Guid CustomerId { get; private set; }
    public int PointsBalance { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }
    public IReadOnlyCollection<LoyaltyTransaction> Transactions => _transactions.AsReadOnly();

    private LoyaltyAccount() { }

    public static LoyaltyAccount Create(Guid customerId)
    {
        if (customerId == Guid.Empty)
            throw new LoyaltyDomainException("INVALID_CUSTOMER_ID", "CustomerId cannot be empty.");

        return new LoyaltyAccount
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            PointsBalance = 0,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public LoyaltyTransaction AccruePoints(int points, Guid? orderId, string description)
    {
        if (points <= 0)
            throw new LoyaltyDomainException("INVALID_POINTS", "Points to accrue must be greater than zero.");

        PointsBalance += points;
        UpdatedAtUtc = DateTime.UtcNow;

        var tx = LoyaltyTransaction.Create(Id, LoyaltyTransactionType.Accrual, points, orderId, description);
        _transactions.Add(tx);
        return tx;
    }

    public LoyaltyTransaction RedeemPoints(int points, Guid? orderId, string description)
    {
        if (points <= 0)
            throw new LoyaltyDomainException("INVALID_POINTS", "Points to redeem must be greater than zero.");

        if (points > PointsBalance)
            throw new LoyaltyDomainException("INSUFFICIENT_POINTS", $"Cannot redeem {points} points. Current balance is {PointsBalance}.");

        PointsBalance -= points;
        UpdatedAtUtc = DateTime.UtcNow;

        var tx = LoyaltyTransaction.Create(Id, LoyaltyTransactionType.Redemption, -points, orderId, description);
        _transactions.Add(tx);
        return tx;
    }
}
