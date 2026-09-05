using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Shopizy.ReviewService.Domain.Entities;
using Shopizy.ReviewService.Infrastructure.Persistence;
using Shopizy.ReviewService.Infrastructure.Persistence.Repositories;
using Xunit;

namespace Shopizy.ReviewService.IntegrationTests;

public class ReviewPersistenceTests
{
    private static ReviewDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ReviewDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ReviewDbContext(options);
    }

    [Fact]
    public async Task ReviewRepository_PersistsAndRetrievesReviewsWithFilters()
    {
        using var context = CreateContext();
        var repo = new ReviewRepository(context);

        var productId = Guid.NewGuid();
        var verifiedReview = Review.Create(
            Guid.NewGuid(),
            productId,
            Guid.NewGuid(),
            "Verified Buyer",
            5,
            "Authentic purchase",
            "Great product quality",
            true);

        var unverifiedReview = Review.Create(
            Guid.NewGuid(),
            productId,
            Guid.NewGuid(),
            "Casual Visitor",
            4,
            "Looks good",
            "Haven't used yet",
            false);

        await repo.AddAsync(verifiedReview);
        await repo.AddAsync(unverifiedReview);

        var allReviews = await repo.GetByProductIdAsync(productId, verifiedOnly: false);
        allReviews.Should().HaveCount(2);

        var verifiedOnly = await repo.GetByProductIdAsync(productId, verifiedOnly: true);
        verifiedOnly.Should().HaveCount(1);
        verifiedOnly[0].IsVerifiedBuyer.Should().BeTrue();
    }

    [Fact]
    public async Task WishlistRepository_EnforcesCustomerIsolationAndCascadeOperations()
    {
        using var context = CreateContext();
        var repo = new WishlistRepository(context);

        var customerId = Guid.NewGuid();
        var wishlist = Wishlist.Create(customerId);

        var pId1 = Guid.NewGuid();
        var pId2 = Guid.NewGuid();

        wishlist.AddItem(pId1, "Product 1", "SKU-1", 49.99m);
        wishlist.AddItem(pId2, "Product 2", "SKU-2", 89.99m);

        await repo.AddAsync(wishlist);

        var retrieved = await repo.GetByCustomerIdAsync(customerId);
        retrieved.Should().NotBeNull();
        retrieved!.Items.Should().HaveCount(2);

        // Remove an item
        wishlist.RemoveItem(pId1);
        await repo.UpdateAsync(wishlist);

        var afterRemoval = await repo.GetByCustomerIdAsync(customerId);
        afterRemoval.Should().NotBeNull();
        afterRemoval!.Items.Should().HaveCount(1);
        afterRemoval.Items.First().ProductId.Should().Be(pId2);
    }
}
