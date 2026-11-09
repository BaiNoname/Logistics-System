using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TruckingSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMobileQrAttendance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AttendanceMaxAccuracyMeters",
                table: "Warehouses",
                type: "integer",
                nullable: false,
                defaultValue: 100);

            migrationBuilder.AddColumn<int>(
                name: "AttendanceRadiusMeters",
                table: "Warehouses",
                type: "integer",
                nullable: false,
                defaultValue: 150);

            migrationBuilder.CreateTable(
                name: "AttendanceAdjustmentRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EmployeeId = table.Column<int>(type: "integer", nullable: false),
                    WorkDate = table.Column<DateTime>(type: "date", nullable: false),
                    RequestedCheckInAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    RequestedCheckOutAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Reason = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ReviewedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ReviewNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttendanceAdjustmentRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AttendanceAdjustmentRequests_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AttendanceQrCodes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WarehouseId = table.Column<int>(type: "integer", nullable: false),
                    WorkDate = table.Column<DateTime>(type: "date", nullable: false),
                    Token = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttendanceQrCodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AttendanceQrCodes_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AttendanceRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EmployeeId = table.Column<int>(type: "integer", nullable: false),
                    WorkDate = table.Column<DateTime>(type: "date", nullable: false),
                    CheckInAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CheckInWarehouseId = table.Column<int>(type: "integer", nullable: true),
                    CheckOutAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CheckOutWarehouseId = table.Column<int>(type: "integer", nullable: true),
                    AdjustedCheckInAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    AdjustedCheckOutAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    AdjustmentRequestId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttendanceRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AttendanceRecords_AttendanceAdjustmentRequests_AdjustmentRe~",
                        column: x => x.AdjustmentRequestId,
                        principalTable: "AttendanceAdjustmentRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AttendanceRecords_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AttendanceRecords_Warehouses_CheckInWarehouseId",
                        column: x => x.CheckInWarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AttendanceRecords_Warehouses_CheckOutWarehouseId",
                        column: x => x.CheckOutWarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AttendanceScans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EmployeeId = table.Column<int>(type: "integer", nullable: false),
                    QrCodeId = table.Column<int>(type: "integer", nullable: true),
                    WarehouseId = table.Column<int>(type: "integer", nullable: true),
                    WorkDate = table.Column<DateTime>(type: "date", nullable: false),
                    ScannedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Lat = table.Column<double>(type: "double precision", nullable: true),
                    Lng = table.Column<double>(type: "double precision", nullable: true),
                    AccuracyMeters = table.Column<double>(type: "double precision", nullable: true),
                    DistanceMeters = table.Column<double>(type: "double precision", nullable: true),
                    IsMockLocation = table.Column<bool>(type: "boolean", nullable: false),
                    Action = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Result = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    RejectReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttendanceScans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AttendanceScans_AttendanceQrCodes_QrCodeId",
                        column: x => x.QrCodeId,
                        principalTable: "AttendanceQrCodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AttendanceScans_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AttendanceScans_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceAdjustmentRequests_EmployeeId_WorkDate",
                table: "AttendanceAdjustmentRequests",
                columns: new[] { "EmployeeId", "WorkDate" });

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceAdjustmentRequests_OnePending",
                table: "AttendanceAdjustmentRequests",
                columns: new[] { "EmployeeId", "WorkDate" },
                unique: true,
                filter: "\"Status\" = 'Pending'");

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceAdjustmentRequests_Status",
                table: "AttendanceAdjustmentRequests",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceQrCodes_Token",
                table: "AttendanceQrCodes",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceQrCodes_WarehouseId_WorkDate",
                table: "AttendanceQrCodes",
                columns: new[] { "WarehouseId", "WorkDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceRecords_AdjustmentRequestId",
                table: "AttendanceRecords",
                column: "AdjustmentRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceRecords_CheckInWarehouseId",
                table: "AttendanceRecords",
                column: "CheckInWarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceRecords_CheckOutWarehouseId",
                table: "AttendanceRecords",
                column: "CheckOutWarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceRecords_EmployeeId_WorkDate",
                table: "AttendanceRecords",
                columns: new[] { "EmployeeId", "WorkDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceRecords_WorkDate",
                table: "AttendanceRecords",
                column: "WorkDate");

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceScans_EmployeeId_WorkDate",
                table: "AttendanceScans",
                columns: new[] { "EmployeeId", "WorkDate" });

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceScans_QrCodeId",
                table: "AttendanceScans",
                column: "QrCodeId");

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceScans_WarehouseId",
                table: "AttendanceScans",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceScans_WorkDate",
                table: "AttendanceScans",
                column: "WorkDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AttendanceRecords");

            migrationBuilder.DropTable(
                name: "AttendanceScans");

            migrationBuilder.DropTable(
                name: "AttendanceAdjustmentRequests");

            migrationBuilder.DropTable(
                name: "AttendanceQrCodes");

            migrationBuilder.DropColumn(
                name: "AttendanceMaxAccuracyMeters",
                table: "Warehouses");

            migrationBuilder.DropColumn(
                name: "AttendanceRadiusMeters",
                table: "Warehouses");
        }
    }
}
