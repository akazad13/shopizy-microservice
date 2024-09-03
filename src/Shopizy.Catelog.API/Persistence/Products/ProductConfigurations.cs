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
        builder.ToTable("Products").HasKey(p => p.Id);

        builder
            .Property(p => p.Id)
            .ValueGeneratedNever()
            .HasConversion(id => id.Value, value => ProductId.Create(value));

        builder.Property(p => p.Name).HasMaxLength(100).IsRequired();
        builder.Property(p => p.Description).HasMaxLength(200);
        builder.Property(p => p.SKU).HasMaxLength(50);
        builder.Property(p => p.StockQuantity);
        builder.Property(p => p.Discount).HasPrecision(18, 2).IsRequired(false);
        builder.Property(p => p.Brand).HasMaxLength(50).IsRequired(false);
        builder.Property(p => p.Barcode).HasMaxLength(50).IsRequired(false);
        builder.Property(p => p.Tags).HasMaxLength(200).IsRequired(false);
        builder.Property(p => p.BreadCrums).HasMaxLength(100).IsRequired(false);
        builder.Property(p => p.CreatedOn).HasColumnType("smalldatetime");
        builder.Property(p => p.ModifiedOn).HasColumnType("smalldatetime").IsRequired(false);

        builder.OwnsOne(
            p => p.UnitPrice,
            pb =>
            {
                pb.Property(p => p.Amount).HasPrecision(18, 2);
            }
        );
        builder.OwnsOne(
            p => p.AverageRating,
            avrb =>
            {
                avrb.Property(avr => avr.Value).HasPrecision(18, 2);
            }
        );

        builder
            .Property(p => p.CategoryId)
            .HasConversion(id => id.Value, value => CategoryId.Create(value));

        builder.Navigation(p => p.ProductImages).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(p => p.ProductReviews).UsePropertyAccessMode(PropertyAccessMode.Field);
    }

    private static void ConfigureProductImagesTable(EntityTypeBuilder<Product> builder)
    {
        builder.OwnsMany(
            p => p.ProductImages,
            pib =>
            {
                pib.ToTable("ProductImages");

                pib.WithOwner().HasForeignKey("ProductId");
                pib.HasKey("ProductId", nameof(ProductImage.Id));

                pib.Property(pi => pi.PublicId).HasMaxLength(100);
                pib.Property(pi => pi.Id)
                    .ValueGeneratedNever()
                    .HasConversion(id => id.Value, value => ProductImageId.Create(value));
                pib.Property(pi => pi.ImageUrl).IsRequired();
            }
        );
    }
}

