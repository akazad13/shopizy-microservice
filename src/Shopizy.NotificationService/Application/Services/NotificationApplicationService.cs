using Microsoft.AspNetCore.SignalR;
using Shopizy.NotificationService.Application.Contracts;
using Shopizy.NotificationService.Application.Interfaces;
using Shopizy.NotificationService.Domain.Entities;
using Shopizy.NotificationService.Hubs;

namespace Shopizy.NotificationService.Application.Services;

public sealed class NotificationApplicationService
{
    private readonly INotificationRepository _repo;
    private readonly INotificationDispatcher _dispatcher;
    private readonly IHubContext<NotificationHub> _notificationHub;
    private readonly IHubContext<MerchantFeedHub> _merchantHub;

    public NotificationApplicationService(
        INotificationRepository repo,
        INotificationDispatcher dispatcher,
        IHubContext<NotificationHub> notificationHub,
        IHubContext<MerchantFeedHub> merchantHub)
    {
        _repo = repo;
        _dispatcher = dispatcher;
        _notificationHub = notificationHub;
        _merchantHub = merchantHub;
    }

    public async Task<NotificationResponse> SendNotificationAsync(SendNotificationRequest request, CancellationToken ct = default)
    {
        var notification = Notification.Create(
            Guid.NewGuid(),
            request.UserId,
            request.RecipientEmail,
            request.Type,
            request.Channel,
            request.Subject,
            request.Body);

        var success = await _dispatcher.DispatchAsync(notification, ct);
        if (success)
        {
            notification.MarkAsSent();
        }
        else
        {
            notification.MarkAsFailed("Failed to dispatch via designated channel.");
        }

        await _repo.AddAsync(notification, ct);
        return ToResponse(notification);
    }

    public async Task<IReadOnlyList<NotificationResponse>> GetUserNotificationsAsync(Guid userId, CancellationToken ct = default)
    {
        var list = await _repo.GetByUserIdAsync(userId, ct);
        return list.Select(ToResponse).ToList();
    }

    public async Task<OrderStatusPushResponse> PushOrderStatusAsync(OrderStatusPushRequest request, CancellationToken ct = default)
    {
        var timestamp = DateTimeOffset.UtcNow;
        var group = $"order_{request.OrderId}";

        await _notificationHub.Clients.Group(group).SendAsync(
            "OrderStatusUpdated",
            new
            {
                orderId = request.OrderId,
                customerId = request.CustomerId,
                status = request.Status,
                trackingNumber = request.TrackingNumber,
                carrier = request.Carrier,
                timestampUtc = timestamp
            },
            cancellationToken: ct);

        return new OrderStatusPushResponse(true, request.OrderId, request.Status, timestamp);
    }

    public async Task<MerchantEventPushResponse> PushMerchantEventAsync(MerchantEventPushRequest request, CancellationToken ct = default)
    {
        var timestamp = DateTimeOffset.UtcNow;

        await _merchantHub.Clients.All.SendAsync(
            "MerchantEventReceived",
            new
            {
                eventType = request.EventType,
                amount = request.Amount,
                currency = request.Currency,
                description = request.Description,
                timestampUtc = timestamp
            },
            cancellationToken: ct);

        return new MerchantEventPushResponse(true, request.EventType, request.Amount, request.Currency, timestamp);
    }

    private static NotificationResponse ToResponse(Notification n) => new(
        n.Id,
        n.UserId,
        n.Recipient,
        n.Type,
        n.Channel,
        n.Subject,
        n.Body,
        n.Status,
        n.FailureReason,
        n.CreatedAtUtc,
        n.SentAtUtc);
}
