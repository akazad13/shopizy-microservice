using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Shopizy.PromotionService.Domain.Entities;
using Shopizy.PromotionService.Domain.Enums;
using Shopizy.PromotionService.Infrastructure.Persistence;
using Shopizy.PromotionService.Infrastructure.Persistence.Repositories;
using Xunit;

namespace Shopizy.PromotionService.IntegrationTests;

public class PromotionPersistenceTests
{
    private static PromotionDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<PromotionDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new PromotionDbContext(options);
    }

    [Fact]
    public async Task AddAndRetrieveCampaign_PersistsCorrectly()
    {
        using var context = CreateContext();
        var repo = new PromotionRepository(context);

        var campaign = PromotionCampaign.Create(
            Guid.NewGuid(), "WELCOME10", "10% off welcome coupon", DiscountType.Percentage, 10m,
            25m, 20m, null, 100, DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(30));

        await repo.AddAsync(campaign);

        var retrieved = await repo.GetByCodeAsync("welcome10");
        retrieved.Should().NotBeNull();
        retrieved!.Code.Should().Be("WELCOME10");
        retrieved.DiscountValue.Should().Be(10m);
        retrieved.MaxGlobalUsages.Should().Be(100);
    }

    [Fact]
    public async Task IncrementUsage_UpdatesUsageCountInDb()
    {
        using var context = CreateContext();
        var repo = new PromotionRepository(context);

        var campaign = PromotionCampaign.Create(
            Guid.NewGuid(), "FLASH50", "Flash sale", DiscountType.Percentage, 50m,
            null, null, null, 5, DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(1));

        await repo.AddAsync(campaign);

        campaign.IncrementUsage();
        await repo.UpdateAsync(campaign);

        var updated = await repo.GetByCodeAsync("FLASH50");
        updated!.CurrentUsageCount.Should().Be(1);
    }
}
