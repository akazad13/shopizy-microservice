using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Shopizy.NotificationService.Domain.Entities;
using Shopizy.NotificationService.Domain.Enums;
using Shopizy.NotificationService.Infrastructure.Persistence;
using Shopizy.NotificationService.Infrastructure.Persistence.Repositories;
using Xunit;

namespace Shopizy.NotificationService.IntegrationTests;

public class NotificationPersistenceTests
{
    private static NotificationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<NotificationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new NotificationDbContext(options);
    }

    [Fact]
    public async Task AddNotification_PersistsSuccessfully()
    {
        using var context = CreateContext();
        var repo = new NotificationRepository(context);

        var userId = Guid.NewGuid();
        var notification = Notification.Create(
            Guid.NewGuid(),
            userId,
            "customer@example.com",
            NotificationType.OrderConfirmation,
            NotificationChannel.Email,
            "Order Confirmed",
            "Your order has been confirmed.");

        notification.MarkAsSent();
        await repo.AddAsync(notification);

        var retrieved = await repo.GetByIdAsync(notification.Id);
        retrieved.Should().NotBeNull();
        retrieved!.Status.Should().Be(NotificationStatus.Sent);
        retrieved.Recipient.Should().Be("customer@example.com");
    }

    [Fact]
    public async Task GetByUserId_ReturnsOnlyThatCustomersNotifications()
    {
        using var context = CreateContext();
        var repo = new NotificationRepository(context);

        var customerId1 = Guid.NewGuid();
        var customerId2 = Guid.NewGuid();

        var n1 = Notification.Create(Guid.NewGuid(), customerId1, "cust1@example.com",
            NotificationType.ShipmentDispatched, NotificationChannel.Email, "Shipped!", "Track here.");
        n1.MarkAsSent();

        var n2 = Notification.Create(Guid.NewGuid(), customerId1, "cust1@example.com",
            NotificationType.OrderConfirmation, NotificationChannel.Email, "Order Confirmed", "Details...");
        n2.MarkAsSent();

        var n3 = Notification.Create(Guid.NewGuid(), customerId2, "cust2@example.com",
            NotificationType.OrderConfirmation, NotificationChannel.Email, "Order Confirmed", "Details...");
        n3.MarkAsSent();

        await repo.AddAsync(n1);
        await repo.AddAsync(n2);
        await repo.AddAsync(n3);

        var customer1Notifications = await repo.GetByUserIdAsync(customerId1);
        customer1Notifications.Should().HaveCount(2);
        customer1Notifications.Should().AllSatisfy(n => n.UserId.Should().Be(customerId1));
    }
}
