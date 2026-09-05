using Shopizy.ShippingService.Application.Contracts;
using Shopizy.ShippingService.Application.Interfaces;
using Shopizy.ShippingService.Domain.Entities;
using Shopizy.ShippingService.Domain.Exceptions;
using Shopizy.ShippingService.Domain.Services;

namespace Shopizy.ShippingService.Application.Services;

public sealed class ShippingApplicationService
{
    private readonly IShipmentRepository _repo;

    public ShippingApplicationService(IShipmentRepository repo)
    {
        _repo = repo;
    }

    public IReadOnlyList<ShippingRateQuote> GetRates(CalculateShippingRatesRequest request)
    {
        return CarrierRateCalculator.CalculateRates(request.Subtotal, request.WeightKg, request.Country ?? "US");
    }

    public async Task<ShipmentResponse> CreateShipmentAsync(CreateShipmentRequest request, CancellationToken ct = default)
    {
        var existing = await _repo.GetByOrderIdAsync(request.OrderId, ct);
        if (existing is not null)
            return ToResponse(existing);

        var shipment = Shipment.Create(
            Guid.NewGuid(),
            request.OrderId,
            request.Carrier,
            request.ServiceLevel,
            request.WeightKg,
            request.DestinationAddress,
            request.DestinationZip,
            request.EstimatedDays ?? 3);

        await _repo.AddAsync(shipment, ct);
        return ToResponse(shipment);
    }

    public async Task<ShipmentResponse?> GetShipmentByTrackingNumberAsync(string trackingNumber, CancellationToken ct = default)
    {
        var shipment = await _repo.GetByTrackingNumberAsync(trackingNumber, ct);
        return shipment is null ? null : ToResponse(shipment);
    }

    public async Task<ShipmentResponse> AddMilestoneAsync(string trackingNumber, AddMilestoneRequest request, CancellationToken ct = default)
    {
        var shipment = await _repo.GetByTrackingNumberAsync(trackingNumber, ct)
            ?? throw new ShippingDomainException("Shipping.NotFound", $"Shipment with tracking number '{trackingNumber}' not found.");

        shipment.AddMilestone(request.Status, request.Location, request.Description);
        await _repo.UpdateAsync(shipment, ct);

        return ToResponse(shipment);
    }

    private static ShipmentResponse ToResponse(Shipment s) => new(
        s.Id,
        s.OrderId,
        s.TrackingNumber,
        s.Carrier,
        s.ServiceLevel,
        s.WeightKg,
        s.DestinationAddress,
        s.DestinationZip,
        s.Status,
        s.EstimatedDeliveryUtc,
        s.CreatedAtUtc,
        s.Milestones.Select(m => new MilestoneDto(m.Id, m.Status, m.Location, m.Description, m.TimestampUtc)).ToList());
}
