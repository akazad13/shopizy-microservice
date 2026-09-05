using Microsoft.EntityFrameworkCore;
using Shopizy.LoyaltyService.Domain.Entities;

namespace Shopizy.LoyaltyService.Infrastructure.Persistence;

public class LoyaltyDbContext : DbContext
{
    public DbSet<LoyaltyAccount> LoyaltyAccounts => Set<LoyaltyAccount>();
    public DbSet<LoyaltyTransaction> LoyaltyTransactions => Set<LoyaltyTransaction>();
    public DbSet<GiftCard> GiftCards => Set<GiftCard>();

    public LoyaltyDbContext(DbContextOptions<LoyaltyDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<LoyaltyAccount>(b =>
        {
            b.HasKey(a => a.Id);
            b.HasIndex(a => a.CustomerId).IsUnique();

            b.HasMany(a => a.Transactions)
                .WithOne()
                .HasForeignKey(t => t.LoyaltyAccountId)
                .OnDelete(DeleteBehavior.Cascade);

            b.Navigation(a => a.Transactions).AutoInclude();
        });

        modelBuilder.Entity<LoyaltyTransaction>(b =>
        {
            b.HasKey(t => t.Id);
            b.Property(t => t.Description).HasMaxLength(250);
            b.HasIndex(t => t.LoyaltyAccountId);
            b.HasIndex(t => t.OrderId);
        });

        modelBuilder.Entity<GiftCard>(b =>
        {
            b.HasKey(g => g.Id);
            b.HasIndex(g => g.Code).IsUnique();
            b.Property(g => g.Code).HasMaxLength(32);
            b.Property(g => g.InitialBalance).HasPrecision(18, 2);
            b.Property(g => g.CurrentBalance).HasPrecision(18, 2);
            b.Property(g => g.Currency).HasMaxLength(3);
        });
    }
}
