using Microsoft.EntityFrameworkCore;
using Shopizy.PaymentService.Domain.Entities;

namespace Shopizy.PaymentService.Infrastructure.Persistence;

public sealed class PaymentDbContext : DbContext
{
    public DbSet<PaymentTransaction> Payments => Set<PaymentTransaction>();

    public PaymentDbContext(DbContextOptions<PaymentDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<PaymentTransaction>(b =>
        {
            b.HasKey(p => p.Id);
            b.Property(p => p.OrderId).IsRequired();
            b.Property(p => p.CustomerId).IsRequired();
            b.Property(p => p.Status).IsRequired();
            b.Property(p => p.GatewayTransactionId).HasMaxLength(128);
            b.Property(p => p.FailureReason).HasMaxLength(256);
            b.Property(p => p.CreatedAtUtc).IsRequired();

            b.OwnsOne(p => p.Amount, m =>
            {
                m.Property(x => x.Amount).HasColumnName("Amount").HasPrecision(18, 2).IsRequired();
                m.Property(x => x.Currency).HasColumnName("Currency").HasMaxLength(3).IsRequired();
            });

            b.OwnsOne(p => p.PaymentMethod, pm =>
            {
                pm.Property(x => x.Token).HasColumnName("PaymentToken").HasMaxLength(128).IsRequired();
                pm.Property(x => x.Brand).HasColumnName("CardBrand").HasMaxLength(32).IsRequired();
                pm.Property(x => x.Last4).HasColumnName("CardLast4").HasMaxLength(4).IsRequired();
            });

            b.OwnsOne(p => p.Refund, r =>
            {
                r.Property(x => x.Id).HasColumnName("RefundId");
                r.Property(x => x.RefundReference).HasColumnName("RefundReference").HasMaxLength(128);
                r.Property(x => x.Reason).HasColumnName("RefundReason").HasMaxLength(256);
                r.Property(x => x.CreatedAtUtc).HasColumnName("RefundCreatedAtUtc");
                r.OwnsOne(x => x.Amount, m =>
                {
                    m.Property(a => a.Amount).HasColumnName("RefundAmount").HasPrecision(18, 2);
                    m.Property(a => a.Currency).HasColumnName("RefundCurrency").HasMaxLength(3);
                });
            });
        });
    }
}
