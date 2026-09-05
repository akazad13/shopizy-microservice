using Microsoft.EntityFrameworkCore;
using Shopizy.ReviewService.Domain.Entities;

namespace Shopizy.ReviewService.Infrastructure.Persistence;

public class ReviewDbContext : DbContext
{
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<ReviewVote> ReviewVotes => Set<ReviewVote>();
    public DbSet<Wishlist> Wishlists => Set<Wishlist>();
    public DbSet<WishlistItem> WishlistItems => Set<WishlistItem>();

    public ReviewDbContext(DbContextOptions<ReviewDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Review>(b =>
        {
            b.HasKey(r => r.Id);
            b.Property(r => r.CustomerName).HasMaxLength(150);
            b.Property(r => r.Title).HasMaxLength(200);
            b.Property(r => r.Comment).HasMaxLength(4000);
            b.HasIndex(r => r.ProductId);
            b.HasIndex(r => r.CustomerId);
        });

        modelBuilder.Entity<ReviewVote>(b =>
        {
            b.HasKey(v => v.Id);
            b.HasIndex(v => new { v.ReviewId, v.UserId }).IsUnique();
        });

        modelBuilder.Entity<Wishlist>(b =>
        {
            b.HasKey(w => w.Id);
            b.HasIndex(w => w.CustomerId).IsUnique();

            b.HasMany(w => w.Items)
                .WithOne()
                .HasForeignKey(i => i.WishlistId)
                .OnDelete(DeleteBehavior.Cascade);

            b.Navigation(w => w.Items).AutoInclude();
        });

        modelBuilder.Entity<WishlistItem>(b =>
        {
            b.HasKey(i => i.Id);
            b.Property(i => i.ProductName).HasMaxLength(250);
            b.Property(i => i.Sku).HasMaxLength(100);
            b.Property(i => i.PriceSnapshot).HasPrecision(18, 2);
            b.HasIndex(i => new { i.WishlistId, i.ProductId }).IsUnique();
        });
    }
}
