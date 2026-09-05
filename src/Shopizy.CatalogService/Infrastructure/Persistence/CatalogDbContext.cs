using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Shopizy.CatalogService.Domain.Entities;
using Shopizy.CatalogService.Domain.Enums;
using Shopizy.CatalogService.Domain.ValueObjects;

namespace Shopizy.CatalogService.Infrastructure.Persistence;

public sealed class CatalogDbContext : DbContext
{
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();

    public CatalogDbContext(DbContextOptions<CatalogDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // JSON converter for dictionary attributes
        var jsonConverter = new ValueConverter<Dictionary<string, string>, string>(
            v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
            v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, (JsonSerializerOptions?)null) ?? new Dictionary<string, string>());

        // Category
        modelBuilder.Entity<Category>(builder =>
        {
            builder.ToTable("categories");
            builder.HasKey(c => c.Id);

            builder.Property(c => c.Name).HasMaxLength(100).IsRequired();
            builder.Property(c => c.Slug).HasMaxLength(120).IsRequired();
            builder.HasIndex(c => c.Slug).IsUnique();

            builder.Property(c => c.Description).HasMaxLength(500);
            builder.Property(c => c.IsActive).IsRequired();
            builder.Property(c => c.CreatedAtUtc).IsRequired();

            builder.HasOne<Category>()
                .WithMany(c => c.SubCategories)
                .HasForeignKey(c => c.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Brand
        modelBuilder.Entity<Brand>(builder =>
        {
            builder.ToTable("brands");
            builder.HasKey(b => b.Id);

            builder.Property(b => b.Name).HasMaxLength(100).IsRequired();
            builder.Property(b => b.Slug).HasMaxLength(120).IsRequired();
            builder.HasIndex(b => b.Slug).IsUnique();

            builder.Property(b => b.Description).HasMaxLength(1000);
            builder.Property(b => b.WebsiteUrl).HasMaxLength(255);
            builder.Property(b => b.LogoUrl).HasMaxLength(500);
            builder.Property(b => b.IsActive).IsRequired();
            builder.Property(b => b.CreatedAtUtc).IsRequired();
        });

        // Product
        modelBuilder.Entity<Product>(builder =>
        {
            builder.ToTable("products");
            builder.HasKey(p => p.Id);

            builder.Property(p => p.Name).HasMaxLength(200).IsRequired();
            builder.Property(p => p.Slug).HasMaxLength(220).IsRequired();
            builder.HasIndex(p => p.Slug).IsUnique();

            builder.Property(p => p.Description).HasMaxLength(4000).IsRequired();
            builder.Property(p => p.Status).HasConversion<int>().IsRequired();

            builder.Property(p => p.Version).IsConcurrencyToken();

            builder.Property(p => p.CreatedAtUtc).IsRequired();

            builder.OwnsOne(p => p.BasePrice, priceBuilder =>
            {
                priceBuilder.Property(m => m.Amount).HasColumnName("base_price_amount").HasPrecision(18, 2).IsRequired();
                priceBuilder.Property(m => m.Currency).HasColumnName("base_price_currency").HasMaxLength(3).IsRequired();
            });

            builder.HasOne<Category>()
                .WithMany()
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<Brand>()
                .WithMany()
                .HasForeignKey(p => p.BrandId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(p => p.Images)
                .WithOne()
                .HasForeignKey(i => i.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(p => p.Variants)
                .WithOne()
                .HasForeignKey(v => v.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ProductVariant
        modelBuilder.Entity<ProductVariant>(builder =>
        {
            builder.ToTable("product_variants");
            builder.HasKey(v => v.Id);

            builder.Property(v => v.Sku).HasMaxLength(64).IsRequired();
            builder.HasIndex(v => v.Sku).IsUnique();

            builder.Property(v => v.Barcode).HasMaxLength(64);
            builder.Property(v => v.StockQuantity).IsRequired();
            builder.Property(v => v.IsActive).IsRequired();
            builder.Property(v => v.CreatedAtUtc).IsRequired();

            builder.OwnsOne(v => v.Price, priceBuilder =>
            {
                priceBuilder.Property(m => m.Amount).HasColumnName("price_amount").HasPrecision(18, 2).IsRequired();
                priceBuilder.Property(m => m.Currency).HasColumnName("price_currency").HasMaxLength(3).IsRequired();
            });

            builder.Property(v => v.Attributes)
                .HasConversion(jsonConverter)
                .HasColumnName("attributes");
        });

        // ProductImage
        modelBuilder.Entity<ProductImage>(builder =>
        {
            builder.ToTable("product_images");
            builder.HasKey(i => i.Id);

            builder.Property(i => i.Url).HasMaxLength(1000).IsRequired();
            builder.Property(i => i.AltText).HasMaxLength(200);
            builder.Property(i => i.DisplayOrder).IsRequired();
            builder.Property(i => i.IsMain).IsRequired();
            builder.Property(i => i.CreatedAtUtc).IsRequired();
        });
    }
}
