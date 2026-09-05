using Microsoft.EntityFrameworkCore;
using Shopizy.ReviewService.Application.Interfaces;
using Shopizy.ReviewService.Domain.Entities;

namespace Shopizy.ReviewService.Infrastructure.Persistence.Repositories;

public class ReviewRepository : IReviewRepository
{
    private readonly ReviewDbContext _context;

    public ReviewRepository(ReviewDbContext context)
    {
        _context = context;
    }

    public async Task<Review?> GetByIdAsync(Guid id)
    {
        return await _context.Reviews.FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<List<Review>> GetByProductIdAsync(Guid productId, bool verifiedOnly = false)
    {
        var query = _context.Reviews.Where(r => r.ProductId == productId);
        if (verifiedOnly)
        {
            query = query.Where(r => r.IsVerifiedBuyer);
        }
        return await query.OrderByDescending(r => r.CreatedAtUtc).ToListAsync();
    }

    public async Task AddAsync(Review review)
    {
        await _context.Reviews.AddAsync(review);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Review review)
    {
        _context.Reviews.Update(review);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Review review)
    {
        _context.Reviews.Remove(review);
        await _context.SaveChangesAsync();
    }

    public async Task<ReviewVote?> GetVoteAsync(Guid reviewId, Guid userId)
    {
        return await _context.ReviewVotes.FirstOrDefaultAsync(v => v.ReviewId == reviewId && v.UserId == userId);
    }

    public async Task AddVoteAsync(ReviewVote vote)
    {
        await _context.ReviewVotes.AddAsync(vote);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateVoteAsync(ReviewVote vote)
    {
        _context.ReviewVotes.Update(vote);
        await _context.SaveChangesAsync();
    }
}
