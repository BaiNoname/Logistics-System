using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TruckingSystem.Application.AuditLogs;

namespace TruckingSystem.API.Controllers;

[ApiController]
[Route("api/audit-logs")]
[Authorize]
public class AuditLogsController : ControllerBase
{
    private readonly IAuditLogService _service;

    public AuditLogsController(IAuditLogService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<List<AuditLogDto>>> Get(
        [FromQuery] string? action, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        => Ok(await _service.GetAsync(new AuditLogFilterRequest(action, fromDate, toDate)));

    [HttpGet("actions")]
    public async Task<ActionResult<List<string>>> GetDistinctActions()
        => Ok(await _service.GetDistinctActionsAsync());
}
