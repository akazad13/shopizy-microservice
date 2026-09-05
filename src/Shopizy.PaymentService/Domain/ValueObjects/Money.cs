using Shopizy.PaymentService.Domain.Exceptions;
using Shopizy.SharedKernel.Domain;

namespace Shopizy.PaymentService.Domain.ValueObjects;

public sealed class Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }

    [System.Text.Json.Serialization.JsonConstructor]
    public Money(decimal amount, string currency = "USD")
    {
        Amount = decimal.Round(amount, 2, MidpointRounding.AwayFromZero);
        Currency = string.IsNullOrWhiteSpace(currency) ? "USD" : currency.Trim().ToUpperInvariant();
    }

    public static Money Create(decimal amount, string currency = "USD")
    {
        if (amount < 0)
            throw new PaymentDomainException("Money.NegativeAmount", "Money amount cannot be negative.");

        if (string.IsNullOrWhiteSpace(currency) || currency.Trim().Length != 3)
            throw new PaymentDomainException("Money.InvalidCurrency", "Currency code must be a valid 3-letter ISO code.");

        return new Money(decimal.Round(amount, 2, MidpointRounding.AwayFromZero), currency.Trim().ToUpperInvariant());
    }

    public static Money Zero(string currency = "USD") => new(0m, currency.ToUpperInvariant());

    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new PaymentDomainException("Money.CurrencyMismatch", $"Cannot add {Currency} and {other.Currency}.");
        return new Money(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        if (Currency != other.Currency)
            throw new PaymentDomainException("Money.CurrencyMismatch", $"Cannot subtract {Currency} and {other.Currency}.");
        if (Amount < other.Amount)
            throw new PaymentDomainException("Money.InsufficientFunds", "Cannot subtract more than available amount.");
        return new Money(Amount - other.Amount, Currency);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    public override string ToString() => $"{Amount:0.00} {Currency}";
}
