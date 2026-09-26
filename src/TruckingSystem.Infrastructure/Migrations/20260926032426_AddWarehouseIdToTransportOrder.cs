using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TruckingSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWarehouseIdToTransportOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "WarehouseId",
                table: "TransportOrders",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            // Gán tạm các đơn CŨ (đang có WarehouseId=0 do vừa thêm cột) sang
            // đúng 1 kho CÓ THẬT trong hệ thống — tránh vi phạm khóa ngoại
            // ngay bên dưới, vì Id=0 không tồn tại trong bảng Warehouses.
            migrationBuilder.Sql(@"UPDATE ""TransportOrders"" SET ""WarehouseId"" = (SELECT ""Id"" FROM ""Warehouses"" ORDER BY ""Id"" LIMIT 1) WHERE ""WarehouseId"" = 0;");

            migrationBuilder.CreateIndex(
                name: "IX_TransportOrders_WarehouseId",
                table: "TransportOrders",
                column: "WarehouseId");

            migrationBuilder.AddForeignKey(
                name: "FK_TransportOrders_Warehouses_WarehouseId",
                table: "TransportOrders",
                column: "WarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TransportOrders_Warehouses_WarehouseId",
                table: "TransportOrders");

            migrationBuilder.DropIndex(
                name: "IX_TransportOrders_WarehouseId",
                table: "TransportOrders");

            migrationBuilder.DropColumn(
                name: "WarehouseId",
                table: "TransportOrders");
        }
    }
}