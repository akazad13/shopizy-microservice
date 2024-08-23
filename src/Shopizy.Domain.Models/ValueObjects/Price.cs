using Shopizy.Domain.Models.Enums;
using Shopizy.Domain.Models.Models;

namespace Shopizy.Domain.Models.ValueObjects;

public sealed class Price : ValueObject
{
    public decimal Amount { get; private set; }
    public Currency Currency { get; private set; }

    private Price(decimal amount, Currency currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public static Price CreateNew(decimal amount, Currency currency)
    {
        return new(amount, currency);
    }

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }
}
