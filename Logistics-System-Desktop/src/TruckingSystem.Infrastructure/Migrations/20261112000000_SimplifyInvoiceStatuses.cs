using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using TruckingSystem.Infrastructure.Persistence;

namespace TruckingSystem.Infrastructure.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20261112000000_SimplifyInvoiceStatuses")]
public class SimplifyInvoiceStatuses : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
        => migrationBuilder.Sql("UPDATE \"Invoices\" SET \"Status\" = 0 WHERE \"Status\" NOT IN (0, 3);");

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Các trạng thái cũ đã gộp không thể suy ra lại; giữ Unpaid=0 (Draft cũ) và Paid=3.
    }
}
