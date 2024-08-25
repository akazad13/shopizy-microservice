using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shopizy.Cart.API.Aggregates;
using Shopizy.Cart.API.Aggregates.Entities;
using Shopizy.Cart.API.Aggregates.ValueObjects;

namespace Shopizy.Cart.API.Persistence;

public sealed class CartConfigurations : IEntityTypeConfiguration<CustomerCart>
{
    public void Configure(EntityTypeBuilder<CustomerCart> builder)
    {
        ConfigureCartsTable(builder);
        ConfigureCartProductIdsTable(builder);
    }

    private static void ConfigureCartsTable(EntityTypeBuilder<CustomerCart> builder)
    {
        builder.ToTable("Carts");
        builder.HasKey(c => c.Id);

        builder.HasIndex(c => c.CustomerId);

        builder
            .Property(c => c.Id)
            .ValueGeneratedNever()
            .HasConversion(id => id.Value, value => CartId.Create(value));

        builder.Property(o => o.CreatedOn).HasColumnType("smalldatetime");
        builder.Property(o => o.ModifiedOn).HasColumnType("smalldatetime").IsRequired(false);

        builder
            .Property(c => c.CustomerId)
            .HasConversion(id => id.Value, value => CustomerId.Create(value));
    }

    private static void ConfigureCartProductIdsTable(EntityTypeBuilder<CustomerCart> builder)
    {
        builder.OwnsMany(
            m => m.CartItems,
            ltmb =>
            {
                ltmb.ToTable("LineItems");
                ltmb.WithOwner().HasForeignKey("CartId");
                ltmb.HasKey(nameof(CartItem.Id), "CartId");
                ltmb.HasIndex("CartId", "ProductId").IsUnique();

                ltmb.Property(li => li.Id)
                    .ValueGeneratedNever()
                    .HasConversion(id => id.Value, value => CartItemId.Create(value));

                ltmb.Property(li => li.ProductId)
                    .ValueGeneratedNever()
                    .HasConversion(id => id.Value, value => ProductId.Create(value));
            }
        );
        builder.Navigation(p => p.CartItems).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
