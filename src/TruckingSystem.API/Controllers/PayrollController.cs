using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TruckingSystem.Application.Payroll;

namespace TruckingSystem.API.Controllers;

[ApiController]
[Route("api/payroll")]
[Authorize]
public class PayrollController(IPayrollService service) : ControllerBase
{
    private string Actor => User.FindFirstValue(ClaimTypes.NameIdentifier)!;
    private ActionResult<PayrollResult> Result(PayrollResult result) => result.Success ? Ok(result) : BadRequest(result);

    [HttpGet("employees")]
    [Authorize(Roles = "Admin,Accountant,Dispatcher")]
    public async Task<ActionResult<List<PayrollEmployeeDto>>> GetEmployees(
        [FromQuery, Range(2000, 2100)] int year, [FromQuery, Range(1, 12)] int month)
        => Ok(await service.GetEmployeesAsync(year, month));

    [HttpGet("employees/{employeeId:int}/salaries")]
    [Authorize(Roles = "Admin,Accountant")]
    public async Task<ActionResult<List<SalaryDto>>> GetSalaries(int employeeId) => Ok(await service.GetSalariesAsync(employeeId));

    [HttpPost("employees/{employeeId:int}/salaries")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<PayrollResult>> AddSalary(int employeeId, SalaryRequest request)
        => Result(await service.AddSalaryAsync(employeeId, request, Actor));

    [HttpGet("periods")]
    [Authorize(Roles = "Admin,Accountant,Dispatcher")]
    public async Task<ActionResult<List<PayrollPeriodDto>>> GetPeriods() => Ok(await service.GetPeriodsAsync());

    [HttpPost("periods")]
    [Authorize(Roles = "Admin,Accountant")]
    public async Task<ActionResult<PayrollResult>> CreatePeriod(PeriodRequest request)
        => Result(await service.CreatePeriodAsync(request, Actor));

    [HttpGet("periods/{periodId:int}/attendance")]
    [Authorize(Roles = "Admin,Accountant,Dispatcher")]
    public async Task<ActionResult<List<AttendanceDto>>> GetAttendance(int periodId) => Ok(await service.GetAttendanceAsync(periodId));

    [HttpGet("periods/{periodId:int}/driver-attendance/{employeeId:int}")]
    [Authorize(Roles = "Admin,Accountant,Dispatcher")]
    public async Task<ActionResult<DriverPayrollDaysDto>> GetDriverWorkDays(int periodId, int employeeId)
    {
        var days = await service.GetDriverWorkDaysAsync(periodId, employeeId);
        return days is null ? NotFound() : Ok(days);
    }

    [HttpPut("periods/{periodId:int}/driver-attendance/{employeeId:int}")]
    [Authorize(Roles = "Admin,Dispatcher")]
    public async Task<ActionResult<PayrollResult>> ImportDriverAttendance(int periodId, int employeeId, DriverAttendanceRequest request)
        => Result(await service.ImportDriverAttendanceAsync(periodId, employeeId, request, Actor));

    [HttpPut("periods/{periodId:int}/attendance/{employeeId:int}")]
    [Authorize(Roles = "Admin,Dispatcher")]
    public async Task<ActionResult<PayrollResult>> SaveAttendance(int periodId, int employeeId, AttendanceRequest request)
        => Result(await service.SaveAttendanceAsync(periodId, employeeId, request, Actor));

    [HttpPost("periods/{periodId:int}/attendance/{employeeId:int}/confirm")]
    [Authorize(Roles = "Admin,Dispatcher")]
    public async Task<ActionResult<PayrollResult>> ConfirmAttendance(int periodId, int employeeId, RevisionRequest request)
        => Result(await service.ConfirmAttendanceAsync(periodId, employeeId, request, Actor));

    [HttpGet("periods/{periodId:int}/slips")]
    [Authorize(Roles = "Admin,Accountant")]
    public async Task<ActionResult<List<PayrollSlipDto>>> GetSlips(int periodId) => Ok(await service.GetSlipsAsync(periodId));

    [HttpPost("periods/{periodId:int}/slips/{employeeId:int}")]
    [Authorize(Roles = "Admin,Accountant")]
    public async Task<ActionResult<PayrollResult>> GenerateSlip(int periodId, int employeeId, RevisionRequest request)
        => Result(await service.GenerateSlipAsync(periodId, employeeId, request, Actor));

    [HttpPost("slips/{slipId:int}/delete-draft")]
    [Authorize(Roles = "Admin,Accountant")]
    public async Task<ActionResult<PayrollResult>> DeleteDraft(int slipId, RevisionRequest request)
        => Result(await service.DeleteDraftSlipAsync(slipId, request, Actor));

    [HttpPost("slips/{slipId:int}/items")]
    [Authorize(Roles = "Admin,Accountant")]
    public async Task<ActionResult<PayrollResult>> AddItem(int slipId, PayrollItemRequest request)
        => Result(await service.AddItemAsync(slipId, request, false, Actor));

    [HttpPost("slips/{slipId:int}/items/{itemId:int}/delete")]
    [Authorize(Roles = "Admin,Accountant")]
    public async Task<ActionResult<PayrollResult>> DeleteItem(int slipId, int itemId, RevisionRequest request)
        => Result(await service.DeleteItemAsync(slipId, itemId, request, Actor));

    [HttpPost("periods/{periodId:int}/approve")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<PayrollResult>> Approve(int periodId, RevisionRequest request)
        => Result(await service.ApproveAsync(periodId, request, Actor));

    [HttpPost("slips/{slipId:int}/adjustments")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<PayrollResult>> Adjust(int slipId, PayrollItemRequest request)
        => Result(await service.AddItemAsync(slipId, request, true, Actor));

    [HttpPost("slips/{slipId:int}/payments")]
    [Authorize(Roles = "Admin,Accountant")]
    public async Task<ActionResult<PayrollResult>> Pay(int slipId, PaymentRequest request)
        => Result(await service.PayAsync(slipId, request, Actor));

    [HttpGet("slips/{slipId:int}/export")]
    [Authorize(Roles = "Admin,Accountant")]
    public async Task<ActionResult<string>> Export(int slipId)
    {
        var html = await service.ExportSlipAsync(slipId);
        return html is null ? NotFound() : new JsonResult(html);
    }
}
