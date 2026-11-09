using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TruckingSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGlobalSurchargePolicies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SurchargePolicies",
                columns: table => new
                {
                    EffectiveDate = table.Column<DateOnly>(type: "date", nullable: false),
                    FragilePercent = table.Column<decimal>(type: "numeric(18,2)", precision: 5, scale: 2, nullable: false),
                    HazmatPercent = table.Column<decimal>(type: "numeric(18,2)", precision: 5, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SurchargePolicies", x => x.EffectiveDate);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SurchargePolicies");
        }
    }
}
