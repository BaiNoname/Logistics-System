using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TruckingSystem.Application.Employees;

namespace TruckingSystem.API.Controllers;

[ApiController]
[Route("api/employees")]
[Authorize]
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
    public async Task<ActionResult<EmployeeListItemDto>> Create(CreateEmployeeRequest request)
        => Ok(await _service.CreateAsync(request));

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
}