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

    [HttpPost]
    public async Task<ActionResult<WarehouseListItemDto>> CreateWarehouse(CreateWarehouseRequest request)
        => Ok(await _slottingService.CreateWarehouseAsync(request));

    [HttpPost("{warehouseId:int}/zones")]
    public async Task<ActionResult<WarehouseZoneDto>> CreateZone(int warehouseId, CreateZoneRequest request)
        => Ok(await _slottingService.CreateZoneAsync(warehouseId, request));

    [HttpPost("zones/{zoneId:int}/bins")]
    public async Task<ActionResult<StorageBinDto>> CreateBin(int zoneId, CreateBinRequest request)
        => Ok(await _slottingService.CreateBinAsync(zoneId, request));

    [HttpDelete("{warehouseId:int}")]
    public async Task<ActionResult<WarehouseActionResult>> DeleteWarehouse(int warehouseId)
    {
        var result = await _slottingService.DeleteWarehouseAsync(warehouseId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("zones/{zoneId:int}")]
    public async Task<ActionResult<WarehouseActionResult>> DeleteZone(int zoneId)
    {
        var result = await _slottingService.DeleteZoneAsync(zoneId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("zones/bins/{binId:int}")]
    public async Task<ActionResult<WarehouseActionResult>> DeleteBin(int binId)
    {
        var result = await _slottingService.DeleteBinAsync(binId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{warehouseId:int}/zones")]
    public async Task<ActionResult<List<WarehouseZoneDto>>> GetZones(int warehouseId)
        => Ok(await _slottingService.GetZonesAsync(warehouseId));

    [HttpGet("zones/{zoneId:int}/bins")]
    public async Task<ActionResult<List<StorageBinDto>>> GetBins(int zoneId)
        => Ok(await _slottingService.GetBinsAsync(zoneId));
}