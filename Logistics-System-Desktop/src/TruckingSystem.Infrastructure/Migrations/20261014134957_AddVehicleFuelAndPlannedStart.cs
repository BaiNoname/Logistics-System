using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TruckingSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVehicleFuelAndPlannedStart : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "FuelLevelLiters",
                table: "Vehicles",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FuelLevelMeasuredAt",
                table: "Vehicles",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "FuelTankCapacityLiters",
                table: "Vehicles",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PlannedStartDate",
                table: "Trips",
                type: "timestamp without time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FuelLevelLiters",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "FuelLevelMeasuredAt",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "FuelTankCapacityLiters",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "PlannedStartDate",
                table: "Trips");
        }
    }
}
