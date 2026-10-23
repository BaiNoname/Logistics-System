using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TruckingSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddContractTariffRule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TariffRuleId",
                table: "Contracts",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_TariffRuleId",
                table: "Contracts",
                column: "TariffRuleId");

            migrationBuilder.AddForeignKey(
                name: "FK_Contracts_TariffRules_TariffRuleId",
                table: "Contracts",
                column: "TariffRuleId",
                principalTable: "TariffRules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contracts_TariffRules_TariffRuleId",
                table: "Contracts");

            migrationBuilder.DropIndex(
                name: "IX_Contracts_TariffRuleId",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "TariffRuleId",
                table: "Contracts");
        }
    }
}
