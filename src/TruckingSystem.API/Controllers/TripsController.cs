using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TruckingSystem.Application.Trips;

namespace TruckingSystem.API.Controllers;

[ApiController]
[Route("api/trips")]
// Chỉ đúng các vai trò được vào màn này trên Desktop (trước đây [Authorize] trơn: tài khoản Tài xế /
// Kế toán gọi thẳng API vẫn bắt đầu / duyệt / đổi xe chuyến, sửa xóa kho...).
[Authorize(Roles = "Admin,Dispatcher")]
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
        if (result.Success) await _tripService.ResyncCalendarIfSyncedAsync(id, PublicBaseUrl); // lịch theo giờ chạy THẬT
        return Ok(result); // luôn 200 OK — Desktop tự đọc result.Success/Message, tránh PostAsync throw exception khi 400 làm mất thông báo rõ ràng
    }

    [HttpPost("{id:int}/complete")]
    public async Task<ActionResult<TripActionResult>> Complete(int id, [FromBody] FinishTripRequest? request)
    {
        var result = await _tripService.CompleteAsync(id, request,
            User.FindFirstValue(ClaimTypes.NameIdentifier), User.FindFirstValue(ClaimTypes.Name));
        if (result.Success) await _tripService.ResyncCalendarIfSyncedAsync(id, PublicBaseUrl);
        return Ok(result); // luôn 200 OK — Desktop tự đọc result.Success/Message, tránh PostAsync throw exception khi 400 làm mất thông báo rõ ràng
    }

    /// <summary>Thông tin cho hộp duyệt / hoàn tất: chuyến giao cuối? hàng nằm kho quá hạn chưa chọn do ai?</summary>
    [HttpGet("{id:int}/finish-check")]
    public async Task<ActionResult<FinishCheckDto>> FinishCheck(int id)
    {
        var result = await _tripService.GetFinishCheckAsync(id);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Vận hành DUYỆT xác nhận "đã xong" của tài xế — PendingConfirmation -> Completed thật sự.</summary>
    [HttpPost("{id:int}/approve-completion")]
    public async Task<ActionResult<TripActionResult>> ApproveCompletion(int id, [FromBody] FinishTripRequest? request)
    {
        var result = await _tripService.ApproveCompletionAsync(id, request,
            User.FindFirstValue(ClaimTypes.NameIdentifier), User.FindFirstValue(ClaimTypes.Name));
        if (result.Success) await _tripService.ResyncCalendarIfSyncedAsync(id, PublicBaseUrl);
        return Ok(result);
    }

    /// <summary>Xe / tài xế có thể thay cho chuyến CHƯA CHẠY (kèm lý do không dùng được).</summary>
    [HttpGet("{id:int}/reassign-options")]
    public async Task<ActionResult<ReassignOptionsDto>> GetReassignOptions(int id)
        => Ok(await _tripService.GetReassignOptionsAsync(id));

    /// <summary>Đổi xe và/hoặc tài xế cho chuyến chưa chạy (gỡ chuyến bị kẹt).</summary>
    [HttpPost("{id:int}/reassign")]
    public async Task<ActionResult<TripActionResult>> Reassign(int id, ReassignTripRequest request)
        => Ok(await _tripService.ReassignAsync(id, request, PublicBaseUrl)); // lịch chuyển sang tài xế mới

    /// <summary>Chuyến Trả hàng chưa chạy -> khách nhận lại hàng tại kho (xuất kho, đóng chuyến, trả xe).</summary>
    [HttpPost("{id:int}/return-at-warehouse")]
    public async Task<ActionResult<TripActionResult>> ReturnAtWarehouse(int id)
    {
        var result = await _tripService.ReturnAtWarehouseAsync(id, User.FindFirstValue(ClaimTypes.Name));
        if (result.Success) await _tripService.ResyncCalendarIfSyncedAsync(id, PublicBaseUrl); // lịch -> Hoàn tất (vàng)
        return Ok(result);
    }

    /// <summary>Vận hành TỪ CHỐI xác nhận của tài xế — đẩy chuyến quay lại Đang chạy.</summary>
    [HttpPost("{id:int}/reject-completion")]
    public async Task<ActionResult<TripActionResult>> RejectCompletion(int id)
    {
        var result = await _tripService.RejectCompletionAsync(id);
        if (result.Success) await _tripService.ResyncCalendarIfSyncedAsync(id, PublicBaseUrl); // quay lại Đang chạy
        return Ok(result);
    }

    [HttpPost("{id:int}/fuel")]
    public async Task<ActionResult<TripActionResult>> RecordFuel(int id, RecordFuelRequest request)
    {
        var result = await _tripService.RecordFuelAsync(id, request);
        return Ok(result); // luôn 200 OK — Desktop tự đọc result.Success/Message, tránh PostAsync throw exception khi 400 làm mất thông báo rõ ràng
    }

    /// <summary>Lịch sử các lần đổ xăng của chuyến.</summary>
    [HttpGet("{id:int}/fuel-logs")]
    public async Task<ActionResult<List<FuelLogDto>>> GetFuelLogs(int id)
        => Ok(await _tripService.GetFuelLogsAsync(id));

    /// <summary>Đơn giá tự động cho form ghi đổ xăng (providerId bỏ trống = giá thị trường chung).</summary>
    [HttpGet("{id:int}/fuel-price")]
    public async Task<ActionResult<FuelPriceQuoteDto>> GetFuelPrice(int id, [FromQuery] int? providerId)
        => Ok(await _tripService.GetFuelPriceQuoteAsync(id, providerId));

    /// <summary>Xóa 1 lần đổ xăng ghi nhầm.</summary>
    [HttpDelete("fuel-logs/{fuelLogId:int}")]
    public async Task<ActionResult<TripActionResult>> DeleteFuelLog(int fuelLogId)
        => Ok(await _tripService.DeleteFuelLogAsync(fuelLogId));

    /// <summary>Tiến độ nhập/xuất kho của từng chuyến (cột "Kho" ở màn Chuyến).</summary>
    [HttpGet("warehouse-progress")]
    public async Task<ActionResult<List<TripWarehouseProgressDto>>> GetWarehouseProgress()
        => Ok(await _tripService.GetWarehouseProgressAsync());

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

    /// <summary>Tính khoảng cách/thời gian ước tính từ VietMap cho chuyến này
    /// và LƯU LẠI vào Trip (phục vụ thống kê so sánh Ước tính vs Thực tế sau này).</summary>
    [HttpPost("{id:int}/calculate-estimate")]
    public async Task<ActionResult<TripRouteEstimateDto>> CalculateEstimate(int id)
        => Ok(await _tripService.CalculateAndSaveRouteEstimateAsync(id));

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
    {
        try
        {
            return Ok(await _tripService.CreateIncidentAsync(id, request));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("incidents/{incidentId:int}")]
    public async Task<ActionResult<TripActionResult>> DeleteIncident(int incidentId)
    {
        var result = await _tripService.DeleteIncidentAsync(incidentId);
        return Ok(result); // luôn 200 OK — Desktop tự đọc result.Success/Message, tránh PostAsync throw exception khi 400 làm mất thông báo rõ ràng
    }

    /// <summary>Đồng bộ lịch chuyến lên Google Calendar của tài xế + khách hàng.</summary>
    private string PublicBaseUrl => PublicUrl.For(Request);

    [HttpPost("{id:int}/sync-calendar")]
    public async Task<ActionResult<CalendarSyncResultDto>> SyncCalendar(int id)
    {
        var baseUrl = PublicUrl.For(Request);
        return Ok(await _tripService.SyncCalendarAsync(id, baseUrl));
    }

    /// <summary>Danh sách chi phí khác (ngoài xăng) đã ghi cho 1 chuyến.</summary>
    [HttpGet("{id:int}/expenses")]
    public async Task<ActionResult<List<TripExpenseDto>>> GetExpenses(int id)
        => Ok(await _tripService.GetExpensesAsync(id));

    [HttpPost("{id:int}/expenses")]
    public async Task<ActionResult<TripExpenseDto>> CreateExpense(int id, CreateTripExpenseRequest request)
    {
        try
        {
            return Ok(await _tripService.CreateExpenseAsync(id, request));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("expenses/{expenseId:int}")]
    public async Task<ActionResult<TripActionResult>> DeleteExpense(int expenseId)
        => Ok(await _tripService.DeleteExpenseAsync(expenseId));

    // ---- App tài xế (đã có từ trước) — [AllowAnonymous] vì app KHÔNG có
    // luồng đăng nhập (chỉ nhập Server URL + Trip ID) — khác các endpoint
    // phía trên dành cho Desktop/Dispatcher vẫn yêu cầu JWT như bình thường.
    // ⚠️ Đánh đổi bảo mật: ai biết đúng TripId đều gọi được — chấp nhận được
    // với quy mô đồ án, nhưng cần lưu ý nếu triển khai thật.
    [HttpPost("{id:int}/ping")]
    [AllowAnonymous]
    public async Task<ActionResult<ServiceResult>> Ping(int id, GpsPingRequest request)
    {
        var result = await _tripService.PingAsync(id, request, PublicBaseUrl); // GPS đầu tiên bắt đầu chuyến -> lịch "Đang chạy"
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>App tài xế kiểm tra trước khi bật theo dõi GPS.</summary>
    [HttpGet("{id:int}/driver-status")]
    [AllowAnonymous]
    public async Task<ActionResult<DriverTripStatusDto>> GetDriverStatus(int id)
    {
        var status = await _tripService.GetDriverStatusAsync(id);
        return status is null ? NotFound(new { message = "Không tìm thấy chuyến." }) : Ok(status);
    }

    [HttpPost("{id:int}/confirm-arrival")]
    [AllowAnonymous]
    public async Task<ActionResult<ServiceResult>> ConfirmArrival(int id, ConfirmArrivalRequest request)
    {
        var result = await _tripService.ConfirmArrivalAsync(id, request);
        // App tự bắt đầu chuyến qua GPS (không qua nút Bắt đầu) -> lúc báo đến nơi cập nhật lịch
        // theo giờ chạy THẬT (bắt đầu + đến nơi) — 1 lần, không cập nhật ở mỗi lần gửi GPS.
        if (result.Success) await _tripService.ResyncCalendarIfSyncedAsync(id, PublicBaseUrl);
        return result.Success ? Ok(result) : NotFound(result);
    }
}