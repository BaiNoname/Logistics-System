using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TruckingSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMaxDiscountPercent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MaxDiscountPercent",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Value = table.Column<int>(type: "integer", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaxDiscountPercent", x => x.Id);
                    table.CheckConstraint("CK_MaxDiscountPercent_Value", "\"Id\" = 1 AND \"Value\" BETWEEN 0 AND 100");
                });

            migrationBuilder.InsertData(
                table: "MaxDiscountPercent",
                columns: new[] { "Id", "UpdatedAt", "Value" },
                values: new object[] { 1, new DateTime(2026, 11, 5, 0, 0, 0, 0, DateTimeKind.Utc), 20 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MaxDiscountPercent");
        }
    }
}
