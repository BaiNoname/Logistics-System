using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using TruckingSystem.Infrastructure.Persistence;

namespace TruckingSystem.Infrastructure.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20261022000000_MergeExpiredPriceListStatus")]
public class MergeExpiredPriceListStatus : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
        => migrationBuilder.Sql("UPDATE \"PriceLists\" SET \"Status\" = 1 WHERE \"Status\" = 2;");

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Keep inactive rows: the two previous statuses cannot be distinguished after merging.
    }
}
