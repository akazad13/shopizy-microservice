using Shopizy.ReviewService.Domain.Entities;

namespace Shopizy.ReviewService.Application.Interfaces;

public interface IReviewRepository
{
    Task<Review?> GetByIdAsync(Guid id);
    Task<List<Review>> GetByProductIdAsync(Guid productId, bool verifiedOnly = false);
    Task AddAsync(Review review);
    Task UpdateAsync(Review review);
    Task DeleteAsync(Review review);
    Task<ReviewVote?> GetVoteAsync(Guid reviewId, Guid userId);
    Task AddVoteAsync(ReviewVote vote);
    Task UpdateVoteAsync(ReviewVote vote);
}
