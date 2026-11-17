using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TruckingSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddContractLateDeliveryDiscount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "LateDeliveryDiscountPercent",
                table: "Contracts",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Contracts_LateDeliveryDiscountPercent",
                table: "Contracts",
                sql: "\"LateDeliveryDiscountPercent\" BETWEEN 0 AND 100");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Contracts_LateDeliveryDiscountPercent",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "LateDeliveryDiscountPercent",
                table: "Contracts");
        }
    }
}
