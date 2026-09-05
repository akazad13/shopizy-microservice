using Microsoft.EntityFrameworkCore;
using Shopizy.CartAbandonmentWorker.Domain.Entities;

namespace Shopizy.CartAbandonmentWorker.Infrastructure.Persistence;

public class AbandonmentDbContext : DbContext
{
    public DbSet<AbandonedCartRecord> AbandonedCartRecords => Set<AbandonedCartRecord>();

    public AbandonmentDbContext(DbContextOptions<AbandonmentDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AbandonedCartRecord>(b =>
        {
            b.HasKey(r => r.Id);
            b.HasIndex(r => r.CartId);
            b.HasIndex(r => r.CustomerId);
            b.HasIndex(r => r.RecoveryToken).IsUnique();
            b.Property(r => r.CustomerEmail).HasMaxLength(250);
            b.Property(r => r.CartTotal).HasPrecision(18, 2);
            b.Property(r => r.RecoveryToken).HasMaxLength(64);
        });
    }
}
