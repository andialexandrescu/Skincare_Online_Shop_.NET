using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Skincare_Online_Shop_.NET.Data.Migrations
{
    /// <inheritdoc />
    public partial class ValidationForReviewWithinProductsShowView : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Score",
                table: "Products");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Products",
                newName: "Content");

            migrationBuilder.AddColumn<float>(
                name: "Rating",
                table: "Products",
                type: "real",
                nullable: false,
                defaultValue: 0f);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Rating",
                table: "Products");

            migrationBuilder.RenameColumn(
                name: "Content",
                table: "Products",
                newName: "Description");

            migrationBuilder.AddColumn<float>(
                name: "Score",
                table: "Products",
                type: "real",
                nullable: true);
        }
    }
}
