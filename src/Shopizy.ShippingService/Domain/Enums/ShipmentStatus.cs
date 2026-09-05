namespace Shopizy.ShippingService.Domain.Enums;

public enum ShipmentStatus
{
    LabelCreated = 1,
    PackageReceived = 2,
    InTransit = 3,
    OutForDelivery = 4,
    Delivered = 5,
    FailedDelivery = 6
}
