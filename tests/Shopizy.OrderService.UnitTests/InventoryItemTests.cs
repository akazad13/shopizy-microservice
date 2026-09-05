using FluentAssertions;
using Shopizy.OrderService.Domain.Entities;
using Shopizy.OrderService.Domain.Exceptions;

namespace Shopizy.OrderService.UnitTests;

public sealed class InventoryItemTests
{
    [Fact]
    public void ReserveStock_SufficientStock_ReducesAvailableIncreasesReserved()
    {
        var item = new InventoryItem(Guid.NewGuid(), 10);
        item.ReserveStock(3);

        item.AvailableStock.Should().Be(7);
        item.ReservedStock.Should().Be(3);
    }

    [Fact]
    public void ReserveStock_InsufficientStock_ThrowsDomainException()
    {
        var item = new InventoryItem(Guid.NewGuid(), 2);
        var act = () => item.ReserveStock(5);

        act.Should().Throw<OrderDomainException>().WithMessage("*Insufficient stock*");
        item.AvailableStock.Should().Be(2);
        item.ReservedStock.Should().Be(0);
    }

    [Fact]
    public void ReleaseReservation_RestoresAvailableStock()
    {
        var item = new InventoryItem(Guid.NewGuid(), 10);
        item.ReserveStock(4);

        item.ReleaseReservation(4);

        item.AvailableStock.Should().Be(10);
        item.ReservedStock.Should().Be(0);
    }

    [Fact]
    public void CommitReservation_DeductsFromReservedStock()
    {
        var item = new InventoryItem(Guid.NewGuid(), 10);
        item.ReserveStock(4);

        item.CommitReservation(4);

        item.AvailableStock.Should().Be(6);
        item.ReservedStock.Should().Be(0);
    }

    [Fact]
    public void Restock_IncreasesAvailableStock()
    {
        var item = new InventoryItem(Guid.NewGuid(), 5);
        item.Restock(10);

        item.AvailableStock.Should().Be(15);
    }
}
