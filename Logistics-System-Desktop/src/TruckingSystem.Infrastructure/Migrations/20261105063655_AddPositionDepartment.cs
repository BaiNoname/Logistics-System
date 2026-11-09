using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TruckingSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPositionDepartment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DepartmentId",
                table: "Positions",
                type: "integer",
                nullable: true);

            // Chỉ gán tự động khi tất cả nhân viên dùng chức danh cùng một phòng ban.
            migrationBuilder.Sql("""
                UPDATE "Positions" p
                SET "DepartmentId" = usage."DepartmentId"
                FROM (
                    SELECT "PositionId", MIN("DepartmentId") AS "DepartmentId"
                    FROM "Employees"
                    WHERE "PositionId" IS NOT NULL
                    GROUP BY "PositionId"
                    HAVING COUNT(DISTINCT "DepartmentId") = 1 AND COUNT(*) = COUNT("DepartmentId")
                ) usage
                WHERE p."Id" = usage."PositionId";
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Positions_DepartmentId",
                table: "Positions",
                column: "DepartmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Positions_Departments_DepartmentId",
                table: "Positions",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Positions_Departments_DepartmentId",
                table: "Positions");

            migrationBuilder.DropIndex(
                name: "IX_Positions_DepartmentId",
                table: "Positions");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                table: "Positions");
        }
    }
}
