using Shopizy.ShippingService.Domain.Enums;
using Shopizy.ShippingService.Domain.Exceptions;

namespace Shopizy.ShippingService.Domain.Entities;

public sealed class Shipment
{
    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public string TrackingNumber { get; private set; } = string.Empty;
    public string Carrier { get; private set; } = string.Empty;
    public string ServiceLevel { get; private set; } = string.Empty;
    public decimal WeightKg { get; private set; }
    public string DestinationAddress { get; private set; } = string.Empty;
    public string DestinationZip { get; private set; } = string.Empty;
    public ShipmentStatus Status { get; private set; }
    public DateTimeOffset EstimatedDeliveryUtc { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public List<ShipmentMilestone> Milestones { get; private set; } = new();

    private Shipment() { }

    public static Shipment Create(
        Guid id,
        Guid orderId,
        string carrier,
        string serviceLevel,
        decimal weightKg,
        string destinationAddress,
        string destinationZip,
        int estimatedDays = 3)
    {
        if (weightKg <= 0)
            throw new ShippingDomainException("Shipping.InvalidWeight", "Parcel weight must be greater than zero.");

        if (string.IsNullOrWhiteSpace(destinationZip))
            throw new ShippingDomainException("Shipping.InvalidDestination", "Destination zip code is required.");

        var shipment = new Shipment
        {
            Id = id,
            OrderId = orderId,
            TrackingNumber = $"trk_{carrier.ToLowerInvariant()}_{Guid.NewGuid():N}",
            Carrier = carrier,
            ServiceLevel = serviceLevel,
            WeightKg = weightKg,
            DestinationAddress = destinationAddress,
            DestinationZip = destinationZip,
            Status = ShipmentStatus.LabelCreated,
            EstimatedDeliveryUtc = DateTimeOffset.UtcNow.AddDays(estimatedDays),
            CreatedAtUtc = DateTimeOffset.UtcNow
        };

        shipment.Milestones.Add(new ShipmentMilestone(
            Guid.NewGuid(),
            id,
            ShipmentStatus.LabelCreated,
            "Origin Fulfillment Center",
            $"Shipping label created for {carrier} ({serviceLevel})"));

        return shipment;
    }

    public void AddMilestone(ShipmentStatus status, string location, string description)
    {
        Status = status;
        Milestones.Add(new ShipmentMilestone(Guid.NewGuid(), Id, status, location, description));
    }
}
