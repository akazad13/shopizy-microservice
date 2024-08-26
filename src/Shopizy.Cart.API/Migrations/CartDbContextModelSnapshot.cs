﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Shopizy.Cart.API.Persistence;

#nullable disable

namespace Shopizy.Cart.API.Migrations
{
    [DbContext(typeof(CartDbContext))]
    partial class CartDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("Shopizy.Cart.API.Aggregates.CustomerCart", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnType("smalldatetime");

                    b.Property<Guid>("CustomerId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime?>("ModifiedOn")
                        .HasColumnType("smalldatetime");

                    b.HasKey("Id");

                    b.HasIndex("CustomerId");

                    b.ToTable("Carts", (string)null);
                });

            modelBuilder.Entity("Shopizy.Cart.API.Aggregates.CustomerCart", b =>
                {
                    b.OwnsMany("Shopizy.Cart.API.Aggregates.Entities.CartItem", "CartItems", b1 =>
                        {
                            b1.Property<Guid>("Id")
                                .HasColumnType("uniqueidentifier");

                            b1.Property<Guid>("CartId")
                                .HasColumnType("uniqueidentifier");

                            b1.Property<Guid>("ProductId")
                                .HasColumnType("uniqueidentifier");

                            b1.Property<int>("Quantity")
                                .HasColumnType("int");

                            b1.HasKey("Id", "CartId");

                            b1.HasIndex("CartId", "ProductId")
                                .IsUnique();

                            b1.ToTable("CartItems", (string)null);

                            b1.WithOwner()
                                .HasForeignKey("CartId");
                        });

                    b.Navigation("CartItems");
                });
#pragma warning restore 612, 618
        }
    }
}
