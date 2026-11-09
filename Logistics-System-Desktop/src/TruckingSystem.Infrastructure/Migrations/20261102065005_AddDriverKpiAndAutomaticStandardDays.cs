using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TruckingSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDriverKpiAndAutomaticStandardDays : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_MonthlyAttendance_Days",
                table: "MonthlyAttendances");

            migrationBuilder.AddColumn<bool>(
                name: "IsDriverKpi",
                table: "PayrollItems",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "DriverKpiRates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VehicleType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    VehicleFactor = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    AmountPerPoint = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    KmPerPoint = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DriverKpiRates", x => x.Id);
                    table.CheckConstraint("CK_DriverKpiRate_Positive", "\"VehicleFactor\" > 0 AND \"AmountPerPoint\" > 0 AND \"KmPerPoint\" > 0");
                });

            migrationBuilder.AddCheckConstraint(
                name: "CK_MonthlyAttendance_Days",
                table: "MonthlyAttendances",
                sql: "\"StandardDays\" > 0 AND \"StandardDays\" <= 31 AND \"WorkedDays\" >= 0 AND \"PaidLeaveDays\" >= 0 AND \"HolidayDays\" >= 0 AND \"UnpaidLeaveDays\" >= 0 AND \"WorkedDays\" + \"PaidLeaveDays\" + \"HolidayDays\" + \"UnpaidLeaveDays\" <= CASE WHEN \"DriverWorkDates\" IS NOT NULL THEN 31 ELSE \"StandardDays\" END AND \"WeekdayOvertimeHours\" >= 0 AND \"RestDayOvertimeHours\" >= 0 AND \"HolidayOvertimeHours\" >= 0");

            migrationBuilder.CreateIndex(
                name: "IX_DriverKpiRates_VehicleType",
                table: "DriverKpiRates",
                column: "VehicleType",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DriverKpiRates");

            migrationBuilder.DropCheckConstraint(
                name: "CK_MonthlyAttendance_Days",
                table: "MonthlyAttendances");

            migrationBuilder.DropColumn(
                name: "IsDriverKpi",
                table: "PayrollItems");

            migrationBuilder.AddCheckConstraint(
                name: "CK_MonthlyAttendance_Days",
                table: "MonthlyAttendances",
                sql: "\"StandardDays\" > 0 AND \"StandardDays\" <= 31 AND \"WorkedDays\" >= 0 AND \"PaidLeaveDays\" >= 0 AND \"HolidayDays\" >= 0 AND \"UnpaidLeaveDays\" >= 0 AND \"WorkedDays\" + \"PaidLeaveDays\" + \"HolidayDays\" + \"UnpaidLeaveDays\" <= \"StandardDays\" AND \"WeekdayOvertimeHours\" >= 0 AND \"RestDayOvertimeHours\" >= 0 AND \"HolidayOvertimeHours\" >= 0");
        }
    }
}
