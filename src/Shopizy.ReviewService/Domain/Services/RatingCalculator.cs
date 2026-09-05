using Shopizy.ReviewService.Domain.Entities;

namespace Shopizy.ReviewService.Domain.Services;

public static class RatingCalculator
{
    public static (decimal AverageRating, int TotalReviews, Dictionary<int, int> Distribution) Calculate(IEnumerable<Review> reviews)
    {
        var reviewList = reviews.ToList();
        var distribution = new Dictionary<int, int>
        {
            { 1, 0 },
            { 2, 0 },
            { 3, 0 },
            { 4, 0 },
            { 5, 0 }
        };

        if (reviewList.Count == 0)
        {
            return (0.0m, 0, distribution);
        }

        foreach (var review in reviewList)
        {
            if (distribution.ContainsKey(review.Rating))
            {
                distribution[review.Rating]++;
            }
        }

        var totalStars = reviewList.Sum(r => r.Rating);
        var average = Math.Round((decimal)totalStars / reviewList.Count, 1, MidpointRounding.AwayFromZero);

        return (average, reviewList.Count, distribution);
    }
}
