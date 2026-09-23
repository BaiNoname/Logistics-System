using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TruckingSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceBinRowColumnWithRealPosition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Column",
                table: "StorageBins");

            migrationBuilder.DropColumn(
                name: "Row",
                table: "StorageBins");

            migrationBuilder.AddColumn<decimal>(
                name: "HeightMeters",
                table: "StorageBins",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PosX",
                table: "StorageBins",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PosY",
                table: "StorageBins",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "WidthMeters",
                table: "StorageBins",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HeightMeters",
                table: "StorageBins");

            migrationBuilder.DropColumn(
                name: "PosX",
                table: "StorageBins");

            migrationBuilder.DropColumn(
                name: "PosY",
                table: "StorageBins");

            migrationBuilder.DropColumn(
                name: "WidthMeters",
                table: "StorageBins");

            migrationBuilder.AddColumn<int>(
                name: "Column",
                table: "StorageBins",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Row",
                table: "StorageBins",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
