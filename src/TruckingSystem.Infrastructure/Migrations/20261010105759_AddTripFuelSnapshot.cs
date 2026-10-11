using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TruckingSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTripFuelSnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "FuelConsumptionPer100KmAtStart",
                table: "Trips",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FuelTypeAtStart",
                table: "Trips",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "FuelUnitPriceAtStart",
                table: "Trips",
                type: "numeric(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FuelConsumptionPer100KmAtStart",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "FuelTypeAtStart",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "FuelUnitPriceAtStart",
                table: "Trips");
        }
    }
}
