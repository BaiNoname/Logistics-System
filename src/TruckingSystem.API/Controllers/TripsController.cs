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

    /// <summary>Điểm GPS thực tế đã ghi nhận — dùng để vẽ đường đi thật trên bản đồ.</summary>
    [HttpGet("{id:int}/waypoints")]
    public async Task<ActionResult<List<WaypointDto>>> GetWaypoints(int id)
        => Ok(await _tripService.GetWaypointsAsync(id));

    /// <summary>Tọa độ đầu-cuối đúng theo chặng — [AllowAnonymous] vì trang
    /// navigate.html (app tài xế mở qua WebView) gọi mà không đăng nhập.</summary>
    [HttpGet("{id:int}/route")]
    [AllowAnonymous]
    public async Task<ActionResult<TripRouteDto>> GetTripRoute(int id)
    {
        var route = await _tripService.GetTripRouteAsync(id);
        return route is null ? NotFound() : Ok(route);
    }

    /// <summary>So sánh chi phí xăng ước tính (từ GPS thật) với chi phí thực tế đã ghi.</summary>
    [HttpGet("{id:int}/fuel-estimate")]
    public async Task<ActionResult<FuelEstimateDto>> GetFuelEstimate(int id)
        => Ok(await _tripService.GetFuelEstimateAsync(id));

    /// <summary>Danh sách sự cố của 1 chuyến.</summary>
    [HttpGet("{id:int}/incidents")]
    public async Task<ActionResult<List<IncidentReportDto>>> GetIncidents(int id)
        => Ok(await _tripService.GetIncidentsAsync(id));

    [HttpPost("{id:int}/incidents")]
    public async Task<ActionResult<IncidentReportDto>> CreateIncident(int id, CreateIncidentReportRequest request)
        => Ok(await _tripService.CreateIncidentAsync(id, request));

    [HttpDelete("incidents/{incidentId:int}")]
    public async Task<ActionResult<TripActionResult>> DeleteIncident(int incidentId)
    {
        var result = await _tripService.DeleteIncidentAsync(incidentId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // ---- App tài xế (đã có từ trước) — [AllowAnonymous] vì app KHÔNG có
    // luồng đăng nhập (chỉ nhập Server URL + Trip ID) — khác các endpoint
    // phía trên dành cho Desktop/Dispatcher vẫn yêu cầu JWT như bình thường.
    // ⚠️ Đánh đổi bảo mật: ai biết đúng TripId đều gọi được — chấp nhận được
    // với quy mô đồ án, nhưng cần lưu ý nếu triển khai thật.
    [HttpPost("{id:int}/ping")]
    [AllowAnonymous]
    public async Task<ActionResult<ServiceResult>> Ping(int id, GpsPingRequest request)
    {
        var result = await _tripService.PingAsync(id, request);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost("{id:int}/confirm-arrival")]
    [AllowAnonymous]
    public async Task<ActionResult<ServiceResult>> ConfirmArrival(int id, ConfirmArrivalRequest request)
    {
        var result = await _tripService.ConfirmArrivalAsync(id, request);
        return result.Success ? Ok(result) : NotFound(result);
    }
}