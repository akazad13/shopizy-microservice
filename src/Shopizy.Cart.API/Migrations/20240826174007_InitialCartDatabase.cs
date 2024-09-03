﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shopizy.Cart.API.Migrations;

/// <inheritdoc />
public partial class InitialCartDatabase : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Carts",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CreatedOn = table.Column<DateTime>(type: "smalldatetime", nullable: false),
                ModifiedOn = table.Column<DateTime>(type: "smalldatetime", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Carts", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "CartItems",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CartId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Quantity = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_CartItems", x => new { x.Id, x.CartId });
                table.ForeignKey(
                    name: "FK_CartItems_Carts_CartId",
                    column: x => x.CartId,
                    principalTable: "Carts",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_CartItems_CartId_ProductId",
            table: "CartItems",
            columns: new[] { "CartId", "ProductId" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Carts_CustomerId",
            table: "Carts",
            column: "CustomerId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "CartItems");

        migrationBuilder.DropTable(
            name: "Carts");
    }
}
