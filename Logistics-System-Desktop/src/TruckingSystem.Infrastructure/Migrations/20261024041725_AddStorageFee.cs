using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TruckingSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStorageFee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "StorageFeePerM3PerDay",
                table: "Warehouses",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "DeliveryDeadlineChanges",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TransportOrderId = table.Column<int>(type: "integer", nullable: false),
                    OldDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    NewDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    RequestedBy = table.Column<int>(type: "integer", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: false),
                    ChangedByUserName = table.Column<string>(type: "text", nullable: true),
                    DelayDays = table.Column<int>(type: "integer", nullable: false),
                    StorageFeePerM3PerDay = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    VolumeM3AtChange = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    EstimatedFee = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryDeadlineChanges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeliveryDeadlineChanges_TransportOrders_TransportOrderId",
                        column: x => x.TransportOrderId,
                        principalTable: "TransportOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryDeadlineChanges_TransportOrderId",
                table: "DeliveryDeadlineChanges",
                column: "TransportOrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeliveryDeadlineChanges");

            migrationBuilder.DropColumn(
                name: "StorageFeePerM3PerDay",
                table: "Warehouses");
        }
    }
}
