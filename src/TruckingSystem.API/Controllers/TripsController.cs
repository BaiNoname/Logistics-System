using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TruckingSystem.Application.Trips;

namespace TruckingSystem.API.Controllers;

[ApiController]
[Route("api/trips")]
[Authorize]
public class TripsController : ControllerBase
{
    private readonly ITripService _tripService;

    public TripsController(ITripService tripService)
    {
        _tripService = tripService;
    }

    [HttpPost("{id:int}/ping")]
    public async Task<ActionResult<ServiceResult>> Ping(int id, GpsPingRequest request)
    {
        var result = await _tripService.PingAsync(id, request);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost("{id:int}/confirm-arrival")]
    public async Task<ActionResult<ServiceResult>> ConfirmArrival(int id, ConfirmArrivalRequest request)
    {
        var result = await _tripService.ConfirmArrivalAsync(id, request);
        return result.Success ? Ok(result) : NotFound(result);
    }
}
