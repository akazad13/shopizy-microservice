using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shopizy.Ordering.API.Migrations;

/// <inheritdoc />
public partial class InitialOrderDatabase : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Orders",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                DeliveryCharge_Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                DeliveryCharge_Currency = table.Column<int>(type: "int", nullable: false),
                OrderStatus = table.Column<int>(type: "int", nullable: false),
                CancellationReason = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                PromoCode = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                ShippingAddress_Street = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                ShippingAddress_City = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                ShippingAddress_State = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                ShippingAddress_Country = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                ShippingAddress_ZipCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                PaymentStatus = table.Column<int>(type: "int", nullable: false),
                CreatedOn = table.Column<DateTime>(type: "smalldatetime", nullable: false),
                ModifiedOn = table.Column<DateTime>(type: "smalldatetime", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Orders", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "OrderItems",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                PictureUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                UnitPrice_Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                UnitPrice_Currency = table.Column<int>(type: "int", nullable: false),
                Quantity = table.Column<int>(type: "int", nullable: false),
                Discount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_OrderItems", x => new { x.Id, x.OrderId });
                table.ForeignKey(
                    name: "FK_OrderItems_Orders_OrderId",
                    column: x => x.OrderId,
                    principalTable: "Orders",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_OrderItems_OrderId",
            table: "OrderItems",
            column: "OrderId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "OrderItems");

        migrationBuilder.DropTable(
            name: "Orders");
    }
}
