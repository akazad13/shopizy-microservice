using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shopizy.Catelog.API.Aggregates.Categories.ValueObjects;
using Shopizy.Catelog.API.Aggregates.Products;
using Shopizy.Catelog.API.Aggregates.Products.Entities;
using Shopizy.Catelog.API.Aggregates.Products.ValueObjects;

namespace Shopizy.Catelog.API.Persistence.Products;

public sealed class ProductConfigurations : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        ConfigureProductsTable(builder);
        ConfigureProductImagesTable(builder);
    }

    private static void ConfigureProductsTable(EntityTypeBuilder<Product> builder)
    {
        _ = builder.ToTable("Products").HasKey(p => p.Id);

        _ = builder
            .Property(p => p.Id)
            .ValueGeneratedNever()
            .HasConversion(id => id.Value, value => ProductId.Create(value));

        _ = builder.Property(p => p.Name).HasMaxLength(100).IsRequired();
        _ = builder.Property(p => p.Description).HasMaxLength(200);
        _ = builder.Property(p => p.SKU).HasMaxLength(50);
        _ = builder.Property(p => p.StockQuantity);
        _ = builder.Property(p => p.Discount).HasPrecision(18, 2).IsRequired(false);
        _ = builder.Property(p => p.Brand).HasMaxLength(50).IsRequired(false);
        _ = builder.Property(p => p.Barcode).HasMaxLength(50).IsRequired(false);
        _ = builder.Property(p => p.Tags).HasMaxLength(200).IsRequired(false);
        _ = builder.Property(p => p.BreadCrums).HasMaxLength(100).IsRequired(false);
        _ = builder.Property(p => p.CreatedOn).HasColumnType("smalldatetime");
        _ = builder.Property(p => p.ModifiedOn).HasColumnType("smalldatetime").IsRequired(false);

        _ = builder.OwnsOne(
            p => p.UnitPrice,
            pb =>
            {
                _ = pb.Property(p => p.Amount).HasPrecision(18, 2);
            }
        );
        _ = builder.OwnsOne(
            p => p.AverageRating,
            avrb =>
            {
                _ = avrb.Property(avr => avr.Value).HasPrecision(18, 2);
            }
        );

        _ = builder
            .Property(p => p.CategoryId)
            .HasConversion(id => id.Value, value => CategoryId.Create(value));

        _ = builder.Navigation(p => p.ProductImages).UsePropertyAccessMode(PropertyAccessMode.Field);
        _ = builder.Navigation(p => p.ProductReviews).UsePropertyAccessMode(PropertyAccessMode.Field);
    }

    private static void ConfigureProductImagesTable(EntityTypeBuilder<Product> builder)
    {
        _ = builder.OwnsMany(
            p => p.ProductImages,
            pib =>
            {
                _ = pib.ToTable("ProductImages");

                _ = pib.WithOwner().HasForeignKey("ProductId");
                _ = pib.HasKey("ProductId", nameof(ProductImage.Id));

                _ = pib.Property(pi => pi.PublicId).HasMaxLength(100);
                _ = pib.Property(pi => pi.Id)
                    .ValueGeneratedNever()
                    .HasConversion(id => id.Value, value => ProductImageId.Create(value));
                _ = pib.Property(pi => pi.ImageUrl).IsRequired();
            }
        );
    }
}

