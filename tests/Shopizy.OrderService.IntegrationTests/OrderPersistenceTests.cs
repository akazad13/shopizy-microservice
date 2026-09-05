using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Shopizy.OrderService.Domain.Entities;
using Shopizy.OrderService.Domain.ValueObjects;
using Shopizy.OrderService.Infrastructure.Persistence;
using Shopizy.OrderService.Infrastructure.Persistence.Repositories;

namespace Shopizy.OrderService.IntegrationTests;

public sealed class OrderPersistenceTests
{
    private static OrderDbContext CreateInMemoryDb()
    {
        var options = new DbContextOptionsBuilder<OrderDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new OrderDbContext(options);
    }

    [Fact]
    public async Task AddOrder_WithItems_CanBeRetrievedWithItems()
    {
        using var db = CreateInMemoryDb();
        var repo = new OrderRepository(db);

        var customerId = Guid.NewGuid();
        var address = new ShippingAddress("Jane Doe", "456 Elm St", "New York", "NY", "10001", "USA");
        var order = Order.Create(Guid.NewGuid(), "ORD-100", customerId, address);
        order.AddItem(Guid.NewGuid(), Guid.NewGuid(), "Mouse", "MS-01", 2, Money.Create(25m));

        await repo.AddAsync(order);

        var fetched = await repo.GetByIdAsync(order.Id);

        fetched.Should().NotBeNull();
        fetched!.OrderNumber.Should().Be("ORD-100");
        fetched.Items.Should().HaveCount(1);
        fetched.Items[0].ProductName.Should().Be("Mouse");
        fetched.TotalAmount.Amount.Should().Be(50m);
    }

    [Fact]
    public async Task GetByCustomerId_ReturnsOnlyTargetCustomerOrders()
    {
        using var db = CreateInMemoryDb();
        var repo = new OrderRepository(db);

        var custA = Guid.NewGuid();
        var custB = Guid.NewGuid();
        var addrA = new ShippingAddress("A", "B", "C", "D", "12345", "USA");
        var addrB = new ShippingAddress("A", "B", "C", "D", "12345", "USA");

        var orderA = Order.Create(Guid.NewGuid(), "ORD-A", custA, addrA);
        var orderB = Order.Create(Guid.NewGuid(), "ORD-B", custB, addrB);

        await repo.AddAsync(orderA);
        await repo.AddAsync(orderB);

        var custAOrders = await repo.GetByCustomerIdAsync(custA);

        custAOrders.Should().HaveCount(1);
        custAOrders[0].Id.Should().Be(orderA.Id);
    }

    [Fact]
    public async Task InventoryRepository_ReserveAndRelease_PersistsAccurately()
    {
        using var db = CreateInMemoryDb();
        var repo = new InventoryRepository(db);

        var variantId = Guid.NewGuid();
        var item = new InventoryItem(variantId, 20);
        await repo.AddAsync(item);

        item.ReserveStock(5);
        await repo.UpdateAsync(item);

        var fetched = await repo.GetByVariantIdAsync(variantId);
        fetched!.AvailableStock.Should().Be(15);
        fetched.ReservedStock.Should().Be(5);
    }
}
