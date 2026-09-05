using Shopizy.NotificationService.Domain.Enums;

namespace Shopizy.NotificationService.Application.Contracts;

public sealed record SendNotificationRequest(
    Guid UserId,
    string RecipientEmail,
    NotificationType Type,
    NotificationChannel Channel,
    string Subject,
    string Body);

public sealed record NotificationResponse(
    Guid Id,
    Guid UserId,
    string Recipient,
    NotificationType Type,
    NotificationChannel Channel,
    string Subject,
    string Body,
    NotificationStatus Status,
    string? FailureReason,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? SentAtUtc);

public sealed record OrderStatusPushRequest(
    Guid OrderId,
    Guid CustomerId,
    string Status,
    string? TrackingNumber = null,
    string? Carrier = null);

public sealed record OrderStatusPushResponse(
    bool Broadcasted,
    Guid OrderId,
    string Status,
    DateTimeOffset TimestampUtc);

public sealed record MerchantEventPushRequest(
    string EventType,
    decimal Amount,
    string Currency,
    string Description);

public sealed record MerchantEventPushResponse(
    bool Broadcasted,
    string EventType,
    decimal Amount,
    string Currency,
    DateTimeOffset TimestampUtc);
