using Shopizy.ShippingService.Domain.Entities;
using Shopizy.ShippingService.Domain.Enums;
using Shopizy.ShippingService.Domain.Services;

namespace Shopizy.ShippingService.Application.Contracts;

public sealed record CalculateShippingRatesRequest(
    decimal Subtotal,
    decimal WeightKg,
    string DestinationZip,
    string? Country);

public sealed record CreateShipmentRequest(
    Guid OrderId,
    string Carrier,
    string ServiceLevel,
    decimal WeightKg,
    string DestinationAddress,
    string DestinationZip,
    int? EstimatedDays);

public sealed record AddMilestoneRequest(
    ShipmentStatus Status,
    string Location,
    string Description);

public sealed record MilestoneDto(
    Guid Id,
    ShipmentStatus Status,
    string Location,
    string Description,
    DateTimeOffset TimestampUtc);

public sealed record ShipmentResponse(
    Guid Id,
    Guid OrderId,
    string TrackingNumber,
    string Carrier,
    string ServiceLevel,
    decimal WeightKg,
    string DestinationAddress,
    string DestinationZip,
    ShipmentStatus Status,
    DateTimeOffset EstimatedDeliveryUtc,
    DateTimeOffset CreatedAtUtc,
    IReadOnlyList<MilestoneDto> Milestones);
