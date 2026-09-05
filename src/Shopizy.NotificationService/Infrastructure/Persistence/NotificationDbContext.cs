using Microsoft.EntityFrameworkCore;
using Shopizy.NotificationService.Domain.Entities;

namespace Shopizy.NotificationService.Infrastructure.Persistence;

public sealed class NotificationDbContext : DbContext
{
    public NotificationDbContext(DbContextOptions<NotificationDbContext> options) : base(options) { }

    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Notification>(b =>
        {
            b.HasKey(n => n.Id);
            b.HasIndex(n => n.UserId);
            b.Property(n => n.Recipient).HasMaxLength(150).IsRequired();
            b.Property(n => n.Subject).HasMaxLength(250).IsRequired();
            b.Property(n => n.Body).IsRequired();
            b.Property(n => n.FailureReason).HasMaxLength(500);
        });
    }
}
