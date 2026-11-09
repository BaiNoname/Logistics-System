using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TruckingSystem.Application.Leave;

namespace TruckingSystem.API.Controllers;

[ApiController]
[Route("api/leaves")]
[Authorize]
public class LeavesController(ILeaveService service) : ControllerBase
{
    private string Actor => User.FindFirstValue(ClaimTypes.NameIdentifier)!;
    private ActionResult<LeaveResult> Result(LeaveResult result) => result.Success ? Ok(result) : BadRequest(result);

    [HttpGet("types")]
    [Authorize(Roles = "Admin,Accountant")]
    public async Task<ActionResult<List<LeaveTypeDto>>> GetTypes() => Ok(await service.GetLeaveTypesAsync());

    [HttpGet]
    [Authorize(Roles = "Admin,Accountant")]
    public async Task<ActionResult<List<LeaveDto>>> Get([FromQuery] int employeeId,
        [FromQuery, Range(2000, 2100)] int year, [FromQuery, Range(1, 12)] int month)
        => Ok(await service.GetLeavesAsync(employeeId, year, month));

    [HttpGet("quota")]
    [Authorize(Roles = "Admin,Accountant")]
    public async Task<ActionResult<LeaveQuotaDto>> GetQuota([FromQuery] int employeeId,
        [FromQuery, Range(2000, 2100)] int year)
        => Ok(await service.GetQuotaAsync(employeeId, year));

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<LeaveResult>> Add(AddLeaveRequest request)
        => Result(await service.AddLeaveAsync(request, Actor));

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<LeaveResult>> Delete(int id)
        => Result(await service.DeleteLeaveAsync(id, Actor));

    [HttpDelete("batch/{batchId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<LeaveResult>> DeleteBatch(Guid batchId)
        => Result(await service.DeleteLeaveBatchAsync(batchId, Actor));
}
