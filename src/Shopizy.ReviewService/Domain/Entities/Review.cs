using Shopizy.ReviewService.Domain.Exceptions;

namespace Shopizy.ReviewService.Domain.Entities;

public class Review
{
    public Guid Id { get; private set; }
    public Guid ProductId { get; private set; }
    public Guid CustomerId { get; private set; }
    public string CustomerName { get; private set; } = string.Empty;
    public int Rating { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Comment { get; private set; } = string.Empty;
    public List<string> ImageUrls { get; private set; } = new();
    public bool IsVerifiedBuyer { get; private set; }
    public int HelpfulVotes { get; private set; }
    public int UnhelpfulVotes { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }

    private Review() { }

    public static Review Create(
        Guid id,
        Guid productId,
        Guid customerId,
        string customerName,
        int rating,
        string title,
        string comment,
        bool isVerifiedBuyer,
        List<string>? imageUrls = null)
    {
        if (productId == Guid.Empty)
            throw new ReviewDomainException("INVALID_PRODUCT_ID", "ProductId cannot be empty.");

        if (customerId == Guid.Empty)
            throw new ReviewDomainException("INVALID_CUSTOMER_ID", "CustomerId cannot be empty.");

        if (rating < 1 || rating > 5)
            throw new ReviewDomainException("INVALID_RATING", "Rating must be between 1 and 5 stars.");

        if (string.IsNullOrWhiteSpace(title))
            throw new ReviewDomainException("EMPTY_TITLE", "Review title cannot be empty.");

        if (string.IsNullOrWhiteSpace(comment))
            throw new ReviewDomainException("EMPTY_COMMENT", "Review comment cannot be empty.");

        return new Review
        {
            Id = id == Guid.Empty ? Guid.NewGuid() : id,
            ProductId = productId,
            CustomerId = customerId,
            CustomerName = string.IsNullOrWhiteSpace(customerName) ? "Anonymous" : customerName.Trim(),
            Rating = rating,
            Title = title.Trim(),
            Comment = comment.Trim(),
            IsVerifiedBuyer = isVerifiedBuyer,
            ImageUrls = imageUrls ?? new List<string>(),
            HelpfulVotes = 0,
            UnhelpfulVotes = 0,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public void AddVote(bool isHelpful)
    {
        if (isHelpful)
            HelpfulVotes++;
        else
            UnhelpfulVotes++;

        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void SwitchVote(bool nowHelpful)
    {
        if (nowHelpful)
        {
            if (UnhelpfulVotes > 0) UnhelpfulVotes--;
            HelpfulVotes++;
        }
        else
        {
            if (HelpfulVotes > 0) HelpfulVotes--;
            UnhelpfulVotes++;
        }

        UpdatedAtUtc = DateTime.UtcNow;
    }
}
