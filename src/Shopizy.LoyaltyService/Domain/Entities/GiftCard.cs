using Shopizy.LoyaltyService.Domain.Enums;
using Shopizy.LoyaltyService.Domain.Exceptions;

namespace Shopizy.LoyaltyService.Domain.Entities;

public class GiftCard
{
    public Guid Id { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public decimal InitialBalance { get; private set; }
    public decimal CurrentBalance { get; private set; }
    public string Currency { get; private set; } = "USD";
    public GiftCardStatus Status { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? ExpiresAtUtc { get; private set; }

    private GiftCard() { }

    public static GiftCard Create(decimal initialBalance, string currency = "USD", DateTime? expiresAtUtc = null, string? customCode = null)
    {
        if (initialBalance <= 0)
            throw new LoyaltyDomainException("INVALID_BALANCE", "Initial gift card balance must be positive.");

        var code = customCode ?? GenerateCode();

        return new GiftCard
        {
            Id = Guid.NewGuid(),
            Code = code.ToUpperInvariant().Trim(),
            InitialBalance = initialBalance,
            CurrentBalance = initialBalance,
            Currency = string.IsNullOrWhiteSpace(currency) ? "USD" : currency.ToUpperInvariant().Trim(),
            Status = GiftCardStatus.Active,
            CreatedAtUtc = DateTime.UtcNow,
            ExpiresAtUtc = expiresAtUtc
        };
    }

    public decimal DeductBalance(decimal amount)
    {
        if (amount <= 0)
            throw new LoyaltyDomainException("INVALID_DEDUCTION", "Deduction amount must be greater than zero.");

        if (Status != GiftCardStatus.Active)
            throw new LoyaltyDomainException("CARD_NOT_ACTIVE", $"Gift card is not active. Status: {Status}.");

        if (ExpiresAtUtc.HasValue && ExpiresAtUtc.Value < DateTime.UtcNow)
        {
            Status = GiftCardStatus.Expired;
            throw new LoyaltyDomainException("CARD_EXPIRED", "Gift card has expired.");
        }

        if (amount > CurrentBalance)
            throw new LoyaltyDomainException("INSUFFICIENT_BALANCE", $"Cannot deduct {amount}. Remaining balance is {CurrentBalance}.");

        CurrentBalance -= amount;

        if (CurrentBalance == 0)
        {
            Status = GiftCardStatus.Depleted;
        }

        return CurrentBalance;
    }

    private static string GenerateCode()
    {
        return Guid.NewGuid().ToString("N")[..16].ToUpperInvariant();
    }
}
