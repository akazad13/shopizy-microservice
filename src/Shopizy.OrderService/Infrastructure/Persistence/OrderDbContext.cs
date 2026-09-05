using Microsoft.EntityFrameworkCore;
using Shopizy.OrderService.Domain.Entities;
using Shopizy.OrderService.Domain.ValueObjects;

namespace Shopizy.OrderService.Infrastructure.Persistence;

public sealed class OrderDbContext : DbContext
{
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<InventoryItem> Inventory => Set<InventoryItem>();

    public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Order>(b =>
        {
            b.HasKey(o => o.Id);
            b.Property(o => o.OrderNumber).IsRequired().HasMaxLength(64);
            b.Property(o => o.CustomerId).IsRequired();
            b.Property(o => o.Status).IsRequired();
            b.Property(o => o.CreatedAtUtc).IsRequired();
            b.Property(o => o.ExpiresAtUtc).IsRequired();

            b.OwnsOne(o => o.ShippingAddress, sa =>
            {
                sa.Property(a => a.FullName).HasColumnName("ShippingFullName").HasMaxLength(128).IsRequired();
                sa.Property(a => a.Street).HasColumnName("ShippingStreet").HasMaxLength(256).IsRequired();
                sa.Property(a => a.City).HasColumnName("ShippingCity").HasMaxLength(100).IsRequired();
                sa.Property(a => a.State).HasColumnName("ShippingState").HasMaxLength(100);
                sa.Property(a => a.PostalCode).HasColumnName("ShippingPostalCode").HasMaxLength(32).IsRequired();
                sa.Property(a => a.Country).HasColumnName("ShippingCountry").HasMaxLength(64).IsRequired();
            });

            b.HasMany(o => o.Items)
                .WithOne()
                .HasForeignKey(i => i.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OrderItem>(b =>
        {
            b.HasKey(i => i.Id);
            b.Property(i => i.OrderId).IsRequired();
            b.Property(i => i.ProductId).IsRequired();
            b.Property(i => i.VariantId).IsRequired();
            b.Property(i => i.ProductName).IsRequired().HasMaxLength(256);
            b.Property(i => i.VariantSku).IsRequired().HasMaxLength(64);
            b.Property(i => i.Quantity).IsRequired();

            b.OwnsOne(i => i.UnitPrice, up =>
            {
                up.Property(m => m.Amount).HasColumnName("UnitPriceAmount").HasPrecision(18, 2).IsRequired();
                up.Property(m => m.Currency).HasColumnName("UnitPriceCurrency").HasMaxLength(3).IsRequired();
            });
        });

        modelBuilder.Entity<InventoryItem>(b =>
        {
            b.HasKey(i => i.Id);
            b.Property(i => i.AvailableStock).IsRequired();
            b.Property(i => i.ReservedStock).IsRequired();
            b.Property(i => i.Version).IsRowVersion();
        });
    }
}
