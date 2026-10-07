using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TruckingSystem.Application.SystemManagement;

namespace TruckingSystem.API.Controllers;

[ApiController]
[Route("api/system")]
[Authorize(Roles = "Admin")]
public class SystemController : ControllerBase
{
    private readonly ISystemManagementService _service;

    public SystemController(ISystemManagementService service) => _service = service;

    [HttpGet("departments")]
    public async Task<ActionResult<List<SystemItemDto>>> GetDepartments()
        => Ok(await _service.GetDepartmentsAsync());

    [HttpGet("positions")]
    public async Task<ActionResult<List<SystemItemDto>>> GetPositions()
        => Ok(await _service.GetPositionsAsync());

    [HttpPost("departments")]
    public async Task<IActionResult> CreateDepartment(NameRequest request)
        => Respond(await _service.CreateDepartmentAsync(request));

    [HttpPost("positions")]
    public async Task<IActionResult> CreatePosition(NameRequest request)
        => Respond(await _service.CreatePositionAsync(request));

    [HttpPut("departments/{id:int}")]
    public async Task<IActionResult> UpdateDepartment(int id, NameRequest request)
        => Respond(await _service.UpdateDepartmentAsync(id, request));

    [HttpPut("positions/{id:int}")]
    public async Task<IActionResult> UpdatePosition(int id, NameRequest request)
        => Respond(await _service.UpdatePositionAsync(id, request));

    [HttpDelete("departments/{id:int}")]
    public async Task<IActionResult> DeleteDepartment(int id)
        => Respond(await _service.DeleteDepartmentAsync(id));

    [HttpDelete("positions/{id:int}")]
    public async Task<IActionResult> DeletePosition(int id)
        => Respond(await _service.DeletePositionAsync(id));

    private IActionResult Respond(SystemActionResult result) => result.Error switch
    {
        SystemActionError.InvalidName => BadRequest(result.Message),
        SystemActionError.Duplicate or SystemActionError.InUse => Conflict(result.Message),
        SystemActionError.NotFound => NotFound(),
        _ => Ok(result.Item ?? (object)new { success = true }),
    };
}
