using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TruckingSystem.Application.Warehouses;

namespace TruckingSystem.API.Controllers;

[ApiController]
[Route("api/warehouses")]
[Authorize]
public class WarehousesController : ControllerBase
{
    private readonly IWarehouseSlottingService _slottingService;

    public WarehousesController(IWarehouseSlottingService slottingService)
    {
        _slottingService = slottingService;
    }

    // Endpoint chính thể hiện thuật toán Best-Fit — gọi khi hàng chuẩn bị nhập kho,
    // trả về vị trí (StorageBin) được đề xuất để nhân viên kho xếp hàng vào.
    [HttpPost("{warehouseId:int}/suggest-bin")]
    public async Task<ActionResult<SuggestBinResponse>> SuggestBin(int warehouseId, SuggestBinRequest request)
    {
        var result = await _slottingService.SuggestBinAsync(warehouseId, request);
        return result.Found ? Ok(result) : NotFound(result);
    }

    [HttpGet]
    public async Task<ActionResult<List<WarehouseListItemDto>>> GetWarehouses()
        => Ok(await _slottingService.GetWarehousesAsync());

    [HttpGet("{warehouseId:int}/zones")]
    public async Task<ActionResult<List<WarehouseZoneDto>>> GetZones(int warehouseId)
        => Ok(await _slottingService.GetZonesAsync(warehouseId));

    [HttpGet("zones/{zoneId:int}/bins")]
    public async Task<ActionResult<List<StorageBinDto>>> GetBins(int zoneId)
        => Ok(await _slottingService.GetBinsAsync(zoneId));
}
