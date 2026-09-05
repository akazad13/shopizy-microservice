namespace Shopizy.SharedKernel.Contracts.Shipping;

public sealed record ShipmentDispatchedIntegrationEvent(
    Guid ShipmentId,
    Guid OrderId,
    string Carrier,
    string TrackingNumber,
    string TrackingUrl,
    DateTime DispatchedAtUtc,
    DateTime EstimatedDeliveryUtc);
