using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TruckingSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDataConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GoodsIssues_CargoLines_CargoLineId",
                table: "GoodsIssues");

            migrationBuilder.DropForeignKey(
                name: "FK_GoodsIssues_TransportOrders_TransportOrderId",
                table: "GoodsIssues");

            migrationBuilder.DropForeignKey(
                name: "FK_GoodsIssues_Warehouses_WarehouseId",
                table: "GoodsIssues");

            migrationBuilder.DropForeignKey(
                name: "FK_GoodsReceipts_CargoLines_CargoLineId",
                table: "GoodsReceipts");

            migrationBuilder.DropForeignKey(
                name: "FK_GoodsReceipts_TransportOrders_TransportOrderId",
                table: "GoodsReceipts");

            migrationBuilder.DropForeignKey(
                name: "FK_GoodsReceipts_Warehouses_WarehouseId",
                table: "GoodsReceipts");

            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_Customers_CustomerId",
                table: "Invoices");

            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_TransportOrders_TransportOrderId",
                table: "Invoices");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Invoices_InvoiceId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_StorageAssignments_StorageBins_StorageBinId",
                table: "StorageAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_TransportOrders_Customers_CustomerId",
                table: "TransportOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_TransportOrders_Warehouses_WarehouseId",
                table: "TransportOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_Trips_TransportOrders_TransportOrderId",
                table: "Trips");

            migrationBuilder.DropForeignKey(
                name: "FK_WarehouseStocks_CargoLines_CargoLineId",
                table: "WarehouseStocks");

            migrationBuilder.DropForeignKey(
                name: "FK_WarehouseStocks_Warehouses_WarehouseId",
                table: "WarehouseStocks");

            migrationBuilder.DropIndex(
                name: "IX_TripVideoSegments_TripId_StartedAt",
                table: "TripVideoSegments");

            migrationBuilder.DropIndex(
                name: "IX_StorageBins_WarehouseZoneId",
                table: "StorageBins");

            migrationBuilder.DropIndex(
                name: "IX_FuelPriceHistories_FuelProviderId",
                table: "FuelPriceHistories");

            migrationBuilder.DropIndex(
                name: "IX_DriverSchedules_DriverEmployeeId",
                table: "DriverSchedules");

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "WarehouseStocks",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AlterColumn<decimal>(
                name: "CapacityM3",
                table: "Vehicles",
                type: "numeric(18,4)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)");

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "Vehicles",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "TransportOrders",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AlterColumn<decimal>(
                name: "CurrentUsedM3",
                table: "StorageBins",
                type: "numeric(18,4)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "CapacityM3",
                table: "StorageBins",
                type: "numeric(18,4)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)");

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "StorageBins",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "StorageAssignments",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "GoodsIssues",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AlterColumn<decimal>(
                name: "VolumeM3",
                table: "CargoLines",
                type: "numeric(18,4)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_WarehouseStock_QtyOnHand_NonNegative",
                table: "WarehouseStocks",
                sql: "\"QtyOnHand\" >= 0");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_PlateNumber",
                table: "Vehicles",
                column: "PlateNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TripVideoSegments_TripId_StartedAt",
                table: "TripVideoSegments",
                columns: new[] { "TripId", "StartedAt" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TransportOrders_Code",
                table: "TransportOrders",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StorageBins_WarehouseZoneId_Code",
                table: "StorageBins",
                columns: new[] { "WarehouseZoneId", "Code" },
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_StorageBin_CurrentUsedM3_NonNegative",
                table: "StorageBins",
                sql: "\"CurrentUsedM3\" >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_StorageAssignment_QtyStored_NonNegative",
                table: "StorageAssignments",
                sql: "\"QtyStored\" >= 0");

            migrationBuilder.CreateIndex(
                name: "UX_FuelPrice_Market_Type_Date",
                table: "FuelPriceHistories",
                columns: new[] { "FuelType", "EffectiveDate" },
                unique: true,
                filter: "\"FuelProviderId\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "UX_FuelPrice_Provider_Type_Date",
                table: "FuelPriceHistories",
                columns: new[] { "FuelProviderId", "FuelType", "EffectiveDate" },
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_FuelPrice_Positive",
                table: "FuelPriceHistories",
                sql: "\"Price\" > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_FuelLog_Positive",
                table: "FuelLogs",
                sql: "\"Liters\" > 0 AND \"Cost\" >= 0");

            migrationBuilder.CreateIndex(
                name: "IX_DriverSchedules_DriverEmployeeId_ShiftDate_ShiftType",
                table: "DriverSchedules",
                columns: new[] { "DriverEmployeeId", "ShiftDate", "ShiftType" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_GoodsIssues_CargoLines_CargoLineId",
                table: "GoodsIssues",
                column: "CargoLineId",
                principalTable: "CargoLines",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GoodsIssues_TransportOrders_TransportOrderId",
                table: "GoodsIssues",
                column: "TransportOrderId",
                principalTable: "TransportOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GoodsIssues_Warehouses_WarehouseId",
                table: "GoodsIssues",
                column: "WarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GoodsReceipts_CargoLines_CargoLineId",
                table: "GoodsReceipts",
                column: "CargoLineId",
                principalTable: "CargoLines",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GoodsReceipts_TransportOrders_TransportOrderId",
                table: "GoodsReceipts",
                column: "TransportOrderId",
                principalTable: "TransportOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GoodsReceipts_Warehouses_WarehouseId",
                table: "GoodsReceipts",
                column: "WarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_Customers_CustomerId",
                table: "Invoices",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_TransportOrders_TransportOrderId",
                table: "Invoices",
                column: "TransportOrderId",
                principalTable: "TransportOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Invoices_InvoiceId",
                table: "Payments",
                column: "InvoiceId",
                principalTable: "Invoices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StorageAssignments_StorageBins_StorageBinId",
                table: "StorageAssignments",
                column: "StorageBinId",
                principalTable: "StorageBins",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TransportOrders_Customers_CustomerId",
                table: "TransportOrders",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TransportOrders_Warehouses_WarehouseId",
                table: "TransportOrders",
                column: "WarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Trips_TransportOrders_TransportOrderId",
                table: "Trips",
                column: "TransportOrderId",
                principalTable: "TransportOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WarehouseStocks_CargoLines_CargoLineId",
                table: "WarehouseStocks",
                column: "CargoLineId",
                principalTable: "CargoLines",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WarehouseStocks_Warehouses_WarehouseId",
                table: "WarehouseStocks",
                column: "WarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GoodsIssues_CargoLines_CargoLineId",
                table: "GoodsIssues");

            migrationBuilder.DropForeignKey(
                name: "FK_GoodsIssues_TransportOrders_TransportOrderId",
                table: "GoodsIssues");

            migrationBuilder.DropForeignKey(
                name: "FK_GoodsIssues_Warehouses_WarehouseId",
                table: "GoodsIssues");

            migrationBuilder.DropForeignKey(
                name: "FK_GoodsReceipts_CargoLines_CargoLineId",
                table: "GoodsReceipts");

            migrationBuilder.DropForeignKey(
                name: "FK_GoodsReceipts_TransportOrders_TransportOrderId",
                table: "GoodsReceipts");

            migrationBuilder.DropForeignKey(
                name: "FK_GoodsReceipts_Warehouses_WarehouseId",
                table: "GoodsReceipts");

            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_Customers_CustomerId",
                table: "Invoices");

            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_TransportOrders_TransportOrderId",
                table: "Invoices");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Invoices_InvoiceId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_StorageAssignments_StorageBins_StorageBinId",
                table: "StorageAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_TransportOrders_Customers_CustomerId",
                table: "TransportOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_TransportOrders_Warehouses_WarehouseId",
                table: "TransportOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_Trips_TransportOrders_TransportOrderId",
                table: "Trips");

            migrationBuilder.DropForeignKey(
                name: "FK_WarehouseStocks_CargoLines_CargoLineId",
                table: "WarehouseStocks");

            migrationBuilder.DropForeignKey(
                name: "FK_WarehouseStocks_Warehouses_WarehouseId",
                table: "WarehouseStocks");

            migrationBuilder.DropCheckConstraint(
                name: "CK_WarehouseStock_QtyOnHand_NonNegative",
                table: "WarehouseStocks");

            migrationBuilder.DropIndex(
                name: "IX_Vehicles_PlateNumber",
                table: "Vehicles");

            migrationBuilder.DropIndex(
                name: "IX_TripVideoSegments_TripId_StartedAt",
                table: "TripVideoSegments");

            migrationBuilder.DropIndex(
                name: "IX_TransportOrders_Code",
                table: "TransportOrders");

            migrationBuilder.DropIndex(
                name: "IX_StorageBins_WarehouseZoneId_Code",
                table: "StorageBins");

            migrationBuilder.DropCheckConstraint(
                name: "CK_StorageBin_CurrentUsedM3_NonNegative",
                table: "StorageBins");

            migrationBuilder.DropCheckConstraint(
                name: "CK_StorageAssignment_QtyStored_NonNegative",
                table: "StorageAssignments");

            migrationBuilder.DropIndex(
                name: "UX_FuelPrice_Market_Type_Date",
                table: "FuelPriceHistories");

            migrationBuilder.DropIndex(
                name: "UX_FuelPrice_Provider_Type_Date",
                table: "FuelPriceHistories");

            migrationBuilder.DropCheckConstraint(
                name: "CK_FuelPrice_Positive",
                table: "FuelPriceHistories");

            migrationBuilder.DropCheckConstraint(
                name: "CK_FuelLog_Positive",
                table: "FuelLogs");

            migrationBuilder.DropIndex(
                name: "IX_DriverSchedules_DriverEmployeeId_ShiftDate_ShiftType",
                table: "DriverSchedules");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "WarehouseStocks");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "TransportOrders");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "StorageBins");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "StorageAssignments");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "GoodsIssues");

            migrationBuilder.AlterColumn<decimal>(
                name: "CapacityM3",
                table: "Vehicles",
                type: "numeric(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,4)");

            migrationBuilder.AlterColumn<decimal>(
                name: "CurrentUsedM3",
                table: "StorageBins",
                type: "numeric(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,4)");

            migrationBuilder.AlterColumn<decimal>(
                name: "CapacityM3",
                table: "StorageBins",
                type: "numeric(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,4)");

            migrationBuilder.AlterColumn<decimal>(
                name: "VolumeM3",
                table: "CargoLines",
                type: "numeric(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,4)");

            migrationBuilder.CreateIndex(
                name: "IX_TripVideoSegments_TripId_StartedAt",
                table: "TripVideoSegments",
                columns: new[] { "TripId", "StartedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_StorageBins_WarehouseZoneId",
                table: "StorageBins",
                column: "WarehouseZoneId");

            migrationBuilder.CreateIndex(
                name: "IX_FuelPriceHistories_FuelProviderId",
                table: "FuelPriceHistories",
                column: "FuelProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_DriverSchedules_DriverEmployeeId",
                table: "DriverSchedules",
                column: "DriverEmployeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_GoodsIssues_CargoLines_CargoLineId",
                table: "GoodsIssues",
                column: "CargoLineId",
                principalTable: "CargoLines",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GoodsIssues_TransportOrders_TransportOrderId",
                table: "GoodsIssues",
                column: "TransportOrderId",
                principalTable: "TransportOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GoodsIssues_Warehouses_WarehouseId",
                table: "GoodsIssues",
                column: "WarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GoodsReceipts_CargoLines_CargoLineId",
                table: "GoodsReceipts",
                column: "CargoLineId",
                principalTable: "CargoLines",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GoodsReceipts_TransportOrders_TransportOrderId",
                table: "GoodsReceipts",
                column: "TransportOrderId",
                principalTable: "TransportOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GoodsReceipts_Warehouses_WarehouseId",
                table: "GoodsReceipts",
                column: "WarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_Customers_CustomerId",
                table: "Invoices",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_TransportOrders_TransportOrderId",
                table: "Invoices",
                column: "TransportOrderId",
                principalTable: "TransportOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Invoices_InvoiceId",
                table: "Payments",
                column: "InvoiceId",
                principalTable: "Invoices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StorageAssignments_StorageBins_StorageBinId",
                table: "StorageAssignments",
                column: "StorageBinId",
                principalTable: "StorageBins",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TransportOrders_Customers_CustomerId",
                table: "TransportOrders",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TransportOrders_Warehouses_WarehouseId",
                table: "TransportOrders",
                column: "WarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Trips_TransportOrders_TransportOrderId",
                table: "Trips",
                column: "TransportOrderId",
                principalTable: "TransportOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WarehouseStocks_CargoLines_CargoLineId",
                table: "WarehouseStocks",
                column: "CargoLineId",
                principalTable: "CargoLines",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WarehouseStocks_Warehouses_WarehouseId",
                table: "WarehouseStocks",
                column: "WarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
