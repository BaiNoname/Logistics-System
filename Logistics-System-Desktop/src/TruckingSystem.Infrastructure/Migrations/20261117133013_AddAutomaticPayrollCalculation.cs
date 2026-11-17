using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TruckingSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAutomaticPayrollCalculation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AutomaticCalculationError",
                table: "PayrollPeriods",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastAutomaticCalculationAt",
                table: "PayrollPeriods",
                type: "timestamp without time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AutomaticCalculationError",
                table: "PayrollPeriods");

            migrationBuilder.DropColumn(
                name: "LastAutomaticCalculationAt",
                table: "PayrollPeriods");
        }
    }
}
