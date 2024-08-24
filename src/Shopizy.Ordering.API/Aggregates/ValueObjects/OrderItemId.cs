using Shopizy.Domain.Models.Base;

namespace Shopizy.Ordering.API.Aggregates.ValueObjects;

public sealed class OrderItemId : AggregateRootId<Guid>
{
    public override Guid Value { get; protected set; }

    private OrderItemId(Guid value)
    {
        Value = value;
    }

    public static OrderItemId CreateUnique()
    {
        return new(Guid.NewGuid());
    }

    public static OrderItemId Create(Guid value)
    {
        return new(value);
    }

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
