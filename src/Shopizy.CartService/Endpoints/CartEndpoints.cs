using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Shopizy.CartService.Application.DTOs;
using Shopizy.CartService.Application.Services;
using Shopizy.SharedKernel.Domain;

namespace Shopizy.CartService.Endpoints;

public static class CartEndpoints
{
    private const string GuestCartIdHeader = "X-Guest-Cart-Id";

    public static IEndpointRouteBuilder MapCartEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/cart").WithTags("Cart");

        // GET /api/v1/cart
        group.MapGet("/", async (
            HttpContext ctx,
            CartCommandService cartSvc,
            CancellationToken ct) =>
        {
            var cartId = ResolveCartId(ctx);
            if (cartId is null) return Results.BadRequest(new { error = "Provide Authorization header or X-Guest-Cart-Id." });

            var response = await cartSvc.GetCartAsync(cartId, ct);
            return Results.Ok(response);
        });

        // POST /api/v1/cart/items
        group.MapPost("/items", async (
            [FromBody] AddToCartRequest req,
            HttpContext ctx,
            CartCommandService cartSvc,
            CancellationToken ct) =>
        {
            if (req.Quantity is < 1 or > 99)
                return Results.Problem(detail: "Quantity must be between 1 and 99.", statusCode: 400, title: "Validation Error");

            var (cartId, customerId, isCustomer) = ResolveCartContext(ctx);
            if (cartId is null) return Results.BadRequest(new { error = "Provide Authorization header or X-Guest-Cart-Id." });

            try
            {
                var response = await cartSvc.AddItemAsync(cartId, customerId, isCustomer, req, ct);
                return Results.Ok(response);
            }
            catch (DomainException ex)
            {
                return Results.Problem(detail: ex.Message, statusCode: 400, title: ex.Code);
            }
        });

        // PUT /api/v1/cart/items/{variantId}
        group.MapPut("/items/{variantId:guid}", async (
            Guid variantId,
            [FromBody] UpdateCartItemRequest req,
            HttpContext ctx,
            CartCommandService cartSvc,
            CancellationToken ct) =>
        {
            if (req.Quantity is < 1 or > 99)
                return Results.Problem(detail: "Quantity must be between 1 and 99.", statusCode: 400, title: "Validation Error");

            var (cartId, _, isCustomer) = ResolveCartContext(ctx);
            if (cartId is null) return Results.BadRequest(new { error = "Provide Authorization header or X-Guest-Cart-Id." });

            try
            {
                var response = await cartSvc.UpdateItemAsync(cartId, isCustomer, variantId, req.Quantity, ct);
                return Results.Ok(response);
            }
            catch (DomainException ex) when (ex.Code == "Cart.ItemNotFound")
            {
                return Results.NotFound(new { error = ex.Message });
            }
            catch (DomainException ex)
            {
                return Results.Problem(detail: ex.Message, statusCode: 400, title: ex.Code);
            }
        });

        // DELETE /api/v1/cart/items/{variantId}
        group.MapDelete("/items/{variantId:guid}", async (
            Guid variantId,
            HttpContext ctx,
            CartCommandService cartSvc,
            CancellationToken ct) =>
        {
            var (cartId, _, isCustomer) = ResolveCartContext(ctx);
            if (cartId is null) return Results.BadRequest(new { error = "Provide Authorization header or X-Guest-Cart-Id." });

            try
            {
                var response = await cartSvc.RemoveItemAsync(cartId, isCustomer, variantId, ct);
                return Results.Ok(response);
            }
            catch (DomainException ex) when (ex.Code == "Cart.ItemNotFound")
            {
                return Results.NotFound(new { error = ex.Message });
            }
        });

        // DELETE /api/v1/cart
        group.MapDelete("/", async (
            HttpContext ctx,
            CartCommandService cartSvc,
            CancellationToken ct) =>
        {
            var (cartId, _, isCustomer) = ResolveCartContext(ctx);
            if (cartId is null) return Results.BadRequest(new { error = "Provide Authorization header or X-Guest-Cart-Id." });

            await cartSvc.ClearCartAsync(cartId, isCustomer, ct);
            return Results.NoContent();
        });

        // POST /api/v1/cart/merge
        group.MapPost("/merge", async (
            [FromBody] MergeCartRequest req,
            HttpContext ctx,
            CartCommandService cartSvc,
            CancellationToken ct) =>
        {
            if (!ctx.User.Identity?.IsAuthenticated ?? true)
                return Results.Unauthorized();

            var customerIdClaim = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? ctx.User.FindFirstValue("sub");
            if (!Guid.TryParse(customerIdClaim, out var customerId))
                return Results.Unauthorized();

            if (string.IsNullOrWhiteSpace(req.GuestCartId))
                return Results.Problem(detail: "GuestCartId must be provided.", statusCode: 400, title: "Validation Error");

            var response = await cartSvc.MergeGuestCartAsync(customerId, req.GuestCartId, ct);
            return Results.Ok(response);
        }).RequireAuthorization();

        return app;
    }

    private static string? ResolveCartId(HttpContext ctx)
    {
        var (cartId, _, _) = ResolveCartContext(ctx);
        return cartId;
    }

    private static (string? cartId, Guid? customerId, bool isCustomer) ResolveCartContext(HttpContext ctx)
    {
        if (ctx.User.Identity?.IsAuthenticated == true)
        {
            var sub = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier)
                   ?? ctx.User.FindFirstValue("sub");
            if (Guid.TryParse(sub, out var cid))
                return ($"cart:customer:{cid}", cid, true);
        }

        if (ctx.Request.Headers.TryGetValue(GuestCartIdHeader, out var guestId) && !string.IsNullOrWhiteSpace(guestId))
            return ($"cart:guest:{guestId}", null, false);

        return (null, null, false);
    }
}

