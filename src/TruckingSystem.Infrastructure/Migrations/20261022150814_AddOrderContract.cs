using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TruckingSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderContract : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ContractId",
                table: "TransportOrders",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TransportOrders_ContractId",
                table: "TransportOrders",
                column: "ContractId");

            migrationBuilder.AddForeignKey(
                name: "FK_TransportOrders_Contracts_ContractId",
                table: "TransportOrders",
                column: "ContractId",
                principalTable: "Contracts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TransportOrders_Contracts_ContractId",
                table: "TransportOrders");

            migrationBuilder.DropIndex(
                name: "IX_TransportOrders_ContractId",
                table: "TransportOrders");

            migrationBuilder.DropColumn(
                name: "ContractId",
                table: "TransportOrders");
        }
    }
}
