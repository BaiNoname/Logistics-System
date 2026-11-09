using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TruckingSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SplitSurchargeLists : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""LOCK TABLE "SurchargePolicies" IN ACCESS EXCLUSIVE MODE;""");

            migrationBuilder.CreateTable(
                name: "SurchargeLists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    EffectiveDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Published = table.Column<bool>(type: "boolean", nullable: false),
                    Revision = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SurchargeLists", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SurchargeLine",
                columns: table => new
                {
                    SurchargeListId = table.Column<int>(type: "integer", nullable: false),
                    Kind = table.Column<int>(type: "integer", nullable: false),
                    Percent = table.Column<decimal>(type: "numeric(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SurchargeLine", x => new { x.SurchargeListId, x.Kind });
                    table.ForeignKey(
                        name: "FK_SurchargeLine_SurchargeLists_SurchargeListId",
                        column: x => x.SurchargeListId,
                        principalTable: "SurchargeLists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SurchargeLists_EffectiveDate",
                table: "SurchargeLists",
                column: "EffectiveDate",
                unique: true,
                filter: "\"Published\"");

            migrationBuilder.Sql("""
                INSERT INTO "SurchargeLists" ("Name", "EffectiveDate", "Published", "Revision")
                SELECT 'Bảng phụ phí ' || to_char("EffectiveDate", 'DD/MM/YYYY'), "EffectiveDate", TRUE, 1
                FROM "SurchargePolicies";

                INSERT INTO "SurchargeLine" ("SurchargeListId", "Kind", "Percent")
                SELECT l."Id", r.kind, r.percent
                FROM "SurchargePolicies" p JOIN "SurchargeLists" l USING ("EffectiveDate")
                CROSS JOIN LATERAL (VALUES (1, p."FragilePercent"), (2, p."HazmatPercent")) AS r(kind, percent);
                """);
            migrationBuilder.DropTable(
                name: "SurchargePolicies");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                LOCK TABLE "SurchargeLists", "SurchargeLine" IN ACCESS EXCLUSIVE MODE;
                DO $$ BEGIN
                    IF EXISTS (SELECT 1 FROM "SurchargeLists" WHERE NOT "Published") THEN
                        RAISE EXCEPTION 'Rollback cannot preserve draft surcharge lists.';
                    END IF;
                END $$;
                """);
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
            migrationBuilder.Sql("""
                INSERT INTO "SurchargePolicies" ("EffectiveDate", "FragilePercent", "HazmatPercent")
                SELECT l."EffectiveDate", f."Percent", h."Percent"
                FROM "SurchargeLists" l
                JOIN "SurchargeLine" f ON f."SurchargeListId" = l."Id" AND f."Kind" = 1
                JOIN "SurchargeLine" h ON h."SurchargeListId" = l."Id" AND h."Kind" = 2;
                """);
            migrationBuilder.DropTable(
                name: "SurchargeLine");

            migrationBuilder.DropTable(
                name: "SurchargeLists");
        }
    }
}
