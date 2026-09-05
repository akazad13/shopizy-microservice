using Microsoft.EntityFrameworkCore;
using Shopizy.PromotionService.Domain.Entities;

namespace Shopizy.PromotionService.Infrastructure.Persistence;

public sealed class PromotionDbContext : DbContext
{
    public PromotionDbContext(DbContextOptions<PromotionDbContext> options) : base(options) { }

    public DbSet<PromotionCampaign> Campaigns => Set<PromotionCampaign>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<PromotionCampaign>(b =>
        {
            b.HasKey(c => c.Id);
            b.HasIndex(c => c.Code).IsUnique();
            b.Property(c => c.Code).HasMaxLength(50).IsRequired();
            b.Property(c => c.Description).HasMaxLength(250);
            b.Property(c => c.DiscountValue).HasPrecision(18, 2);
            b.Property(c => c.MinimumSpend).HasPrecision(18, 2);
            b.Property(c => c.MaxDiscountCap).HasPrecision(18, 2);
            b.Property(c => c.EligibleCategory).HasMaxLength(100);
        });
    }
}
