using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TruckingSystem.Application.Resources;

namespace TruckingSystem.API.Controllers;

/// <summary>Màn "🧭 Nguồn lực theo kho" — xe + tài xế của 1 kho kèm tình trạng giấy tờ và
/// "chạy được hôm nay?" (chỉ đọc).</summary>
[ApiController]
[Route("api/resources")]
[Authorize(Roles = "Admin,Dispatcher")]
public class ResourcesController : ControllerBase
{
    private readonly IResourceOverviewService _service;

    public ResourcesController(IResourceOverviewService service)
    {
        _service = service;
    }

    /// <summary>warehouseId bỏ trống = mọi kho.</summary>
    [HttpGet("overview")]
    public async Task<ActionResult<ResourceOverviewDto>> GetOverview([FromQuery] int? warehouseId)
        => Ok(await _service.GetAsync(warehouseId));
}
