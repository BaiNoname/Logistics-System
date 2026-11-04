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

    [HttpPost("months/generate")]
    [Authorize(Roles = "Admin,Accountant")]
    public async Task<ActionResult<PayrollResult>> GenerateMonth(GenerateMonthRequest request)
        => Result(await service.GenerateMonthAsync(request, Actor));

    [HttpGet("kpi-policies")]
    [Authorize(Roles = "Admin,Accountant")]
    public async Task<ActionResult<List<KpiPolicyDto>>> GetPolicies() => Ok(await service.GetKpiPoliciesAsync());

    [HttpPut("kpi-policies")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<PayrollResult>> SavePolicy(SaveKpiPolicyRequest request)
        => Result(await service.SaveKpiPolicyAsync(request, Actor));

    [HttpGet("monthly-adjustments")]
    [Authorize(Roles = "Admin,Accountant")]
    public async Task<ActionResult<List<MonthlyAdjustmentDto>>> GetAdjustments([FromQuery, Range(2000, 2100)] int year,
        [FromQuery, Range(1, 12)] int month) => Ok(await service.GetMonthlyAdjustmentsAsync(year, month));

    [HttpPost("monthly-adjustments")]
    [Authorize(Roles = "Admin,Accountant")]
    public async Task<ActionResult<PayrollResult>> AddAdjustment(MonthlyAdjustmentRequest request)
        => Result(await service.AddMonthlyAdjustmentAsync(request, Actor));

    [HttpDelete("monthly-adjustments/{id:int}")]
    [Authorize(Roles = "Admin,Accountant")]
    public async Task<ActionResult<PayrollResult>> DeleteAdjustment(int id)
        => Result(await service.DeleteMonthlyAdjustmentAsync(id, Actor));

    [HttpGet("kpi-rates")]
    [Authorize(Roles = "Admin,Accountant")]
    public async Task<ActionResult<List<DriverKpiRateDto>>> GetKpiRates() => Ok(await service.GetKpiRatesAsync());

    [HttpPut("kpi-rates")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<PayrollResult>> SaveKpiRates(List<DriverKpiRateDto> request)
        => Result(await service.SaveKpiRatesAsync(request, Actor));

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
