using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shopizy.Catelog.API.Migrations;

/// <inheritdoc />
public partial class InitialCatelogDatabase : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Categories",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Categories", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Products",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                SKU = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                StockQuantity = table.Column<int>(type: "int", nullable: false),
                UnitPrice_Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                UnitPrice_Currency = table.Column<int>(type: "int", nullable: false),
                Discount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                Brand = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                Barcode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                Tags = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                AverageRating_Value = table.Column<double>(type: "float(18)", precision: 18, scale: 2, nullable: false),
                AverageRating_NumRatings = table.Column<int>(type: "int", nullable: false),
                BreadCrums = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                CreatedOn = table.Column<DateTime>(type: "smalldatetime", nullable: false),
                ModifiedOn = table.Column<DateTime>(type: "smalldatetime", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Products", x => x.Id);
                table.ForeignKey(
                    name: "FK_Products_Categories_CategoryId",
                    column: x => x.CategoryId,
                    principalTable: "Categories",
                    principalColumn: "Id");
            });

        migrationBuilder.CreateTable(
            name: "ProductImages",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Seq = table.Column<int>(type: "int", nullable: false),
                PublicId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ProductImages", x => new { x.ProductId, x.Id });
                table.ForeignKey(
                    name: "FK_ProductImages_Products_ProductId",
                    column: x => x.ProductId,
                    principalTable: "Products",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "ProductReviews",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Rating_Value = table.Column<double>(type: "float(18)", precision: 18, scale: 2, nullable: false),
                Comment = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                CreatedOn = table.Column<DateTime>(type: "smalldatetime", nullable: false),
                ModifiedOn = table.Column<DateTime>(type: "smalldatetime", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ProductReviews", x => x.Id);
                table.ForeignKey(
                    name: "FK_ProductReviews_Products_ProductId",
                    column: x => x.ProductId,
                    principalTable: "Products",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_ProductReviews_ProductId",
            table: "ProductReviews",
            column: "ProductId");

        migrationBuilder.CreateIndex(
            name: "IX_Products_CategoryId",
            table: "Products",
            column: "CategoryId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "ProductImages");

        migrationBuilder.DropTable(
            name: "ProductReviews");

        migrationBuilder.DropTable(
            name: "Products");

        migrationBuilder.DropTable(
            name: "Categories");
    }
}
