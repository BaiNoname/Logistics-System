using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TruckingSystem.Application.AuditLogs;

namespace TruckingSystem.API.Controllers;

[ApiController]
[Route("api/audit-logs")]
// LƯU Ý: [Authorize] ở lớp và ở hàm được AND với nhau -> quyền đặt RIÊNG từng hàm:
//  • Toàn bộ nhật ký: chỉ Quản trị viên (lộ hoạt động của MỌI người).
//  • Lịch sử 1 đơn / chuyến / xe: Quản trị + Điều phối.
[Authorize]
public class AuditLogsController : ControllerBase
{
    private readonly IAuditLogService _service;

    public AuditLogsController(IAuditLogService service)
    {
        _service = service;
    }

    /// <summary>Nhật ký mới nhất trước; lọc theo hành động / người / loại đối tượng / chữ tìm / khoảng ngày (giờ VN),
    /// phân trang bằng skip + take (tối đa 1000).</summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<List<AuditLogDto>>> Get(
        [FromQuery] string? action, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate,
        [FromQuery] string? userName, [FromQuery] string? entityName, [FromQuery] string? search,
        [FromQuery] int skip = 0, [FromQuery] int take = 300)
        => Ok(await _service.GetAsync(new AuditLogFilterRequest(action, fromDate, toDate, userName, entityName, search, skip, take)));

    [HttpGet("actions")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<List<string>>> GetDistinctActions()
        => Ok(await _service.GetDistinctActionsAsync());

    /// <summary>Lịch sử thay đổi của 1 đơn / chuyến / xe — Điều phối cũng xem được (chỉ đối tượng mình đang xử lý,
    /// không phải toàn bộ nhật ký). entityName: TransportOrder | Trip | Vehicle.</summary>
    [HttpGet("history")]
    [Authorize(Roles = "Admin,Dispatcher")]
    public async Task<ActionResult<List<AuditLogDto>>> GetHistory([FromQuery] string entityName, [FromQuery] int entityId)
    {
        var rows = await _service.GetHistoryAsync(entityName, entityId);
        return rows is null ? BadRequest(new { message = "Loại đối tượng không hỗ trợ xem lịch sử." }) : Ok(rows);
    }

    [HttpGet("filter-options")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<AuditLogFilterOptionsDto>> GetFilterOptions()
        => Ok(await _service.GetFilterOptionsAsync());
}
