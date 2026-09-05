using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Shopizy.OrderService.Application.Contracts;
using Shopizy.OrderService.Application.Services;
using Shopizy.OrderService.Domain.Exceptions;

namespace Shopizy.OrderService.Endpoints;

public static class OrderEndpoints
{
    public static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        var ordersGroup = app.MapGroup("/api/v1/orders").WithTags("Orders");
        var inventoryGroup = app.MapGroup("/api/v1/inventory").WithTags("Inventory");

        // POST /api/v1/orders
        ordersGroup.MapPost("/", async (
            [FromBody] CreateOrderRequest req,
            HttpContext ctx,
            OrderApplicationService orderSvc,
            CancellationToken ct) =>
        {
            var (customerId, _) = ResolveUser(ctx);
            if (!customerId.HasValue) return Results.Unauthorized();

            try
            {
                var response = await orderSvc.CreateOrderAsync(customerId.Value, req, ct);
                return Results.Created($"/api/v1/orders/{response.Id}", response);
            }
            catch (OrderDomainException ex) when (ex.Code == "Inventory.InsufficientStock")
            {
                return Results.Problem(detail: ex.Message, statusCode: 400, title: ex.Code);
            }
            catch (OrderDomainException ex)
            {
                return Results.Problem(detail: ex.Message, statusCode: 400, title: ex.Code);
            }
        }).RequireAuthorization();

        // GET /api/v1/orders/{id}
        ordersGroup.MapGet("/{id:guid}", async (
            Guid id,
            HttpContext ctx,
            OrderApplicationService orderSvc,
            CancellationToken ct) =>
        {
            var (customerId, isAdmin) = ResolveUser(ctx);
            if (!customerId.HasValue && !isAdmin) return Results.Unauthorized();

            var response = await orderSvc.GetOrderAsync(id, customerId, isAdmin, ct);
            return response is null ? Results.NotFound() : Results.Ok(response);
        }).RequireAuthorization();

        // GET /api/v1/orders
        ordersGroup.MapGet("/", async (
            HttpContext ctx,
            OrderApplicationService orderSvc,
            CancellationToken ct) =>
        {
            var (customerId, isAdmin) = ResolveUser(ctx);
            if (!customerId.HasValue && !isAdmin) return Results.Unauthorized();

            var response = await orderSvc.ListOrdersAsync(customerId, isAdmin, ct);
            return Results.Ok(response);
        }).RequireAuthorization();

        // POST /api/v1/orders/{id}/pay
        ordersGroup.MapPost("/{id:guid}/pay", async (
            Guid id,
            OrderApplicationService orderSvc,
            CancellationToken ct) =>
        {
            try
            {
                var response = await orderSvc.PayOrderAsync(id, ct);
                return Results.Ok(response);
            }
            catch (OrderDomainException ex)
            {
                return Results.Problem(detail: ex.Message, statusCode: 400, title: ex.Code);
            }
        });

        // POST /api/v1/orders/{id}/cancel
        ordersGroup.MapPost("/{id:guid}/cancel", async (
            Guid id,
            HttpContext ctx,
            OrderApplicationService orderSvc,
            CancellationToken ct) =>
        {
            var (customerId, isAdmin) = ResolveUser(ctx);
            try
            {
                var response = await orderSvc.CancelOrderAsync(id, customerId, isAdmin, "CustomerCancelled", ct);
                return Results.Ok(response);
            }
            catch (OrderDomainException ex)
            {
                return Results.Problem(detail: ex.Message, statusCode: 400, title: ex.Code);
            }
        }).RequireAuthorization();

        // POST /api/v1/orders/{id}/expire
        ordersGroup.MapPost("/{id:guid}/expire", async (
            Guid id,
            [FromQuery] DateTimeOffset? asOf,
            OrderApplicationService orderSvc,
            CancellationToken ct) =>
        {
            try
            {
                var response = await orderSvc.ExpireOrderAsync(id, asOf, ct);
                return Results.Ok(response);
            }
            catch (OrderDomainException ex)
            {
                return Results.Problem(detail: ex.Message, statusCode: 400, title: ex.Code);
            }
        });

        // POST /api/v1/orders/{id}/ship
        ordersGroup.MapPost("/{id:guid}/ship", async (
            Guid id,
            OrderApplicationService orderSvc,
            CancellationToken ct) =>
        {
            try
            {
                var response = await orderSvc.ShipOrderAsync(id, ct);
                return Results.Ok(response);
            }
            catch (OrderDomainException ex)
            {
                return Results.Problem(detail: ex.Message, statusCode: 400, title: ex.Code);
            }
        });

        // POST /api/v1/orders/{id}/deliver
        ordersGroup.MapPost("/{id:guid}/deliver", async (
            Guid id,
            OrderApplicationService orderSvc,
            CancellationToken ct) =>
        {
            try
            {
                var response = await orderSvc.DeliverOrderAsync(id, ct);
                return Results.Ok(response);
            }
            catch (OrderDomainException ex)
            {
                return Results.Problem(detail: ex.Message, statusCode: 400, title: ex.Code);
            }
        });

        // GET /api/v1/inventory/{variantId}
        inventoryGroup.MapGet("/{variantId:guid}", async (
            Guid variantId,
            OrderApplicationService orderSvc,
            CancellationToken ct) =>
        {
            var response = await orderSvc.GetInventoryAsync(variantId, ct);
            return response is null ? Results.NotFound() : Results.Ok(response);
        });

        // POST /api/v1/inventory/{variantId}/adjust
        inventoryGroup.MapPost("/{variantId:guid}/adjust", async (
            Guid variantId,
            [FromBody] AdjustInventoryRequest req,
            OrderApplicationService orderSvc,
            CancellationToken ct) =>
        {
            var response = await orderSvc.AdjustInventoryAsync(variantId, req.Quantity, ct);
            return Results.Ok(response);
        });

        return app;
    }

    private static (Guid? customerId, bool isAdmin) ResolveUser(HttpContext ctx)
    {
        if (ctx.User.Identity?.IsAuthenticated != true)
            return (null, false);

        var sub = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier)
               ?? ctx.User.FindFirstValue("sub");
        var role = ctx.User.FindFirstValue(ClaimTypes.Role)
                ?? ctx.User.FindFirstValue("role");

        bool isAdmin = string.Equals(role, "StoreAdmin", StringComparison.OrdinalIgnoreCase);
        Guid? customerId = Guid.TryParse(sub, out var id) ? id : null;

        return (customerId, isAdmin);
    }
}
