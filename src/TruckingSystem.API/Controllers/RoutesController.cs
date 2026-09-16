using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TruckingSystem.Application.Routing;

namespace TruckingSystem.API.Controllers;

[ApiController]
[Route("api/routes")]
[Authorize]
public class RoutesController : ControllerBase
{
    private readonly IRouteProvider _routeProvider;

    public RoutesController(IRouteProvider routeProvider)
    {
        _routeProvider = routeProvider;
    }

    /// <summary>Tính khoảng cách, thời gian, và tuyến đường thật (qua VietMap) giữa 2 tọa độ.</summary>
    [HttpGet("calculate")]
    public async Task<ActionResult<RouteResult>> Calculate(double fromLat, double fromLng, double toLat, double toLng)
    {
        var result = await _routeProvider.GetRouteAsync(new RouteRequest(fromLat, fromLng, toLat, toLng));
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
