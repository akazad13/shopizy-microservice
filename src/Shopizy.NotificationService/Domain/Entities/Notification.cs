using Shopizy.NotificationService.Domain.Enums;
using Shopizy.NotificationService.Domain.Exceptions;

namespace Shopizy.NotificationService.Domain.Entities;

public sealed class Notification
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string Recipient { get; private set; } = string.Empty;
    public NotificationType Type { get; private set; }
    public NotificationChannel Channel { get; private set; }
    public string Subject { get; private set; } = string.Empty;
    public string Body { get; private set; } = string.Empty;
    public NotificationStatus Status { get; private set; }
    public string? FailureReason { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset? SentAtUtc { get; private set; }

    private Notification() { }

    public static Notification Create(
        Guid id,
        Guid userId,
        string recipient,
        NotificationType type,
        NotificationChannel channel,
        string subject,
        string body)
    {
        if (id == Guid.Empty)
            throw new NotificationDomainException("Notification.InvalidId", "Notification ID cannot be empty.");

        if (string.IsNullOrWhiteSpace(recipient))
            throw new NotificationDomainException("Notification.InvalidRecipient", "Recipient cannot be empty.");

        if (channel == NotificationChannel.Email && !recipient.Contains('@'))
            throw new NotificationDomainException("Notification.InvalidEmail", "Recipient must be a valid email address.");

        if (string.IsNullOrWhiteSpace(subject))
            throw new NotificationDomainException("Notification.InvalidSubject", "Subject cannot be empty.");

        return new Notification
        {
            Id = id,
            UserId = userId,
            Recipient = recipient.Trim(),
            Type = type,
            Channel = channel,
            Subject = subject.Trim(),
            Body = body.Trim(),
            Status = NotificationStatus.Pending,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };
    }

    public void MarkAsSent()
    {
        if (Status == NotificationStatus.Sent)
            return;

        Status = NotificationStatus.Sent;
        SentAtUtc = DateTimeOffset.UtcNow;
        FailureReason = null;
    }

    public void MarkAsFailed(string reason)
    {
        Status = NotificationStatus.Failed;
        FailureReason = string.IsNullOrWhiteSpace(reason) ? "Delivery failure" : reason.Trim();
    }
}
