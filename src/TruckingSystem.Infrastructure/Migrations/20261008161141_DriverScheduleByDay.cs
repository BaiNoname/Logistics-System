using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TruckingSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DriverScheduleByDay : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DriverSchedules_DriverEmployeeId_ShiftDate_ShiftType",
                table: "DriverSchedules");

            migrationBuilder.CreateIndex(
                name: "IX_DriverSchedules_DriverEmployeeId_ShiftDate",
                table: "DriverSchedules",
                columns: new[] { "DriverEmployeeId", "ShiftDate" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DriverSchedules_DriverEmployeeId_ShiftDate",
                table: "DriverSchedules");

            migrationBuilder.CreateIndex(
                name: "IX_DriverSchedules_DriverEmployeeId_ShiftDate_ShiftType",
                table: "DriverSchedules",
                columns: new[] { "DriverEmployeeId", "ShiftDate", "ShiftType" },
                unique: true);
        }
    }
}
