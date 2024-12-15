using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Skincare_Online_Shop_.NET.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveRatingAddRequestStatusProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Rating",
                table: "Products");

            migrationBuilder.AddColumn<int>(
                name: "RequestStatus",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RequestStatus",
                table: "Products");

            migrationBuilder.AddColumn<float>(
                name: "Rating",
                table: "Products",
                type: "real",
                nullable: true);
        }
    }
}
