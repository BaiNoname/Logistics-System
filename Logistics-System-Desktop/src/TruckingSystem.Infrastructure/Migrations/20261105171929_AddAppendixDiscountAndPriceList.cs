using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TruckingSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAppendixDiscountAndPriceList : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "TariffRuleId",
                table: "ContractAppendices",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountPercent",
                table: "ContractAppendices",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PriceListId",
                table: "ContractAppendices",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContractAppendices_PriceListId",
                table: "ContractAppendices",
                column: "PriceListId");

            migrationBuilder.AddForeignKey(
                name: "FK_ContractAppendices_PriceLists_PriceListId",
                table: "ContractAppendices",
                column: "PriceListId",
                principalTable: "PriceLists",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DO $$ BEGIN
                    IF EXISTS (SELECT 1 FROM "ContractAppendices" WHERE "TariffRuleId" IS NULL) THEN
                        RAISE EXCEPTION 'Cannot roll back while appendices without a legacy tariff exist.';
                    END IF;
                END $$;
                """);
            migrationBuilder.DropForeignKey(
                name: "FK_ContractAppendices_PriceLists_PriceListId",
                table: "ContractAppendices");

            migrationBuilder.DropIndex(
                name: "IX_ContractAppendices_PriceListId",
                table: "ContractAppendices");

            migrationBuilder.DropColumn(
                name: "DiscountPercent",
                table: "ContractAppendices");

            migrationBuilder.DropColumn(
                name: "PriceListId",
                table: "ContractAppendices");

            migrationBuilder.AlterColumn<int>(
                name: "TariffRuleId",
                table: "ContractAppendices",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }
    }
}
