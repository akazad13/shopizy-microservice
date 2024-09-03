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
        _ = builder.ToTable("Carts")
               .HasKey(c => c.Id);

        _ = builder.HasIndex(c => c.CustomerId);

        _ = builder
            .Property(c => c.Id)
            .ValueGeneratedNever()
            .HasConversion(id => id.Value, value => CartId.Create(value));

        _ = builder.Property(o => o.CreatedOn).HasColumnType("smalldatetime");
        _ = builder.Property(o => o.ModifiedOn).HasColumnType("smalldatetime").IsRequired(false);

        _ = builder
            .Property(c => c.CustomerId)
            .HasConversion(id => id.Value, value => CustomerId.Create(value));
    }

    private static void ConfigureCartProductIdsTable(EntityTypeBuilder<CustomerCart> builder)
    {
        _ = builder.OwnsMany(
            m => m.CartItems,
            ltmb =>
            {
                _ = ltmb.ToTable("CartItems");
                _ = ltmb.WithOwner().HasForeignKey("CartId");
                _ = ltmb.HasKey(nameof(CartItem.Id), "CartId");
                _ = ltmb.HasIndex("CartId", "ProductId").IsUnique();

                _ = ltmb.Property(li => li.Id)
                    .ValueGeneratedNever()
                    .HasConversion(id => id.Value, value => CartItemId.Create(value));

                _ = ltmb.Property(li => li.ProductId)
                    .ValueGeneratedNever()
                    .HasConversion(id => id.Value, value => ProductId.Create(value));
            }
        );
        _ = builder.Navigation(p => p.CartItems).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
