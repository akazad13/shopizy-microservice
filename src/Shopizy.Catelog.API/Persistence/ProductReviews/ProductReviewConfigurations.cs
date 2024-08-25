using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Shopizy.Catelog.API.Aggregates.ProductReviews;
using Shopizy.Catelog.API.Aggregates.ProductReviews.ValueObjects;
using Shopizy.Catelog.API.Aggregates.Products.ValueObjects;

namespace Shopizy.Catelog.API.Persistence.ProductReviews;

public class ProductReviewConfigurations : IEntityTypeConfiguration<ProductReview>
{
    public void Configure(EntityTypeBuilder<ProductReview> builder)
    {
        ConfigureProductReviewsTable(builder);
    }

    private static void ConfigureProductReviewsTable(EntityTypeBuilder<ProductReview> builder)
    {
        builder.ToTable("ProductReviews");
        builder.HasKey(pr => pr.Id);
        builder
            .Property(pr => pr.Id)
            .ValueGeneratedNever()
            .HasConversion(id => id.Value, value => ProductReviewId.Create(value));

        builder.Property(pr => pr.Comment).HasMaxLength(1000).IsRequired(false);
        builder.Property(pr => pr.CreatedOn).HasColumnType("smalldatetime");
        builder.Property(pr => pr.ModifiedOn).HasColumnType("smalldatetime").IsRequired(false);

        builder.OwnsOne(
            pr => pr.Rating,
            rb =>
            {
                rb.Property(r => r.Value).HasPrecision(18, 2);
            }
        );

        builder
            .Property(pr => pr.CustomerId)
            .HasConversion(id => id.Value, value => CustomerId.Create(value));
        builder
            .Property(pr => pr.ProductId)
            .HasConversion(id => id.Value, value => ProductId.Create(value));
    }
}
