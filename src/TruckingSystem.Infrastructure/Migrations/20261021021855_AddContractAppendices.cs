using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TruckingSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddContractAppendices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ContractAppendices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ContractId = table.Column<int>(type: "integer", nullable: false),
                    IssueDate = table.Column<DateTime>(type: "date", nullable: false),
                    EffectiveDate = table.Column<DateTime>(type: "date", nullable: false),
                    TariffRuleId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractAppendices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContractAppendices_Contracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "Contracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContractAppendices_TariffRules_TariffRuleId",
                        column: x => x.TariffRuleId,
                        principalTable: "TariffRules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContractAppendices_Code",
                table: "ContractAppendices",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContractAppendices_ContractId",
                table: "ContractAppendices",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractAppendices_TariffRuleId",
                table: "ContractAppendices",
                column: "TariffRuleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContractAppendices");
        }
    }
}
