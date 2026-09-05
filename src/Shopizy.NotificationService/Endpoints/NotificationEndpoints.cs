using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shopizy.NotificationService.Application.Contracts;
using Shopizy.NotificationService.Application.Services;
using Shopizy.NotificationService.Domain.Exceptions;
using Shopizy.NotificationService.Hubs;

namespace Shopizy.NotificationService.Endpoints;

public static class NotificationEndpoints
{
    public static IEndpointRouteBuilder MapNotificationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/notifications")
            .WithTags("Notifications");

        group.MapPost("/send", async (
            SendNotificationRequest request,
            NotificationApplicationService service,
            CancellationToken ct) =>
        {
            try
            {
                var result = await service.SendNotificationAsync(request, ct);
                return Results.Created($"/api/v1/notifications/{result.Id}", result);
            }
            catch (NotificationDomainException ex)
            {
                return Results.Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: ex.Code,
                    detail: ex.Message);
            }
        })
        .RequireAuthorization("AdminOnly");

        group.MapGet("/user/{userId:guid}", async (
            Guid userId,
            ClaimsPrincipal user,
            NotificationApplicationService service,
            CancellationToken ct) =>
        {
            var callerId = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub");
            var isAdmin = user.IsInRole("StoreAdmin");

            if (!isAdmin && callerId != userId.ToString())
            {
                return Results.Forbid();
            }

            var notifications = await service.GetUserNotificationsAsync(userId, ct);
            return Results.Ok(notifications);
        })
        .RequireAuthorization();

        group.MapPost("/push/order-status", async (
            OrderStatusPushRequest request,
            NotificationApplicationService service,
            CancellationToken ct) =>
        {
            var result = await service.PushOrderStatusAsync(request, ct);
            return Results.Ok(result);
        })
        .RequireAuthorization("AdminOnly");

        group.MapPost("/push/merchant-event", async (
            MerchantEventPushRequest request,
            NotificationApplicationService service,
            CancellationToken ct) =>
        {
            var result = await service.PushMerchantEventAsync(request, ct);
            return Results.Ok(result);
        })
        .RequireAuthorization("AdminOnly");

        app.MapHub<NotificationHub>("/hubs/notifications");
        app.MapHub<MerchantFeedHub>("/hubs/merchant-feed");

        return app;
    }
}
