using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Shopizy.CartService.Domain.Entities;
using Shopizy.CartService.Domain.ValueObjects;
using Shopizy.CartService.Infrastructure.Redis;

namespace Shopizy.CartService.IntegrationTests;

/// <summary>Integration tests for RedisCartRepository using in-process MemoryDistributedCache.</summary>
public sealed class RedisCartRepositoryTests
{
    private static IDistributedCache CreateInMemoryCache()
    {
        var opts = Options.Create(new MemoryDistributedCacheOptions());
        return new MemoryDistributedCache(opts);
    }

    private static RedisCartRepository CreateRepo() =>
        new RedisCartRepository(CreateInMemoryCache());

    private static Cart BuildCart(Guid customerId)
    {
        var cart = Cart.CreateForCustomer(customerId);
        cart.AddItem(
            Guid.NewGuid(), Guid.NewGuid(),
            "Test Product", "SKU-TEST",
            new Dictionary<string, string> { ["Color"] = "Red" },
            2,
            Money.Create(49.99m));
        return cart;
    }

    [Fact]
    public async Task SaveAsync_ThenGetAsync_ReturnsIdenticalCart()
    {
        var repo = CreateRepo();
        var customerId = Guid.NewGuid();
        var cart = BuildCart(customerId);

        await repo.SaveAsync(cart, TimeSpan.FromMinutes(5));
        var retrieved = await repo.GetAsync(cart.Id);

        retrieved.Should().NotBeNull();
        retrieved!.Id.Should().Be(cart.Id);
        retrieved.CustomerId.Should().Be(customerId);
        retrieved.Items.Should().HaveCount(1);
        retrieved.Items[0].ProductName.Should().Be("Test Product");
        retrieved.Items[0].Quantity.Should().Be(2);
        retrieved.Items[0].SnapshotPrice.Amount.Should().Be(49.99m);
        retrieved.Items[0].Attributes["Color"].Should().Be("Red");
    }

    [Fact]
    public async Task GetAsync_NonExistentKey_ReturnsNull()
    {
        var repo = CreateRepo();
        var result = await repo.GetAsync("cart:customer:nonexistent-key");
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_RemovesCartFromCache()
    {
        var repo = CreateRepo();
        var cart = BuildCart(Guid.NewGuid());

        await repo.SaveAsync(cart, TimeSpan.FromMinutes(5));
        await repo.DeleteAsync(cart.Id);
        var retrieved = await repo.GetAsync(cart.Id);

        retrieved.Should().BeNull();
    }

    [Fact]
    public async Task SaveAsync_UpdatedCart_OverwritesPreviousValue()
    {
        var repo = CreateRepo();
        var cart = BuildCart(Guid.NewGuid());
        var variantId = cart.Items[0].VariantId;

        await repo.SaveAsync(cart, TimeSpan.FromMinutes(5));

        cart.UpdateItemQuantity(variantId, 5);
        await repo.SaveAsync(cart, TimeSpan.FromMinutes(5));

        var retrieved = await repo.GetAsync(cart.Id);
        retrieved!.Items[0].Quantity.Should().Be(5);
    }

    [Fact]
    public async Task SaveAsync_MultipleItems_AllItemsRoundtrip()
    {
        var repo = CreateRepo();
        var cart = Cart.CreateForGuest("integration-guest");
        cart.AddItem(Guid.NewGuid(), Guid.NewGuid(), "Item A", "SKU-A", null, 1, Money.Create(10m));
        cart.AddItem(Guid.NewGuid(), Guid.NewGuid(), "Item B", "SKU-B", null, 3, Money.Create(25m));

        await repo.SaveAsync(cart, TimeSpan.FromDays(7));
        var retrieved = await repo.GetAsync(cart.Id);

        retrieved!.Items.Should().HaveCount(2);
        retrieved.Subtotal.Amount.Should().Be(85m);
    }
}
