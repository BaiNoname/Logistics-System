using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TruckingSystem.Application.AuditLogs;

namespace TruckingSystem.API.Controllers;

[ApiController]
[Route("api/audit-logs")]
[Authorize(Roles = "Admin")] // nhật ký lộ hoạt động của MỌI người -> chỉ Quản trị viên xem
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
    public async Task<ActionResult<List<AuditLogDto>>> Get(
        [FromQuery] string? action, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate,
        [FromQuery] string? userName, [FromQuery] string? entityName, [FromQuery] string? search,
        [FromQuery] int skip = 0, [FromQuery] int take = 300)
        => Ok(await _service.GetAsync(new AuditLogFilterRequest(action, fromDate, toDate, userName, entityName, search, skip, take)));

    [HttpGet("actions")]
    public async Task<ActionResult<List<string>>> GetDistinctActions()
        => Ok(await _service.GetDistinctActionsAsync());

    [HttpGet("filter-options")]
    public async Task<ActionResult<AuditLogFilterOptionsDto>> GetFilterOptions()
        => Ok(await _service.GetFilterOptionsAsync());
}
