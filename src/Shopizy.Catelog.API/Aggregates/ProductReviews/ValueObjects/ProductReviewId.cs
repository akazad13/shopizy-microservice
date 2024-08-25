using Shopizy.Domain.Models.Base;

namespace Shopizy.Catelog.API.Aggregates.ProductReviews.ValueObjects;

public sealed class ProductReviewId : AggregateRootId<Guid>
{
    public override Guid Value { get; protected set; }

    private ProductReviewId(Guid value)
    {
        Value = value;
    }

    public static ProductReviewId CreateUnique()
    {
        return new(Guid.NewGuid());
    }

    public static ProductReviewId Create(Guid value)
    {
        return new(value);
    }

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
