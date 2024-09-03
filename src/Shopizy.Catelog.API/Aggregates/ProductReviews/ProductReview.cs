using Shopizy.Catelog.API.Aggregates.ProductReviews.ValueObjects;
using Shopizy.Catelog.API.Aggregates.Products.ValueObjects;
using Shopizy.Domain.Models.Base;
using Shopizy.Domain.Models.ValueObjects;

namespace Shopizy.Catelog.API.Aggregates.ProductReviews;

public sealed class ProductReview : AggregateRoot<ProductReviewId, Guid>
{
    public CustomerId CustomerId { get; set; }
    public ProductId ProductId { get; set; }
    public Rating Rating { get; set; }
    public string Comment { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime? ModifiedOn { get; set; }

    public static ProductReview Create(
        CustomerId customerId,
        ProductId productId,
        Rating rating,
        string comment
    )
    {
        return new ProductReview(
            ProductReviewId.CreateUnique(),
            customerId,
            productId,
            rating,
            comment
        );
    }

    private ProductReview(
        ProductReviewId productReviewId,
        CustomerId customerId,
        ProductId productId,
        Rating rating,
        string comment
    ) : base(productReviewId)
    {
        CustomerId = customerId;
        ProductId = productId;
        Rating = rating;
        Comment = comment;
        CreatedOn = DateTime.UtcNow;
    }

    private ProductReview() { }
}

