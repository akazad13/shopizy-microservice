namespace Shopizy.ReviewService.Domain.Entities;

public class ReviewVote
{
    public Guid Id { get; private set; }
    public Guid ReviewId { get; private set; }
    public Guid UserId { get; private set; }
    public bool IsHelpful { get; private set; }
    public DateTime VotedAtUtc { get; private set; }

    private ReviewVote() { }

    public static ReviewVote Create(Guid reviewId, Guid userId, bool isHelpful)
    {
        return new ReviewVote
        {
            Id = Guid.NewGuid(),
            ReviewId = reviewId,
            UserId = userId,
            IsHelpful = isHelpful,
            VotedAtUtc = DateTime.UtcNow
        };
    }

    public void UpdateVote(bool isHelpful)
    {
        IsHelpful = isHelpful;
        VotedAtUtc = DateTime.UtcNow;
    }
}
