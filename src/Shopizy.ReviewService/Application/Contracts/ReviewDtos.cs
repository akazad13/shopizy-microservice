namespace Shopizy.ReviewService.Application.Contracts;

public record CreateReviewRequest(
    Guid ProductId,
    int Rating,
    string Title,
    string Comment,
    List<string>? ImageUrls,
    Guid? VerifiedOrderId = null);

public record ReviewResponse(
    Guid Id,
    Guid ProductId,
    Guid CustomerId,
    string CustomerName,
    int Rating,
    string Title,
    string Comment,
    List<string> ImageUrls,
    bool IsVerifiedBuyer,
    int HelpfulVotes,
    int UnhelpfulVotes,
    DateTime CreatedAtUtc);

public record ProductReviewSummaryResponse(
    Guid ProductId,
    decimal AverageRating,
    int TotalReviews,
    Dictionary<int, int> RatingDistribution);

public record VoteReviewRequest(bool IsHelpful);

public record ReviewVoteSummaryResponse(
    Guid ReviewId,
    int HelpfulVotes,
    int UnhelpfulVotes);

public record AddWishlistItemRequest(
    Guid ProductId,
    string ProductName,
    string Sku,
    decimal PriceSnapshot);

public record WishlistItemResponse(
    Guid Id,
    Guid ProductId,
    string ProductName,
    string Sku,
    decimal PriceSnapshot,
    DateTime AddedAtUtc);

public record WishlistResponse(
    Guid Id,
    Guid CustomerId,
    List<WishlistItemResponse> Items);
