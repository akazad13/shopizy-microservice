using Shopizy.ReviewService.Application.Contracts;
using Shopizy.ReviewService.Application.Interfaces;
using Shopizy.ReviewService.Domain.Entities;
using Shopizy.ReviewService.Domain.Exceptions;
using Shopizy.ReviewService.Domain.Services;

namespace Shopizy.ReviewService.Application.Services;

public class ReviewApplicationService
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IWishlistRepository _wishlistRepository;
    private readonly IOrderVerificationClient _orderVerificationClient;

    public ReviewApplicationService(
        IReviewRepository reviewRepository,
        IWishlistRepository wishlistRepository,
        IOrderVerificationClient orderVerificationClient)
    {
        _reviewRepository = reviewRepository;
        _wishlistRepository = wishlistRepository;
        _orderVerificationClient = orderVerificationClient;
    }

    public async Task<ReviewResponse> CreateReviewAsync(Guid customerId, string customerName, CreateReviewRequest request)
    {
        var isVerified = await _orderVerificationClient.IsDeliveredOrderAsync(
            customerId, request.ProductId, request.VerifiedOrderId);

        var review = Review.Create(
            Guid.NewGuid(),
            request.ProductId,
            customerId,
            customerName,
            request.Rating,
            request.Title,
            request.Comment,
            isVerified,
            request.ImageUrls);

        await _reviewRepository.AddAsync(review);
        return MapToResponse(review);
    }

    public async Task<List<ReviewResponse>> GetReviewsByProductIdAsync(Guid productId, bool verifiedOnly = false)
    {
        var reviews = await _reviewRepository.GetByProductIdAsync(productId, verifiedOnly);
        return reviews.Select(MapToResponse).ToList();
    }

    public async Task<ProductReviewSummaryResponse> GetProductReviewSummaryAsync(Guid productId)
    {
        var reviews = await _reviewRepository.GetByProductIdAsync(productId);
        var (average, total, distribution) = RatingCalculator.Calculate(reviews);
        return new ProductReviewSummaryResponse(productId, average, total, distribution);
    }

    public async Task<ReviewVoteSummaryResponse> VoteReviewAsync(Guid reviewId, Guid userId, bool isHelpful)
    {
        var review = await _reviewRepository.GetByIdAsync(reviewId)
            ?? throw new ReviewDomainException("NOT_FOUND", "Review not found.");

        var existingVote = await _reviewRepository.GetVoteAsync(reviewId, userId);

        if (existingVote == null)
        {
            var vote = ReviewVote.Create(reviewId, userId, isHelpful);
            await _reviewRepository.AddVoteAsync(vote);
            review.AddVote(isHelpful);
        }
        else if (existingVote.IsHelpful != isHelpful)
        {
            existingVote.UpdateVote(isHelpful);
            await _reviewRepository.UpdateVoteAsync(existingVote);
            review.SwitchVote(isHelpful);
        }

        await _reviewRepository.UpdateAsync(review);
        return new ReviewVoteSummaryResponse(review.Id, review.HelpfulVotes, review.UnhelpfulVotes);
    }

    public async Task DeleteReviewAsync(Guid reviewId, Guid requesterId, bool isAdmin)
    {
        var review = await _reviewRepository.GetByIdAsync(reviewId)
            ?? throw new ReviewDomainException("NOT_FOUND", "Review not found.");

        if (!isAdmin && review.CustomerId != requesterId)
        {
            throw new UnauthorizedAccessException("You are not authorized to delete this review.");
        }

        await _reviewRepository.DeleteAsync(review);
    }

    public async Task<WishlistResponse> GetWishlistByCustomerIdAsync(Guid customerId)
    {
        var wishlist = await _wishlistRepository.GetByCustomerIdAsync(customerId);
        if (wishlist == null)
        {
            wishlist = Wishlist.Create(customerId);
            await _wishlistRepository.AddAsync(wishlist);
        }

        return MapToWishlistResponse(wishlist);
    }

    public async Task<WishlistItemResponse> AddWishlistItemAsync(Guid customerId, AddWishlistItemRequest request)
    {
        var wishlist = await _wishlistRepository.GetByCustomerIdAsync(customerId);
        if (wishlist == null)
        {
            wishlist = Wishlist.Create(customerId);
            await _wishlistRepository.AddAsync(wishlist);
        }

        var item = wishlist.AddItem(request.ProductId, request.ProductName, request.Sku, request.PriceSnapshot);
        await _wishlistRepository.UpdateAsync(wishlist);

        return new WishlistItemResponse(item.Id, item.ProductId, item.ProductName, item.Sku, item.PriceSnapshot, item.AddedAtUtc);
    }

    public async Task<bool> RemoveWishlistItemAsync(Guid customerId, Guid productId)
    {
        var wishlist = await _wishlistRepository.GetByCustomerIdAsync(customerId);
        if (wishlist == null) return false;

        var removed = wishlist.RemoveItem(productId);
        if (removed)
        {
            await _wishlistRepository.UpdateAsync(wishlist);
        }
        return removed;
    }

    private static ReviewResponse MapToResponse(Review r) =>
        new(r.Id, r.ProductId, r.CustomerId, r.CustomerName, r.Rating, r.Title, r.Comment,
            r.ImageUrls, r.IsVerifiedBuyer, r.HelpfulVotes, r.UnhelpfulVotes, r.CreatedAtUtc);

    private static WishlistResponse MapToWishlistResponse(Wishlist w) =>
        new(w.Id, w.CustomerId,
            w.Items.Select(i => new WishlistItemResponse(i.Id, i.ProductId, i.ProductName, i.Sku, i.PriceSnapshot, i.AddedAtUtc)).ToList());
}
