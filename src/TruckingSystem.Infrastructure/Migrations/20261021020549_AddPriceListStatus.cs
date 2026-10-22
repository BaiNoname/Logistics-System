using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TruckingSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPriceListStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "PriceLists",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "PriceLists");
        }
    }
}
