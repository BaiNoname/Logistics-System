using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TruckingSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStockPerformerAndReversal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GoodsIssues_Employees_IssuedByEmployeeId",
                table: "GoodsIssues");

            migrationBuilder.DropForeignKey(
                name: "FK_GoodsReceipts_Employees_ReceivedByEmployeeId",
                table: "GoodsReceipts");

            migrationBuilder.AlterColumn<int>(
                name: "ReceivedByEmployeeId",
                table: "GoodsReceipts",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<string>(
                name: "PerformedByUserName",
                table: "GoodsReceipts",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "IssuedByEmployeeId",
                table: "GoodsIssues",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<string>(
                name: "PerformedByUserName",
                table: "GoodsIssues",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReversedAt",
                table: "GoodsIssues",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReversedByUserName",
                table: "GoodsIssues",
                type: "text",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_GoodsIssues_Employees_IssuedByEmployeeId",
                table: "GoodsIssues",
                column: "IssuedByEmployeeId",
                principalTable: "Employees",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GoodsReceipts_Employees_ReceivedByEmployeeId",
                table: "GoodsReceipts",
                column: "ReceivedByEmployeeId",
                principalTable: "Employees",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GoodsIssues_Employees_IssuedByEmployeeId",
                table: "GoodsIssues");

            migrationBuilder.DropForeignKey(
                name: "FK_GoodsReceipts_Employees_ReceivedByEmployeeId",
                table: "GoodsReceipts");

            migrationBuilder.DropColumn(
                name: "PerformedByUserName",
                table: "GoodsReceipts");

            migrationBuilder.DropColumn(
                name: "PerformedByUserName",
                table: "GoodsIssues");

            migrationBuilder.DropColumn(
                name: "ReversedAt",
                table: "GoodsIssues");

            migrationBuilder.DropColumn(
                name: "ReversedByUserName",
                table: "GoodsIssues");

            migrationBuilder.AlterColumn<int>(
                name: "ReceivedByEmployeeId",
                table: "GoodsReceipts",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "IssuedByEmployeeId",
                table: "GoodsIssues",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_GoodsIssues_Employees_IssuedByEmployeeId",
                table: "GoodsIssues",
                column: "IssuedByEmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GoodsReceipts_Employees_ReceivedByEmployeeId",
                table: "GoodsReceipts",
                column: "ReceivedByEmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
