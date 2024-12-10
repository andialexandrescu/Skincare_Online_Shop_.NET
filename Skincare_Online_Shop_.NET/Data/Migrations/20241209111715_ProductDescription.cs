using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Skincare_Online_Shop_.NET.Data.Migrations
{
    /// <inheritdoc />
    public partial class ProductDescription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Content",
                table: "Products",
                newName: "Description");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Products",
                newName: "Content");
        }
    }
}
