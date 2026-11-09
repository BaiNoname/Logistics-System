using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TruckingSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeePayroll : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "TerminationDate",
                table: "Employees",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EmployeeSalaries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EmployeeId = table.Column<int>(type: "integer", nullable: false),
                    EffectiveFrom = table.Column<DateTime>(type: "date", nullable: false),
                    MonthlySalary = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeSalaries", x => x.Id);
                    table.CheckConstraint("CK_EmployeeSalary_Positive", "\"MonthlySalary\" > 0");
                    table.ForeignKey(
                        name: "FK_EmployeeSalaries_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PayrollPeriods",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    Month = table.Column<int>(type: "integer", nullable: false),
                    Revision = table.Column<int>(type: "integer", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    ApprovedBy = table.Column<string>(type: "text", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayrollPeriods", x => x.Id);
                    table.CheckConstraint("CK_PayrollPeriod_Month", "\"Month\" BETWEEN 1 AND 12 AND \"Year\" BETWEEN 2000 AND 2100");
                });

            migrationBuilder.CreateTable(
                name: "MonthlyAttendances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EmployeeId = table.Column<int>(type: "integer", nullable: false),
                    PayrollPeriodId = table.Column<int>(type: "integer", nullable: false),
                    StandardDays = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    WorkedDays = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    PaidLeaveDays = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    HolidayDays = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    UnpaidLeaveDays = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    WeekdayOvertimeHours = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    RestDayOvertimeHours = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    HolidayOvertimeHours = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ConfirmedBy = table.Column<string>(type: "text", nullable: true),
                    ConfirmedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonthlyAttendances", x => x.Id);
                    table.CheckConstraint("CK_MonthlyAttendance_Days", "\"StandardDays\" > 0 AND \"StandardDays\" <= 31 AND \"WorkedDays\" >= 0 AND \"PaidLeaveDays\" >= 0 AND \"HolidayDays\" >= 0 AND \"UnpaidLeaveDays\" >= 0 AND \"WorkedDays\" + \"PaidLeaveDays\" + \"HolidayDays\" + \"UnpaidLeaveDays\" <= \"StandardDays\" AND \"WeekdayOvertimeHours\" >= 0 AND \"RestDayOvertimeHours\" >= 0 AND \"HolidayOvertimeHours\" >= 0");
                    table.ForeignKey(
                        name: "FK_MonthlyAttendances_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MonthlyAttendances_PayrollPeriods_PayrollPeriodId",
                        column: x => x.PayrollPeriodId,
                        principalTable: "PayrollPeriods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PayrollSlips",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EmployeeId = table.Column<int>(type: "integer", nullable: false),
                    PayrollPeriodId = table.Column<int>(type: "integer", nullable: false),
                    EmployeeCode = table.Column<string>(type: "text", nullable: false),
                    EmployeeName = table.Column<string>(type: "text", nullable: false),
                    DepartmentName = table.Column<string>(type: "text", nullable: false),
                    PositionName = table.Column<string>(type: "text", nullable: false),
                    MonthlySalary = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    SalaryEffectiveFrom = table.Column<DateTime>(type: "date", nullable: false),
                    StandardDays = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    WorkedDays = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    PaidLeaveDays = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    HolidayDays = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    UnpaidLeaveDays = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    WeekdayOvertimeHours = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    RestDayOvertimeHours = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    HolidayOvertimeHours = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    AttendanceNote = table.Column<string>(type: "text", nullable: false),
                    AttendanceConfirmedBy = table.Column<string>(type: "text", nullable: false),
                    AttendanceConfirmedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    EarnedSalary = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    GrossIncome = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Deductions = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    NetPay = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ApprovedNetPay = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    PaidAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayrollSlips", x => x.Id);
                    table.CheckConstraint("CK_PayrollSlip_Balance", "\"NetPay\" >= 0 AND \"PaidAmount\" >= 0 AND \"PaidAmount\" <= \"NetPay\"");
                    table.ForeignKey(
                        name: "FK_PayrollSlips_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PayrollSlips_PayrollPeriods_PayrollPeriodId",
                        column: x => x.PayrollPeriodId,
                        principalTable: "PayrollPeriods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PayrollItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PayrollSlipId = table.Column<int>(type: "integer", nullable: false),
                    Kind = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ProrateByAttendance = table.Column<bool>(type: "boolean", nullable: false),
                    CalculatedAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    IsAdjustment = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayrollItems", x => x.Id);
                    table.CheckConstraint("CK_PayrollItem_Amount", "\"Amount\" > 0 AND \"CalculatedAmount\" >= 0");
                    table.ForeignKey(
                        name: "FK_PayrollItems_PayrollSlips_PayrollSlipId",
                        column: x => x.PayrollSlipId,
                        principalTable: "PayrollSlips",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SalaryPayments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PayrollSlipId = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    PaidOn = table.Column<DateTime>(type: "date", nullable: false),
                    Method = table.Column<string>(type: "text", nullable: false),
                    Reference = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RecordedBy = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalaryPayments", x => x.Id);
                    table.CheckConstraint("CK_SalaryPayment_Positive", "\"Amount\" > 0");
                    table.ForeignKey(
                        name: "FK_SalaryPayments_PayrollSlips_PayrollSlipId",
                        column: x => x.PayrollSlipId,
                        principalTable: "PayrollSlips",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.AddCheckConstraint(
                name: "CK_Employee_EmploymentDates",
                table: "Employees",
                sql: "\"TerminationDate\" IS NULL OR \"TerminationDate\" >= \"HireDate\"::date");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeSalaries_EmployeeId_EffectiveFrom",
                table: "EmployeeSalaries",
                columns: new[] { "EmployeeId", "EffectiveFrom" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MonthlyAttendances_EmployeeId",
                table: "MonthlyAttendances",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_MonthlyAttendances_PayrollPeriodId_EmployeeId",
                table: "MonthlyAttendances",
                columns: new[] { "PayrollPeriodId", "EmployeeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PayrollItems_PayrollSlipId",
                table: "PayrollItems",
                column: "PayrollSlipId");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollPeriods_Year_Month",
                table: "PayrollPeriods",
                columns: new[] { "Year", "Month" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PayrollSlips_EmployeeId",
                table: "PayrollSlips",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollSlips_PayrollPeriodId_EmployeeId",
                table: "PayrollSlips",
                columns: new[] { "PayrollPeriodId", "EmployeeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalaryPayments_PayrollSlipId",
                table: "SalaryPayments",
                column: "PayrollSlipId");

            migrationBuilder.CreateIndex(
                name: "IX_SalaryPayments_Reference",
                table: "SalaryPayments",
                column: "Reference",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmployeeSalaries");

            migrationBuilder.DropTable(
                name: "MonthlyAttendances");

            migrationBuilder.DropTable(
                name: "PayrollItems");

            migrationBuilder.DropTable(
                name: "SalaryPayments");

            migrationBuilder.DropTable(
                name: "PayrollSlips");

            migrationBuilder.DropTable(
                name: "PayrollPeriods");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Employee_EmploymentDates",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "TerminationDate",
                table: "Employees");
        }
    }
}
