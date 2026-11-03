using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TruckingSystem.Application.Attendance;

namespace TruckingSystem.API.Controllers;

/// <summary>Chấm công QR + GPS. Nhóm "me/*" và "scan" dành cho ứng dụng điện thoại — nhân viên được
/// xác định từ JWT (NameIdentifier) ở service; các endpoint còn lại cho quản lý trên Desktop.</summary>
[ApiController]
[Route("api/attendance")]
[Authorize]
public class AttendanceController : ControllerBase
{
    private const string NotLinked = "Tài khoản chưa được liên kết với hồ sơ nhân viên đang làm việc. Liên hệ quản trị viên.";
    private readonly IAttendanceService _service;

    public AttendanceController(IAttendanceService service) => _service = service;

    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
    private string UserName => User.Identity?.Name ?? "unknown";

    // ---------------- Mobile ----------------

    [HttpGet("me")]
    public async Task<ActionResult<MobileTodayDto>> GetMe()
    {
        var result = await _service.GetMyTodayAsync(UserId);
        return result is null ? StatusCode(StatusCodes.Status403Forbidden, new AttendanceResult(false, NotLinked)) : Ok(result);
    }

    [HttpPost("scan")]
    public async Task<ActionResult<AttendanceScanResponse>> Scan(AttendanceScanRequest request)
    {
        var result = await _service.ScanAsync(UserId, request);
        if (result.Code == "NotEmployee") return StatusCode(StatusCodes.Status403Forbidden, result);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("me/history")]
    public async Task<ActionResult<List<AttendanceDayDto>>> GetMyHistory([FromQuery] DateTime from, [FromQuery] DateTime to)
    {
        var result = await _service.GetMyHistoryAsync(UserId, from, to);
        return result is null ? StatusCode(StatusCodes.Status403Forbidden, new AttendanceResult(false, NotLinked)) : Ok(result);
    }

    [HttpGet("me/adjustments")]
    public async Task<ActionResult<List<AdjustmentRequestDto>>> GetMyAdjustments()
    {
        var result = await _service.GetMyAdjustmentsAsync(UserId);
        return result is null ? StatusCode(StatusCodes.Status403Forbidden, new AttendanceResult(false, NotLinked)) : Ok(result);
    }

    [HttpPost("me/adjustments")]
    public async Task<ActionResult<AttendanceResult>> CreateAdjustment(CreateAdjustmentRequest request)
    {
        var result = await _service.CreateAdjustmentAsync(UserId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("adjustment-reasons")]
    public ActionResult<List<AdjustmentReasonDto>> GetAdjustmentReasons() => Ok(_service.GetAdjustmentReasons());

    // ---------------- Desktop ----------------

    [HttpGet("sites")]
    [Authorize(Roles = "Admin,Dispatcher")]
    public async Task<ActionResult<List<AttendanceSiteDto>>> GetSites() => Ok(await _service.GetSitesAsync());

    [HttpPut("sites/{warehouseId:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<AttendanceResult>> UpdateSite(int warehouseId, UpdateAttendanceSiteRequest request)
    {
        var result = await _service.UpdateSiteAsync(warehouseId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("sites/{warehouseId:int}/qr")]
    [Authorize(Roles = "Admin,Dispatcher")]
    public async Task<ActionResult<AttendanceQrDto>> GetQr(int warehouseId, [FromQuery] DateTime date)
    {
        var (result, qr) = await _service.GetOrCreateQrAsync(warehouseId, date, UserName);
        return result.Success ? Ok(qr) : BadRequest(result);
    }

    [HttpGet("records")]
    [Authorize(Roles = "Admin,Dispatcher")]
    public async Task<ActionResult<List<AttendanceRecordDto>>> GetRecords([FromQuery] DateTime from, [FromQuery] DateTime to)
        => Ok(await _service.GetRecordsAsync(from, to));

    [HttpGet("scans")]
    [Authorize(Roles = "Admin,Dispatcher")]
    public async Task<ActionResult<List<AttendanceScanLogDto>>> GetScans([FromQuery] DateTime date)
        => Ok(await _service.GetScansAsync(date));

    [HttpGet("adjustments")]
    [Authorize(Roles = "Admin,Dispatcher")]
    public async Task<ActionResult<List<AdjustmentRequestDto>>> GetAdjustments([FromQuery] string? status)
        => Ok(await _service.GetAdjustmentsAsync(status));

    [HttpPost("adjustments/{id:int}/review")]
    [Authorize(Roles = "Admin,Dispatcher")]
    public async Task<ActionResult<AttendanceResult>> Review(int id, ReviewAdjustmentRequest request)
    {
        var result = await _service.ReviewAdjustmentAsync(id, request, UserName);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
