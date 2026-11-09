using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TruckingSystem.Application.Leave;

namespace TruckingSystem.API.Controllers;

[ApiController]
[Route("api/holidays")]
[Authorize]
public class HolidaysController(ILeaveService service) : ControllerBase
{
    private string Actor => User.FindFirstValue(ClaimTypes.NameIdentifier)!;
    private ActionResult<LeaveResult> Result(LeaveResult result) => result.Success ? Ok(result) : BadRequest(result);

    [HttpGet]
    [Authorize(Roles = "Admin,Accountant")]
    public async Task<ActionResult<List<HolidayDto>>> Get([FromQuery, Range(2000, 2100)] int year)
        => Ok(await service.GetHolidaysAsync(year));

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<LeaveResult>> Add(AddHolidayRequest request)
        => Result(await service.AddHolidayAsync(request, Actor));

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<LeaveResult>> Delete(int id)
        => Result(await service.DeleteHolidayAsync(id, Actor));
}
