using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TruckingSystem.Application.Vehicles;

namespace TruckingSystem.API.Controllers;

[ApiController]
[Route("api/vehicles")]
[Authorize]
public class VehiclesController : ControllerBase
{
    private readonly IVehicleService _service;

    public VehiclesController(IVehicleService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<List<VehicleListItemDto>>> GetAll()
        => Ok(await _service.GetAllAsync());

    [HttpPost]
    public async Task<ActionResult<VehicleListItemDto>> Create(CreateVehicleRequest request)
        => Ok(await _service.CreateAsync(request));
}
