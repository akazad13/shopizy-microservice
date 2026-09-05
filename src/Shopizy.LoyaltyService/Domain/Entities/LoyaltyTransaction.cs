using Shopizy.LoyaltyService.Domain.Enums;

namespace Shopizy.LoyaltyService.Domain.Entities;

public class LoyaltyTransaction
{
    public Guid Id { get; private set; }
    public Guid LoyaltyAccountId { get; private set; }
    public LoyaltyTransactionType Type { get; private set; }
    public int Points { get; private set; }
    public Guid? OrderId { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public DateTime CreatedAtUtc { get; private set; }

    private LoyaltyTransaction() { }

    public static LoyaltyTransaction Create(
        Guid loyaltyAccountId,
        LoyaltyTransactionType type,
        int points,
        Guid? orderId,
        string description)
    {
        return new LoyaltyTransaction
        {
            Id = Guid.NewGuid(),
            LoyaltyAccountId = loyaltyAccountId,
            Type = type,
            Points = points,
            OrderId = orderId,
            Description = description,
            CreatedAtUtc = DateTime.UtcNow
        };
    }
}
