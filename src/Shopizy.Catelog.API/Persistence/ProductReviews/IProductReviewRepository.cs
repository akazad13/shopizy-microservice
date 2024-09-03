using Shopizy.Catelog.API.Aggregates.ProductReviews;
using Shopizy.Catelog.API.Aggregates.ProductReviews.ValueObjects;

namespace Shopizy.Catelog.API.Persistence.ProductReviews;

public interface IProductReviewRepository
{
    Task<List<ProductReview>> GetProductReviewsAsync();
    Task<ProductReview?> GetProductReviewByIdAsync(ProductReviewId id);
    Task AddAsync(ProductReview productReview);
    void Update(ProductReview productReview);
    Task<int> Commit(CancellationToken cancellationToken);
}
