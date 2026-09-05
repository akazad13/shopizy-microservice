using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Shopizy.LoyaltyService.Domain.Entities;
using Shopizy.LoyaltyService.Domain.Enums;
using Shopizy.LoyaltyService.Infrastructure.Persistence;
using Shopizy.LoyaltyService.Infrastructure.Persistence.Repositories;
using Xunit;

namespace Shopizy.LoyaltyService.IntegrationTests;

public class LoyaltyPersistenceTests
{
    private static LoyaltyDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<LoyaltyDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new LoyaltyDbContext(options);
    }

    [Fact]
    public async Task LoyaltyRepository_PersistsAccountAndTransactionsCorrectly()
    {
        using var context = CreateContext();
        var repo = new LoyaltyRepository(context);

        var customerId = Guid.NewGuid();
        var account = LoyaltyAccount.Create(customerId);

        account.AccruePoints(120, Guid.NewGuid(), "Order 1 points");
        account.AccruePoints(80, Guid.NewGuid(), "Order 2 points");
        account.RedeemPoints(50, Guid.NewGuid(), "Redemption for checkout discount");

        await repo.AddAsync(account);

        var retrieved = await repo.GetByCustomerIdAsync(customerId);
        retrieved.Should().NotBeNull();
        retrieved!.PointsBalance.Should().Be(150);
        retrieved.Transactions.Should().HaveCount(3);
    }

    [Fact]
    public async Task GiftCardRepository_PersistsCardAndTracksBalanceDeductions()
    {
        using var context = CreateContext();
        var repo = new GiftCardRepository(context);

        var card = GiftCard.Create(100.00m, "USD", customCode: "GIFT-TEST-1234");
        await repo.AddAsync(card);

        var retrieved = await repo.GetByCodeAsync("GIFT-TEST-1234");
        retrieved.Should().NotBeNull();
        retrieved!.CurrentBalance.Should().Be(100.00m);
        retrieved.Status.Should().Be(GiftCardStatus.Active);

        // Deduct
        retrieved.DeductBalance(40.00m);
        await repo.UpdateAsync(retrieved);

        var afterUpdate = await repo.GetByCodeAsync("GIFT-TEST-1234");
        afterUpdate.Should().NotBeNull();
        afterUpdate!.CurrentBalance.Should().Be(60.00m);
    }
}
