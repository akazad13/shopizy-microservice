using Shopizy.Domain.Models.Base;

namespace Shopizy.Catelog.API.Aggregates.Products.ValueObjects;

public sealed class ProductImageId : AggregateRootId<Guid>
{
    public override Guid Value { get; protected set; }

    private ProductImageId(Guid value)
    {
        Value = value;
    }

    public static ProductImageId CreateUnique()
    {
        return new(Guid.NewGuid());
    }

    public static ProductImageId Create(Guid value)
    {
        return new(value);
    }

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}

