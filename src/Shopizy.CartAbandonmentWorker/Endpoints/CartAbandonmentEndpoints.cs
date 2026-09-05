using System.Security.Claims;
using Shopizy.CartAbandonmentWorker.Application.Services;

namespace Shopizy.CartAbandonmentWorker.Endpoints;

public static class CartAbandonmentEndpoints
{
    public static void MapCartAbandonmentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/cart-abandonment");

        // Admin: Trigger manual abandonment sweep
        group.MapPost("/sweep", async (
            ClaimsPrincipal user,
            CartAbandonmentApplicationService service) =>
        {
            if (!user.IsInRole("StoreAdmin"))
            {
                return Results.Forbid();
            }

            var result = await service.RunAbandonmentSweepAsync();
            return Results.Ok(result);
        }).RequireAuthorization();

        // Admin: Query recovery history by customer ID
        group.MapGet("/records", async (
            Guid? customerId,
            ClaimsPrincipal user,
            CartAbandonmentApplicationService service) =>
        {
            if (!user.IsInRole("StoreAdmin"))
            {
                return Results.Forbid();
            }

            if (!customerId.HasValue)
            {
                return Results.BadRequest(new { error = "customerId query parameter required" });
            }

            var records = await service.GetRecordsByCustomerIdAsync(customerId.Value);
            return Results.Ok(records);
        }).RequireAuthorization();

        // Public / Shopper: Restore cart with token
        group.MapGet("/restore/{token}", async (
            string token,
            CartAbandonmentApplicationService service) =>
        {
            var result = await service.RestoreCartAsync(token);
            if (result == null)
            {
                return Results.NotFound(new { error = "Invalid or expired recovery token" });
            }

            return Results.Ok(result);
        });
    }
}
