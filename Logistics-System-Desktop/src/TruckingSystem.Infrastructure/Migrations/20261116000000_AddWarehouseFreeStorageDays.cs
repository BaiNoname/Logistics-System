using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using TruckingSystem.Infrastructure.Persistence;

namespace TruckingSystem.Infrastructure.Migrations;

/// <summary>Phí lưu kho kiểu mới: mỗi kho có số ngày miễn phí (mặc định 5) tính từ ngày nhập kho đầu tiên.</summary>
[DbContext(typeof(AppDbContext))]
[Migration("20261116000000_AddWarehouseFreeStorageDays")]
public class AddWarehouseFreeStorageDays : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "FreeStorageDays",
            table: "Warehouses",
            type: "integer",
            nullable: false,
            defaultValue: 5);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "FreeStorageDays", table: "Warehouses");
    }
}
