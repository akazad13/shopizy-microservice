using System.Security.Claims;
using Shopizy.LoyaltyService.Application.Contracts;
using Shopizy.LoyaltyService.Application.Services;

namespace Shopizy.LoyaltyService.Endpoints;

public static class LoyaltyEndpoints
{
    public static void MapLoyaltyEndpoints(this IEndpointRouteBuilder app)
    {
        var loyalty = app.MapGroup("/api/v1/loyalty");

        // Customer: Get my loyalty account
        loyalty.MapGet("/my", async (
            ClaimsPrincipal user,
            LoyaltyApplicationService service) =>
        {
            var userIdStr = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub");
            if (!Guid.TryParse(userIdStr, out var userId))
            {
                return Results.Unauthorized();
            }

            var account = await service.GetOrCreateAccountAsync(userId);
            return Results.Ok(account);
        }).RequireAuthorization();

        // Admin or Self: Get loyalty account by user ID (zero-trust check)
        loyalty.MapGet("/account/{userId:guid}", async (
            Guid userId,
            ClaimsPrincipal user,
            LoyaltyApplicationService service) =>
        {
            var currentUserIdStr = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub");
            var isSelf = Guid.TryParse(currentUserIdStr, out var currentUserId) && currentUserId == userId;
            var isAdmin = user.IsInRole("StoreAdmin");

            if (!isSelf && !isAdmin)
            {
                return Results.Forbid();
            }

            var account = await service.GetOrCreateAccountAsync(userId);
            return Results.Ok(account);
        }).RequireAuthorization();

        // Admin / Worker: Accrue points on completed order
        loyalty.MapPost("/accrue", async (
            AccruePointsRequest request,
            ClaimsPrincipal user,
            LoyaltyApplicationService service) =>
        {
            if (!user.IsInRole("StoreAdmin"))
            {
                return Results.Forbid();
            }

            try
            {
                var account = await service.AccruePointsAsync(request);
                return Results.Ok(account);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }).RequireAuthorization();

        // Customer: Redeem points
        loyalty.MapPost("/redeem", async (
            RedeemPointsRequest request,
            ClaimsPrincipal user,
            LoyaltyApplicationService service) =>
        {
            var userIdStr = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub");
            if (!Guid.TryParse(userIdStr, out var userId))
            {
                return Results.Unauthorized();
            }

            try
            {
                var response = await service.RedeemPointsAsync(userId, request);
                return Results.Ok(response);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }).RequireAuthorization();

        // Gift Cards
        var giftCards = app.MapGroup("/api/v1/gift-cards");

        // Admin: Create gift card
        giftCards.MapPost("", async (
            CreateGiftCardRequest request,
            ClaimsPrincipal user,
            LoyaltyApplicationService service) =>
        {
            if (!user.IsInRole("StoreAdmin"))
            {
                return Results.Forbid();
            }

            try
            {
                var response = await service.CreateGiftCardAsync(request);
                return Results.Created($"/api/v1/gift-cards/{response.Id}", response);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }).RequireAuthorization();

        // Public / Shopper: Check gift card balance
        giftCards.MapGet("/check/{code}", async (
            string code,
            LoyaltyApplicationService service) =>
        {
            try
            {
                var response = await service.CheckGiftCardBalanceAsync(code);
                return Results.Ok(response);
            }
            catch (Exception ex)
            {
                return Results.NotFound(new { error = ex.Message });
            }
        });

        // Shopper: Apply / spend gift card balance
        giftCards.MapPost("/apply", async (
            ApplyGiftCardRequest request,
            ClaimsPrincipal user,
            LoyaltyApplicationService service) =>
        {
            try
            {
                var response = await service.ApplyGiftCardAsync(request);
                return Results.Ok(response);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }).RequireAuthorization();
    }
}
