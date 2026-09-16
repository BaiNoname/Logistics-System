using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TruckingSystem.Application.Locations;

namespace TruckingSystem.API.Controllers;

[ApiController]
[Route("api/locations")]
[Authorize]
public class LocationsController : ControllerBase
{
    private readonly ILocationService _service;

    public LocationsController(ILocationService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<List<LocationDto>>> GetAll()
        => Ok(await _service.GetAllAsync());

    [HttpPost]
    public async Task<ActionResult<LocationDto>> Create(CreateLocationRequest request)
        => Ok(await _service.CreateAsync(request));
}
