using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Skincare_Online_Shop_.NET.Data.Migrations
{
    /// <inheritdoc />
    public partial class TeosImplementationProductCategoryReview : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Rating",
                table: "Reviews",
                newName: "Grade");

            migrationBuilder.RenameColumn(
                name: "Quantity",
                table: "Products",
                newName: "Stock");

            migrationBuilder.RenameColumn(
                name: "Quantity",
                table: "CartProducts",
                newName: "Stock");

            migrationBuilder.AlterColumn<string>(
                name: "Ingredients",
                table: "Products",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Grade",
                table: "Reviews",
                newName: "Rating");

            migrationBuilder.RenameColumn(
                name: "Stock",
                table: "Products",
                newName: "Quantity");

            migrationBuilder.RenameColumn(
                name: "Stock",
                table: "CartProducts",
                newName: "Quantity");

            migrationBuilder.AlterColumn<string>(
                name: "Ingredients",
                table: "Products",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000);
        }
    }
}
