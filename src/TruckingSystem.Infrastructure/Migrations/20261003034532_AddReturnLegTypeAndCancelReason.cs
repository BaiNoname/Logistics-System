using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TruckingSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReturnLegTypeAndCancelReason : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CancelReason",
                table: "Trips",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CancelReason",
                table: "Trips");
        }
    }
}
