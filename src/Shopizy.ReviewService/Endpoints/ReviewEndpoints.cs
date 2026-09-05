using System.Security.Claims;
using Shopizy.ReviewService.Application.Contracts;
using Shopizy.ReviewService.Application.Services;

namespace Shopizy.ReviewService.Endpoints;

public static class ReviewEndpoints
{
    public static void MapReviewEndpoints(this IEndpointRouteBuilder app)
    {
        var reviews = app.MapGroup("/api/v1/reviews");

        // Public: Get reviews by product ID
        reviews.MapGet("/product/{productId:guid}", async (
            Guid productId,
            bool verifiedOnly,
            ReviewApplicationService service) =>
        {
            var result = await service.GetReviewsByProductIdAsync(productId, verifiedOnly);
            return Results.Ok(result);
        });

        // Public: Get review aggregate summary
        reviews.MapGet("/product/{productId:guid}/summary", async (
            Guid productId,
            ReviewApplicationService service) =>
        {
            var summary = await service.GetProductReviewSummaryAsync(productId);
            return Results.Ok(summary);
        });

        // Customer: Submit review
        reviews.MapPost("", async (
            CreateReviewRequest request,
            ClaimsPrincipal user,
            ReviewApplicationService service) =>
        {
            var userIdStr = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub");
            if (!Guid.TryParse(userIdStr, out var userId))
            {
                return Results.Unauthorized();
            }

            var customerName = user.FindFirstValue(ClaimTypes.Name) ?? "Customer";

            try
            {
                var response = await service.CreateReviewAsync(userId, customerName, request);
                return Results.Created($"/api/v1/reviews/{response.Id}", response);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }).RequireAuthorization();

        // Customer: Vote on a review
        reviews.MapPost("/{id:guid}/vote", async (
            Guid id,
            VoteReviewRequest request,
            ClaimsPrincipal user,
            ReviewApplicationService service) =>
        {
            var userIdStr = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub");
            if (!Guid.TryParse(userIdStr, out var userId))
            {
                return Results.Unauthorized();
            }

            try
            {
                var summary = await service.VoteReviewAsync(id, userId, request.IsHelpful);
                return Results.Ok(summary);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }).RequireAuthorization();

        // Customer/Admin: Delete review
        reviews.MapDelete("/{id:guid}", async (
            Guid id,
            ClaimsPrincipal user,
            ReviewApplicationService service) =>
        {
            var userIdStr = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub");
            if (!Guid.TryParse(userIdStr, out var userId))
            {
                return Results.Unauthorized();
            }

            var isAdmin = user.IsInRole("StoreAdmin");

            try
            {
                await service.DeleteReviewAsync(id, userId, isAdmin);
                return Results.NoContent();
            }
            catch (UnauthorizedAccessException)
            {
                return Results.Forbid();
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }).RequireAuthorization();

        // Wishlists
        var wishlists = app.MapGroup("/api/v1/wishlists");

        // Customer: Get my wishlist
        wishlists.MapGet("/my", async (
            ClaimsPrincipal user,
            ReviewApplicationService service) =>
        {
            var userIdStr = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub");
            if (!Guid.TryParse(userIdStr, out var userId))
            {
                return Results.Unauthorized();
            }

            var wishlist = await service.GetWishlistByCustomerIdAsync(userId);
            return Results.Ok(wishlist);
        }).RequireAuthorization();

        // Customer: Add item to wishlist
        wishlists.MapPost("/items", async (
            AddWishlistItemRequest request,
            ClaimsPrincipal user,
            ReviewApplicationService service) =>
        {
            var userIdStr = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub");
            if (!Guid.TryParse(userIdStr, out var userId))
            {
                return Results.Unauthorized();
            }

            try
            {
                var item = await service.AddWishlistItemAsync(userId, request);
                return Results.Created($"/api/v1/wishlists/items/{item.Id}", item);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }).RequireAuthorization();

        // Customer: Remove item from wishlist
        wishlists.MapDelete("/items/{productId:guid}", async (
            Guid productId,
            ClaimsPrincipal user,
            ReviewApplicationService service) =>
        {
            var userIdStr = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub");
            if (!Guid.TryParse(userIdStr, out var userId))
            {
                return Results.Unauthorized();
            }

            var removed = await service.RemoveWishlistItemAsync(userId, productId);
            return removed ? Results.NoContent() : Results.NotFound();
        }).RequireAuthorization();

        // Admin or Self: Get wishlist by user ID (zero-trust enforcement)
        wishlists.MapGet("/user/{userId:guid}", async (
            Guid userId,
            ClaimsPrincipal user,
            ReviewApplicationService service) =>
        {
            var currentUserIdStr = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub");
            var isSelf = Guid.TryParse(currentUserIdStr, out var currentUserId) && currentUserId == userId;
            var isAdmin = user.IsInRole("StoreAdmin");

            if (!isSelf && !isAdmin)
            {
                return Results.Forbid();
            }

            var wishlist = await service.GetWishlistByCustomerIdAsync(userId);
            return Results.Ok(wishlist);
        }).RequireAuthorization();
    }
}
