using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TruckingSystem.Application.Employees;

namespace TruckingSystem.API.Controllers;

[ApiController]
[Route("api/employees")]
[Authorize(Roles = "Admin,Dispatcher")]
public class EmployeesController : ControllerBase
{
    private readonly IEmployeeService _service;

    public EmployeesController(IEmployeeService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<List<EmployeeListItemDto>>> GetAll()
        => Ok(await _service.GetAllAsync());

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<EmployeeActionResult>> Create(CreateEmployeeRequest request)
    {
        var result = await _service.CreateAsync(request, User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<EmployeeActionResult>> Update(int id, UpdateEmployeeRequest request)
    {
        var result = await _service.UpdateAsync(id, request, User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("departments")]
    public async Task<ActionResult<List<LookupItemDto>>> GetDepartments()
        => Ok(await _service.GetDepartmentsAsync());

    [HttpGet("positions")]
    public async Task<ActionResult<List<LookupItemDto>>> GetPositions()
        => Ok(await _service.GetPositionsAsync());

    [HttpPost("{employeeId:int}/driver-profile")]
    public async Task<ActionResult<EmployeeActionResult>> CreateDriverProfile(int employeeId, CreateDriverProfileRequest request)
    {
        var result = await _service.CreateDriverProfileAsync(employeeId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{employeeId:int}/driver-profile")]
    public async Task<ActionResult<DriverProfileDto?>> GetDriverProfile(int employeeId)
        => Ok(await _service.GetDriverProfileAsync(employeeId));

    [HttpPut("{employeeId:int}/driver-profile")]
    public async Task<ActionResult<EmployeeActionResult>> UpdateDriverProfile(int employeeId, UpdateDriverProfileRequest request)
    {
        var result = await _service.UpdateDriverProfileAsync(employeeId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Danh sách ca làm việc đã đăng ký của 1 tài xế.</summary>
    [HttpGet("{employeeId:int}/schedules")]
    public async Task<ActionResult<List<DriverScheduleDto>>> GetSchedules(int employeeId)
        => Ok(await _service.GetSchedulesAsync(employeeId));

    [HttpPost("{employeeId:int}/schedules")]
    public async Task<ActionResult<EmployeeActionResult>> CreateSchedule(int employeeId, CreateDriverScheduleRequest request)
    {
        var result = await _service.CreateScheduleAsync(employeeId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("schedules/{scheduleId:int}")]
    public async Task<ActionResult<EmployeeActionResult>> DeleteSchedule(int scheduleId)
    {
        var result = await _service.DeleteScheduleAsync(scheduleId);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
