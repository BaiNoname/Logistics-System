using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TruckingSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWarehouseIdToVehicleAndDriverProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "WarehouseId",
                table: "Vehicles",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WarehouseId",
                table: "DriverProfiles",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_WarehouseId",
                table: "Vehicles",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_DriverProfiles_WarehouseId",
                table: "DriverProfiles",
                column: "WarehouseId");

            migrationBuilder.AddForeignKey(
                name: "FK_DriverProfiles_Warehouses_WarehouseId",
                table: "DriverProfiles",
                column: "WarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Vehicles_Warehouses_WarehouseId",
                table: "Vehicles",
                column: "WarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DriverProfiles_Warehouses_WarehouseId",
                table: "DriverProfiles");

            migrationBuilder.DropForeignKey(
                name: "FK_Vehicles_Warehouses_WarehouseId",
                table: "Vehicles");

            migrationBuilder.DropIndex(
                name: "IX_Vehicles_WarehouseId",
                table: "Vehicles");

            migrationBuilder.DropIndex(
                name: "IX_DriverProfiles_WarehouseId",
                table: "DriverProfiles");

            migrationBuilder.DropColumn(
                name: "WarehouseId",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "WarehouseId",
                table: "DriverProfiles");
        }
    }
}
