using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TruckingSystem.Application.Drivers;

namespace TruckingSystem.API.Controllers;

[ApiController]
[Route("api/drivers")]
[Authorize]
public class DriversController : ControllerBase
{
    private readonly IDriverService _driverService;

    public DriversController(IDriverService driverService)
    {
        _driverService = driverService;
    }

    /// <summary>Danh sách tài xế kèm trạng thái Availability (Đang làm / Sẵn
    /// sàng / Không sẵn sàng) — khớp đúng điều kiện thật dùng khi Chia xe.</summary>
    [HttpGet]
    public async Task<ActionResult<List<DriverListItemDto>>> GetAll()
    {
        var drivers = await _driverService.GetAllAsync();
        return Ok(drivers);
    }
}