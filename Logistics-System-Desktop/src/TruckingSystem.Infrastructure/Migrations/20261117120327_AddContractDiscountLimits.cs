using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TruckingSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddContractDiscountLimits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "LateDeliveryDiscountPercent",
                table: "MaxDiscountPercent",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "LateDeliveryDiscountPercent",
                table: "ContractAppendices",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_MaxDiscountPercent_LateDeliveryDiscountPercent",
                table: "MaxDiscountPercent",
                sql: "\"LateDeliveryDiscountPercent\" BETWEEN 0 AND 100");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ContractAppendices_LateDeliveryDiscountPercent",
                table: "ContractAppendices",
                sql: "\"LateDeliveryDiscountPercent\" BETWEEN 0 AND 100");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_MaxDiscountPercent_LateDeliveryDiscountPercent",
                table: "MaxDiscountPercent");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ContractAppendices_LateDeliveryDiscountPercent",
                table: "ContractAppendices");

            migrationBuilder.DropColumn(
                name: "LateDeliveryDiscountPercent",
                table: "MaxDiscountPercent");

            migrationBuilder.DropColumn(
                name: "LateDeliveryDiscountPercent",
                table: "ContractAppendices");
        }
    }
}
