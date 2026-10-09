using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TruckingSystem.Application.Warehouses;

namespace TruckingSystem.API.Controllers;

/// <summary>Hàng CHỜ NHẬP KHO — chuyến Lấy hàng đã về kho nhưng hàng chưa lên kệ.
///   GET  /api/warehouses/{warehouseId}/pending-receipts                — danh sách dòng hàng còn chờ nhập
///   POST /api/warehouses/{warehouseId}/pending-receipts/{tripId}/auto   — nhập hết hàng của chuyến theo Best-Fit
///   POST /api/warehouses/{warehouseId}/pending-receipts/{tripId}/manual — nhập 1 phần vào vị trí tự chọn
/// Luôn trả 200 kèm Success/Message (giống nhập tay) để Desktop hiện đúng lý do.</summary>
[ApiController]
[Route("api/warehouses/{warehouseId:int}/pending-receipts")]
[Authorize]
public class PendingReceiptsController : ControllerBase
{
    private readonly IPendingReceiptService _pendingService;
    private readonly IWarehouseSlottingService _slottingService;

    public PendingReceiptsController(IPendingReceiptService pendingService, IWarehouseSlottingService slottingService)
    {
        _pendingService = pendingService;
        _slottingService = slottingService;
    }

    [HttpGet]
    public async Task<ActionResult<List<PendingReceiptLineDto>>> GetPending(int warehouseId)
        => Ok(await _pendingService.GetPendingAsync(warehouseId));

    [HttpPost("{tripId:int}/auto")]
    public async Task<ActionResult<ReceiveGoodsResult>> ReceiveAuto(int warehouseId, int tripId)
        => Ok(await _pendingService.ReceiveAutoAsync(warehouseId, tripId, await CurrentPerformerAsync()));

    [HttpPost("{tripId:int}/manual")]
    public async Task<ActionResult<ReceiveGoodsResult>> ReceiveManual(int warehouseId, int tripId, PendingManualReceiveRequest request)
        => Ok(await _pendingService.ReceiveManualAsync(warehouseId, tripId, request, await CurrentPerformerAsync()));

    private async Task<StockPerformer> CurrentPerformerAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userName = User.FindFirstValue(ClaimTypes.Name);
        return new StockPerformer(await _slottingService.ResolveEmployeeIdAsync(userId), userName);
    }
}
