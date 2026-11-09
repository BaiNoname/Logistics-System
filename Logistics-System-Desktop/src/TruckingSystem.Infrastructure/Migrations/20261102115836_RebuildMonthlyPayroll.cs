using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TruckingSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RebuildMonthlyPayroll : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CalculationVersion",
                table: "PayrollSlips",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "KpiPoints",
                table: "PayrollSlips",
                type: "numeric(24,8)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "KpiTarget",
                table: "PayrollSlips",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "SourceSnapshot",
                table: "PayrollSlips",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "PayrollKpiPolicies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EffectiveFrom = table.Column<DateTime>(type: "date", nullable: false),
                    KmPerPoint = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TargetPoints = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    FactorsJson = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayrollKpiPolicies", x => x.Id);
                    table.CheckConstraint("CK_PayrollKpiPolicy_Positive", "\"KmPerPoint\" > 0 AND \"TargetPoints\" > 0");
                });

            migrationBuilder.CreateTable(
                name: "PayrollMonthlyAdjustments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EmployeeId = table.Column<int>(type: "integer", nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    Month = table.Column<int>(type: "integer", nullable: false),
                    Kind = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayrollMonthlyAdjustments", x => x.Id);
                    table.CheckConstraint("CK_PayrollMonthlyAdjustment_Valid", "\"Amount\" > 0 AND \"Month\" BETWEEN 1 AND 12 AND \"Year\" BETWEEN 2000 AND 2100");
                    table.ForeignKey(
                        name: "FK_PayrollMonthlyAdjustments_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PayrollKpiPolicies_EffectiveFrom",
                table: "PayrollKpiPolicies",
                column: "EffectiveFrom",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PayrollMonthlyAdjustments_EmployeeId",
                table: "PayrollMonthlyAdjustments",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollMonthlyAdjustments_Year_Month_EmployeeId",
                table: "PayrollMonthlyAdjustments",
                columns: new[] { "Year", "Month", "EmployeeId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PayrollKpiPolicies");

            migrationBuilder.DropTable(
                name: "PayrollMonthlyAdjustments");

            migrationBuilder.DropColumn(
                name: "CalculationVersion",
                table: "PayrollSlips");

            migrationBuilder.DropColumn(
                name: "KpiPoints",
                table: "PayrollSlips");

            migrationBuilder.DropColumn(
                name: "KpiTarget",
                table: "PayrollSlips");

            migrationBuilder.DropColumn(
                name: "SourceSnapshot",
                table: "PayrollSlips");
        }
    }
}
