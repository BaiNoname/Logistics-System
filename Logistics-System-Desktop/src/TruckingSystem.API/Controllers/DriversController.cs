using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TruckingSystem.Application.Drivers;

namespace TruckingSystem.API.Controllers;

[ApiController]
[Route("api/drivers")]
[Authorize(Roles = "Admin,Dispatcher")]
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

    /// <summary>Các ngày làm việc đã đăng ký của 1 tài xế (bảng DriverSchedules).</summary>
    [HttpGet("{employeeId:int}/workdays")]
    public async Task<ActionResult<List<DriverWorkDayDto>>> GetWorkDays(int employeeId)
        => Ok(await _driverService.GetWorkDaysAsync(employeeId));

    /// <summary>Đăng ký ngày làm việc theo khoảng ngày (không chọn ca).</summary>
    [HttpPost("{employeeId:int}/workdays")]
    public async Task<ActionResult<DriverWorkDayResult>> RegisterWorkDays(int employeeId, [FromBody] RegisterWorkDaysRequest request)
        => Ok(await _driverService.RegisterWorkDaysAsync(employeeId, request));

    [HttpDelete("workdays/{id:int}")]
    public async Task<ActionResult<DriverWorkDayResult>> DeleteWorkDay(int id)
        => Ok(await _driverService.DeleteWorkDayAsync(id));
}
