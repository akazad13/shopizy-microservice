using FluentAssertions;
using Shopizy.ReviewService.Domain.Entities;
using Shopizy.ReviewService.Domain.Exceptions;
using Shopizy.ReviewService.Domain.Services;
using Xunit;

namespace Shopizy.ReviewService.UnitTests;

public class ReviewUnitTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    [InlineData(-1)]
    public void CreateReview_WithInvalidRating_ThrowsReviewDomainException(int invalidRating)
    {
        var act = () => Review.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "John Doe",
            invalidRating,
            "Good product",
            "I liked it.",
            true);

        act.Should().Throw<ReviewDomainException>()
            .WithMessage("*Rating must be between 1 and 5 stars*");
    }

    [Theory]
    [InlineData("", "Valid comment")]
    [InlineData("Valid Title", "")]
    [InlineData("   ", "Valid comment")]
    [InlineData("Valid Title", "   ")]
    public void CreateReview_WithEmptyTitleOrComment_ThrowsReviewDomainException(string title, string comment)
    {
        var act = () => Review.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "John Doe",
            5,
            title,
            comment,
            false);

        act.Should().Throw<ReviewDomainException>();
    }

    [Fact]
    public void AddVote_AndSwitchVote_CorrectlyUpdatesHelpfulnessCounters()
    {
        var review = Review.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Reviewer",
            4,
            "Solid Buy",
            "Works as expected",
            true);

        // Initially zero
        review.HelpfulVotes.Should().Be(0);
        review.UnhelpfulVotes.Should().Be(0);

        // Add helpful vote
        review.AddVote(true);
        review.HelpfulVotes.Should().Be(1);
        review.UnhelpfulVotes.Should().Be(0);

        // Switch to unhelpful
        review.SwitchVote(false);
        review.HelpfulVotes.Should().Be(0);
        review.UnhelpfulVotes.Should().Be(1);
    }

    [Fact]
    public void RatingCalculator_ComputesCorrectWeightedAverageAndDistribution()
    {
        var pId = Guid.NewGuid();
        var reviews = new List<Review>
        {
            Review.Create(Guid.NewGuid(), pId, Guid.NewGuid(), "U1", 5, "Great", "Loved it", true),
            Review.Create(Guid.NewGuid(), pId, Guid.NewGuid(), "U2", 4, "Good", "Very nice", true),
            Review.Create(Guid.NewGuid(), pId, Guid.NewGuid(), "U3", 5, "Awesome", "Super", false),
            Review.Create(Guid.NewGuid(), pId, Guid.NewGuid(), "U4", 3, "Average", "Decent", false)
        };

        // Total = 5+4+5+3 = 17 / 4 = 4.25 -> rounded to 4.3
        var (average, total, distribution) = RatingCalculator.Calculate(reviews);

        average.Should().Be(4.3m);
        total.Should().Be(4);
        distribution[5].Should().Be(2);
        distribution[4].Should().Be(1);
        distribution[3].Should().Be(1);
        distribution[2].Should().Be(0);
        distribution[1].Should().Be(0);
    }
}
