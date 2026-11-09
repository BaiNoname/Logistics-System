using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TruckingSystem.Application.Warehouses;

namespace TruckingSystem.API.Controllers;

/// <summary>XUẤT KHO THEO CHUYẾN — chặng Giao đã chia xe, kho soạn hàng + bốc lên xe rồi xác nhận xuất.
///   GET  /api/warehouses/{warehouseId}/trip-issues                — chuyến giao chưa chạy + hàng cần xuất
///   POST /api/warehouses/{warehouseId}/trip-issues/{tripId}/issue — xác nhận xuất ĐỦ hàng của chuyến
///   POST /api/warehouses/{warehouseId}/trip-issues/{tripId}/undo  — hoàn tác (dỡ hàng về kệ, xe chưa chạy)
/// Luôn trả 200 kèm Success/Message để Desktop hiện đúng lý do.</summary>
[ApiController]
[Route("api/warehouses/{warehouseId:int}/trip-issues")]
[Authorize]
public class TripIssuesController : ControllerBase
{
    private readonly ITripIssueService _service;
    private readonly IWarehouseSlottingService _slottingService;

    public TripIssuesController(ITripIssueService service, IWarehouseSlottingService slottingService)
    {
        _service = service;
        _slottingService = slottingService;
    }

    [HttpGet]
    public async Task<ActionResult<List<TripIssueLineDto>>> Get(int warehouseId)
        => Ok(await _service.GetAsync(warehouseId));

    [HttpPost("{tripId:int}/issue")]
    public async Task<ActionResult<WarehouseActionResult>> Issue(int warehouseId, int tripId)
        => Ok(await _service.IssueTripAsync(warehouseId, tripId, await CurrentPerformerAsync()));

    [HttpPost("{tripId:int}/undo")]
    public async Task<ActionResult<WarehouseActionResult>> Undo(int warehouseId, int tripId)
        => Ok(await _service.UndoTripAsync(warehouseId, tripId, await CurrentPerformerAsync()));

    private async Task<StockPerformer> CurrentPerformerAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userName = User.FindFirstValue(ClaimTypes.Name);
        return new StockPerformer(await _slottingService.ResolveEmployeeIdAsync(userId), userName);
    }
}
