using Shopizy.Domain.Models.Base;

namespace Shopizy.Cart.API.Aggregates.ValueObjects;

public sealed class CustomerId : AggregateRootId<Guid>
{
    public override Guid Value { get; protected set; }

    private CustomerId(Guid value)
    {
        Value = value;
    }

    public static CustomerId CreateUnique()
    {
        return new(Guid.NewGuid());
    }

    public static CustomerId Create(Guid value)
    {
        return new(value);
    }

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}

