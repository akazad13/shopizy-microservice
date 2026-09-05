using Shopizy.ShippingService.Domain.Enums;

namespace Shopizy.ShippingService.Domain.Entities;

public sealed class ShipmentMilestone
{
    public Guid Id { get; private set; }
    public Guid ShipmentId { get; private set; }
    public ShipmentStatus Status { get; private set; }
    public string Location { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public DateTimeOffset TimestampUtc { get; private set; }

    private ShipmentMilestone() { }

    public ShipmentMilestone(Guid id, Guid shipmentId, ShipmentStatus status, string location, string description)
    {
        Id = id;
        ShipmentId = shipmentId;
        Status = status;
        Location = location;
        Description = description;
        TimestampUtc = DateTimeOffset.UtcNow;
    }
}
