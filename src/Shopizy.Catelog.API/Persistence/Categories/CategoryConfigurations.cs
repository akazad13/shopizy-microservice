using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shopizy.Catelog.API.Aggregates.Categories;
using Shopizy.Catelog.API.Aggregates.Categories.ValueObjects;

namespace Shopizy.Catelog.API.Persistence.Categories;

public sealed class CategoryConfigurations : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        ConfigureCategoriesTable(builder);
    }

    private static void ConfigureCategoriesTable(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");
        builder.HasKey(c => c.Id);

        builder
            .Property(c => c.Id)
            .ValueGeneratedNever()
            .HasConversion(id => id.Value, value => CategoryId.Create(value));

        builder.Property(c => c.Name).HasMaxLength(100);
        builder.Property(c => c.ParentId).IsRequired(false);

        builder.HasMany(c => c.Products).WithOne().OnDelete(DeleteBehavior.NoAction);
        builder.Navigation(p => p.Products).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
