using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using TruckingSystem.Infrastructure.Persistence;

namespace TruckingSystem.Infrastructure.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20261102160000_AddFreightVectors")]
public sealed class AddFreightVectors : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("""
        CREATE EXTENSION IF NOT EXISTS vector;
        CREATE TABLE "FreightVectors" (
            "TariffId" integer PRIMARY KEY REFERENCES "TariffRules" ("Id") ON DELETE CASCADE,
            "PriceListId" integer NOT NULL,
            "Text" text NOT NULL, "Hash" varchar(64) NOT NULL, "Model" text NOT NULL,
            "Embedding" vector(768) NOT NULL, "IndexedAtUtc" timestamptz NOT NULL
        );
        CREATE INDEX "IX_FreightVectors_PriceListId" ON "FreightVectors" ("PriceListId");
        """);
    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("DROP TABLE \"FreightVectors\";");
}
