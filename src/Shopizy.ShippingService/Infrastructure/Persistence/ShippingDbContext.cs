using Microsoft.EntityFrameworkCore;
using Shopizy.ShippingService.Domain.Entities;

namespace Shopizy.ShippingService.Infrastructure.Persistence;

public sealed class ShippingDbContext : DbContext
{
    public ShippingDbContext(DbContextOptions<ShippingDbContext> options) : base(options) { }

    public DbSet<Shipment> Shipments => Set<Shipment>();
    public DbSet<ShipmentMilestone> ShipmentMilestones => Set<ShipmentMilestone>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Shipment>(b =>
        {
            b.HasKey(s => s.Id);
            b.HasIndex(s => s.TrackingNumber).IsUnique();
            b.HasIndex(s => s.OrderId);
            b.Property(s => s.TrackingNumber).HasMaxLength(50).IsRequired();
            b.Property(s => s.Carrier).HasMaxLength(50).IsRequired();
            b.Property(s => s.ServiceLevel).HasMaxLength(50);
            b.Property(s => s.WeightKg).HasPrecision(18, 2);
            b.Property(s => s.DestinationAddress).HasMaxLength(250);
            b.Property(s => s.DestinationZip).HasMaxLength(20);

            b.HasMany(s => s.Milestones)
                .WithOne()
                .HasForeignKey(m => m.ShipmentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ShipmentMilestone>(b =>
        {
            b.HasKey(m => m.Id);
            b.Property(m => m.Location).HasMaxLength(150);
            b.Property(m => m.Description).HasMaxLength(250);
        });
    }
}
