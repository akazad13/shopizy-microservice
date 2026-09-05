using Microsoft.AspNetCore.Mvc;
using Shopizy.ShippingService.Application.Contracts;
using Shopizy.ShippingService.Application.Services;

namespace Shopizy.ShippingService.Endpoints;

public static class ShippingEndpoints
{
    public static IEndpointRouteBuilder MapShippingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/shipping")
            .WithTags("Shipping");

        group.MapPost("/rates", (
            [FromBody] CalculateShippingRatesRequest request,
            ShippingApplicationService shippingService) =>
        {
            var rates = shippingService.GetRates(request);
            return Results.Ok(rates);
        }).AllowAnonymous();

        group.MapPost("/shipments", async (
            [FromBody] CreateShipmentRequest request,
            ShippingApplicationService shippingService,
            CancellationToken ct) =>
        {
            var shipment = await shippingService.CreateShipmentAsync(request, ct);
            return Results.Created($"/api/v1/shipping/shipments/{shipment.TrackingNumber}", shipment);
        }).RequireAuthorization("StoreAdminOnly");

        group.MapGet("/shipments/{trackingNumber}", async (
            string trackingNumber,
            ShippingApplicationService shippingService,
            CancellationToken ct) =>
        {
            var shipment = await shippingService.GetShipmentByTrackingNumberAsync(trackingNumber, ct);
            return shipment is null ? Results.NotFound() : Results.Ok(shipment);
        }).AllowAnonymous();

        group.MapPost("/shipments/{trackingNumber}/milestones", async (
            string trackingNumber,
            [FromBody] AddMilestoneRequest request,
            ShippingApplicationService shippingService,
            CancellationToken ct) =>
        {
            var updated = await shippingService.AddMilestoneAsync(trackingNumber, request, ct);
            return Results.Ok(updated);
        }).RequireAuthorization("StoreAdminOnly");

        return app;
    }
}
