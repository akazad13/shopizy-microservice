using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Shopizy.ShippingService.Domain.Entities;
using Shopizy.ShippingService.Domain.Enums;
using Shopizy.ShippingService.Infrastructure.Persistence;
using Shopizy.ShippingService.Infrastructure.Persistence.Repositories;
using Xunit;

namespace Shopizy.ShippingService.IntegrationTests;

public class ShippingPersistenceTests
{
    private static ShippingDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ShippingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ShippingDbContext(options);
    }

    [Fact]
    public async Task AddShipment_PersistsWithMilestones()
    {
        using var context = CreateContext();
        var repo = new ShipmentRepository(context);

        var shipment = Shipment.Create(
            Guid.NewGuid(), Guid.NewGuid(), "FedEx", "2-Day", 3.0m, "456 Market St", "94105");

        await repo.AddAsync(shipment);

        var retrieved = await repo.GetByTrackingNumberAsync(shipment.TrackingNumber);
        retrieved.Should().NotBeNull();
        retrieved!.Carrier.Should().Be("FedEx");
        retrieved.Milestones.Should().HaveCount(1);
    }

    [Fact]
    public async Task UpdateShipment_AppendsNewMilestones()
    {
        using var context = CreateContext();
        var repo = new ShipmentRepository(context);

        var shipment = Shipment.Create(
            Guid.NewGuid(), Guid.NewGuid(), "UPS", "Ground", 2.2m, "789 Broadway", "10003");

        await repo.AddAsync(shipment);

        shipment.AddMilestone(ShipmentStatus.InTransit, "Sorting Facility - New York", "Departed facility");
        await repo.UpdateAsync(shipment);

        var updated = await repo.GetByTrackingNumberAsync(shipment.TrackingNumber);
        updated!.Status.Should().Be(ShipmentStatus.InTransit);
        updated.Milestones.Should().HaveCount(2);
    }
}
