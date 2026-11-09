using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TruckingSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UnifyPayrollCutoff25 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "WorkFrom",
                table: "MonthlyAttendances",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "WorkTo",
                table: "MonthlyAttendances",
                type: "date",
                nullable: true);

            // Record the old basis without changing hours, confirmations or approved money.
            migrationBuilder.Sql("""
                UPDATE "MonthlyAttendances" a
                SET "WorkFrom" = CASE WHEN a."DriverWorkDates" IS NOT NULL
                        THEN (make_date(p."Year", p."Month", 25) - INTERVAL '1 month' + INTERVAL '1 day')::date
                        ELSE make_date(p."Year", p."Month", 1) END,
                    "WorkTo" = CASE WHEN a."DriverWorkDates" IS NOT NULL
                        THEN make_date(p."Year", p."Month", 25)
                        ELSE (make_date(p."Year", p."Month", 1) + INTERVAL '1 month' - INTERVAL '1 day')::date END
                FROM "PayrollPeriods" p WHERE p."Id" = a."PayrollPeriodId";
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WorkFrom",
                table: "MonthlyAttendances");

            migrationBuilder.DropColumn(
                name: "WorkTo",
                table: "MonthlyAttendances");
        }
    }
}
