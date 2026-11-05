using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TruckingSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AllowSameDaySurchargeLists : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SurchargeLists_EffectiveDate",
                table: "SurchargeLists");

            migrationBuilder.CreateIndex(
                name: "IX_SurchargeLists_EffectiveDate",
                table: "SurchargeLists",
                column: "EffectiveDate",
                filter: "\"Published\"");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SurchargeLists_EffectiveDate",
                table: "SurchargeLists");

            migrationBuilder.CreateIndex(
                name: "IX_SurchargeLists_EffectiveDate",
                table: "SurchargeLists",
                column: "EffectiveDate",
                unique: true,
                filter: "\"Published\"");
        }
    }
}
