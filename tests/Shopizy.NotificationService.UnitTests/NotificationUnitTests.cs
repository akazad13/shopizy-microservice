using FluentAssertions;
using Shopizy.NotificationService.Domain.Entities;
using Shopizy.NotificationService.Domain.Enums;
using Shopizy.NotificationService.Domain.Exceptions;
using Shopizy.NotificationService.Domain.Services;
using Xunit;

namespace Shopizy.NotificationService.UnitTests;

public class NotificationUnitTests
{
    [Fact]
    public void FormatShipmentDispatched_GeneratesValidTrackingLink()
    {
        var orderId = Guid.NewGuid();
        var carrier = "FedEx";
        var trackingNumber = "trk_fedex_123456789";

        var (subject, body) = NotificationTemplateEngine.FormatShipmentDispatched(orderId, carrier, trackingNumber);

        subject.Should().Contain("Has Shipped!");
        body.Should().Contain(carrier);
        body.Should().Contain($"https://shopizy.com/track/{trackingNumber}");
    }

    [Fact]
    public void CreateNotification_WithInvalidEmail_ThrowsDomainException()
    {
        var act = () => Notification.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "invalid-email-address",
            NotificationType.OrderConfirmation,
            NotificationChannel.Email,
            "Order Confirmation",
            "Order details...");

        act.Should().Throw<NotificationDomainException>()
            .WithMessage("*valid email address*");
    }

    [Fact]
    public void MarkAsSent_UpdatesStatusAndSentTimestamp()
    {
        var notification = Notification.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "customer@example.com",
            NotificationType.OrderConfirmation,
            NotificationChannel.Email,
            "Order Confirmation",
            "Order details...");

        notification.Status.Should().Be(NotificationStatus.Pending);
        notification.SentAtUtc.Should().BeNull();

        notification.MarkAsSent();

        notification.Status.Should().Be(NotificationStatus.Sent);
        notification.SentAtUtc.Should().NotBeNull();
        notification.FailureReason.Should().BeNull();
    }

    [Fact]
    public void MarkAsFailed_UpdatesStatusAndFailureReason()
    {
        var notification = Notification.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "customer@example.com",
            NotificationType.PasswordReset,
            NotificationChannel.Email,
            "Password Reset",
            "Reset token details...");

        notification.MarkAsFailed("SMTP service unavailable");

        notification.Status.Should().Be(NotificationStatus.Failed);
        notification.FailureReason.Should().Be("SMTP service unavailable");
    }
}
