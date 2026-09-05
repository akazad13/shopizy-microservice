using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Shopizy.CartAbandonmentWorker.Domain.Entities;
using Shopizy.CartAbandonmentWorker.Infrastructure.Persistence;
using Shopizy.CartAbandonmentWorker.Infrastructure.Persistence.Repositories;
using Xunit;

namespace Shopizy.CartAbandonmentWorker.IntegrationTests;

public class CartAbandonmentPersistenceTests
{
    private AbandonmentDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AbandonmentDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AbandonmentDbContext(options);
    }

    [Fact]
    public async Task AddAsync_And_GetByTokenAsync_ShouldPersistAndRetrieveRecord()
    {
        using var context = CreateDbContext();
        var repo = new AbandonedCartRepository(context);

        var cartId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var record = AbandonedCartRecord.Create(
            cartId,
            customerId,
            "test@example.com",
            120m,
            "[{\"productId\":\"p1\",\"quantity\":1}]",
            DateTime.UtcNow.AddHours(-3));

        await repo.AddAsync(record);

        var retrieved = await repo.GetByTokenAsync(record.RecoveryToken);
        retrieved.Should().NotBeNull();
        retrieved!.CartId.Should().Be(cartId);
        retrieved.CustomerId.Should().Be(customerId);
        retrieved.CustomerEmail.Should().Be("test@example.com");
        retrieved.IsRestored.Should().BeFalse();
    }

    [Fact]
    public async Task GetLatestByCartIdAsync_ShouldReturnMostRecentDispatchedRecord()
    {
        using var context = CreateDbContext();
        var repo = new AbandonedCartRepository(context);

        var cartId = Guid.NewGuid();
        var customerId = Guid.NewGuid();

        var record1 = AbandonedCartRecord.Create(cartId, customerId, "first@example.com", 50m, "[]", DateTime.UtcNow.AddDays(-3));
        await repo.AddAsync(record1);

        var record2 = AbandonedCartRecord.Create(cartId, customerId, "second@example.com", 70m, "[]", DateTime.UtcNow.AddHours(-2));
        await repo.AddAsync(record2);

        var latest = await repo.GetLatestByCartIdAsync(cartId);
        latest.Should().NotBeNull();
        latest!.RecoveryToken.Should().Be(record2.RecoveryToken);
        latest.CustomerEmail.Should().Be("second@example.com");
    }

    [Fact]
    public async Task UpdateAsync_ShouldPersistRestoredStatus()
    {
        using var context = CreateDbContext();
        var repo = new AbandonedCartRepository(context);

        var record = AbandonedCartRecord.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "restore@example.com",
            40m,
            "[]",
            DateTime.UtcNow.AddHours(-2));

        await repo.AddAsync(record);

        record.MarkAsRestored();
        await repo.UpdateAsync(record);

        var updated = await repo.GetByTokenAsync(record.RecoveryToken);
        updated.Should().NotBeNull();
        updated!.IsRestored.Should().BeTrue();
        updated.RestoredAtUtc.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByCustomerIdAsync_ShouldReturnCustomerRecordsOrderedByDispatchedDesc()
    {
        using var context = CreateDbContext();
        var repo = new AbandonedCartRepository(context);

        var customerId = Guid.NewGuid();
        var r1 = AbandonedCartRecord.Create(Guid.NewGuid(), customerId, "c@example.com", 10m, "[]", DateTime.UtcNow.AddHours(-5));
        var r2 = AbandonedCartRecord.Create(Guid.NewGuid(), customerId, "c@example.com", 20m, "[]", DateTime.UtcNow.AddHours(-2));
        var rOther = AbandonedCartRecord.Create(Guid.NewGuid(), Guid.NewGuid(), "other@example.com", 30m, "[]", DateTime.UtcNow.AddHours(-2));

        await repo.AddAsync(r1);
        await repo.AddAsync(r2);
        await repo.AddAsync(rOther);

        var results = await repo.GetByCustomerIdAsync(customerId);
        results.Should().HaveCount(2);
        results.All(r => r.CustomerId == customerId).Should().BeTrue();
    }
}
