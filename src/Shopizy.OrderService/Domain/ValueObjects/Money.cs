using Shopizy.OrderService.Domain.Exceptions;
using Shopizy.SharedKernel.Domain;

namespace Shopizy.OrderService.Domain.ValueObjects;

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
            throw new OrderDomainException("Money.NegativeAmount", "Money amount cannot be negative.");

        if (string.IsNullOrWhiteSpace(currency) || currency.Trim().Length != 3)
            throw new OrderDomainException("Money.InvalidCurrency", "Currency code must be a valid 3-letter ISO code.");

        return new Money(decimal.Round(amount, 2, MidpointRounding.AwayFromZero), currency.Trim().ToUpperInvariant());
    }

    public static Money Zero(string currency = "USD") => new(0m, currency.ToUpperInvariant());

    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new OrderDomainException("Money.CurrencyMismatch", $"Cannot add {Currency} and {other.Currency}.");
        return new Money(Amount + other.Amount, Currency);
    }

    public Money Multiply(int factor) => new(Amount * factor, Currency);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    public override string ToString() => $"{Amount:0.00} {Currency}";
}
