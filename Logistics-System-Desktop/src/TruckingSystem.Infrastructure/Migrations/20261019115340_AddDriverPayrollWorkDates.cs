using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TruckingSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDriverPayrollWorkDates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DriverWorkDates",
                table: "PayrollSlips",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "WorkFrom",
                table: "PayrollSlips",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "WorkTo",
                table: "PayrollSlips",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DriverWorkDates",
                table: "MonthlyAttendances",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DriverWorkDates",
                table: "PayrollSlips");

            migrationBuilder.DropColumn(
                name: "WorkFrom",
                table: "PayrollSlips");

            migrationBuilder.DropColumn(
                name: "WorkTo",
                table: "PayrollSlips");

            migrationBuilder.DropColumn(
                name: "DriverWorkDates",
                table: "MonthlyAttendances");
        }
    }
}
