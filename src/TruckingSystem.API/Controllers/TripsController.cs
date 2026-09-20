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

    // ---- Desktop (Dispatcher) ----
    [HttpGet]
    public async Task<ActionResult<List<TripListItemDto>>> GetAll()
        => Ok(await _tripService.GetAllAsync());

    [HttpPost("{id:int}/start")]
    public async Task<ActionResult<TripActionResult>> Start(int id)
    {
        var result = await _tripService.StartAsync(id);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id:int}/complete")]
    public async Task<ActionResult<TripActionResult>> Complete(int id)
    {
        var result = await _tripService.CompleteAsync(id);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id:int}/fuel")]
    public async Task<ActionResult<TripActionResult>> RecordFuel(int id, RecordFuelRequest request)
    {
        var result = await _tripService.RecordFuelAsync(id, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // ---- App tài xế (đã có từ trước) ----
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