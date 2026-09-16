using Microsoft.AspNetCore.Mvc;
using TruckingSystem.Application.PublicTracking;

namespace TruckingSystem.API.Controllers;

/// <summary>Endpoint công khai — KHÔNG [Authorize], an toàn nhờ token GUID không đoán được, chỉ cho xem.</summary>
[ApiController]
[Route("api/public")]
public class PublicController : ControllerBase
{
    private readonly IPublicTrackingService _service;

    public PublicController(IPublicTrackingService service)
    {
        _service = service;
    }

    [HttpGet("track/{token}")]
    public async Task<ActionResult<PublicTrackingResponse>> Track(string token)
    {
        var result = await _service.TrackAsync(token);
        return result is null ? NotFound() : Ok(result);
    }
}
