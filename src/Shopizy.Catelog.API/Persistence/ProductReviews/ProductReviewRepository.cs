using Microsoft.EntityFrameworkCore;
using Shopizy.Catelog.API.Aggregates.ProductReviews;
using Shopizy.Catelog.API.Aggregates.ProductReviews.ValueObjects;

namespace Shopizy.Catelog.API.Persistence.ProductReviews;

public class ProductReviewRepository(CatelogDbContext dbContext) : IProductReviewRepository
{
    private readonly CatelogDbContext _dbContext = dbContext;

    public Task<List<ProductReview>> GetProductReviewsAsync()
    {
        return _dbContext.ProductReviews.AsNoTracking().ToListAsync();
    }
    public Task<ProductReview?> GetProductReviewByIdAsync(ProductReviewId id)
    {
        return _dbContext.ProductReviews.FirstOrDefaultAsync(c => c.Id == id);
    }
    public async Task AddAsync(ProductReview productReview)
    {
        await _dbContext.ProductReviews.AddAsync(productReview);
    }
    public void Update(ProductReview productReview)
    {
        _dbContext.Update(productReview);
    }

    public Task<int> Commit(CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
