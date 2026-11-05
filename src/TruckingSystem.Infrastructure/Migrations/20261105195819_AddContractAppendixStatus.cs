using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TruckingSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddContractAppendixStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ContractAppendices_ContractId",
                table: "ContractAppendices");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "ContractAppendices",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql("""
                WITH ranked AS (
                    SELECT "Id", ROW_NUMBER() OVER (
                        PARTITION BY "ContractId" ORDER BY "CreatedAt" DESC, "Id" DESC
                    ) AS position
                    FROM "ContractAppendices"
                )
                UPDATE "ContractAppendices" AS appendix
                SET "Status" = 1
                FROM ranked
                WHERE appendix."Id" = ranked."Id" AND ranked.position > 1;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_ContractAppendices_ContractId",
                table: "ContractAppendices",
                column: "ContractId",
                unique: true,
                filter: "\"Status\" = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ContractAppendices_ContractId",
                table: "ContractAppendices");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "ContractAppendices");

            migrationBuilder.CreateIndex(
                name: "IX_ContractAppendices_ContractId",
                table: "ContractAppendices",
                column: "ContractId");
        }
    }
}
