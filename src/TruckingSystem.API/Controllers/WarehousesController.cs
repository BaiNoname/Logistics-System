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

    [HttpPut("{warehouseId:int}")]
    public async Task<ActionResult<WarehouseActionResult>> UpdateWarehouse(int warehouseId, UpdateWarehouseRequest request)
    {
        var result = await _slottingService.UpdateWarehouseAsync(warehouseId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{warehouseId:int}/zones")]
    public async Task<ActionResult<WarehouseZoneDto>> CreateZone(int warehouseId, CreateZoneRequest request)
    {
        try
        {
            return Ok(await _slottingService.CreateZoneAsync(warehouseId, request));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("zones/{zoneId:int}")]
    public async Task<ActionResult<WarehouseActionResult>> UpdateZone(int zoneId, CreateZoneRequest request)
    {
        var result = await _slottingService.UpdateZoneAsync(zoneId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("zones/{zoneId:int}/bins")]
    public async Task<ActionResult<StorageBinDto>> CreateBin(int zoneId, CreateBinRequest request)
    {
        try
        {
            return Ok(await _slottingService.CreateBinAsync(zoneId, request));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("zones/bins/{binId:int}")]
    public async Task<ActionResult<WarehouseActionResult>> UpdateBin(int binId, CreateBinRequest request)
    {
        var result = await _slottingService.UpdateBinAsync(binId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

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

    /// <summary>Nhập kho THỦ CÔNG — không gắn Trip nào (VD: hàng dư, hàng trả về).
    /// ChosenBinId có giá trị → dùng đúng vị trí đó; null → tự động Best-Fit.</summary>
    [HttpPost("{warehouseId:int}/receive-manual")]
    public async Task<ActionResult<ReceiveGoodsResult>> ReceiveManual(int warehouseId, ManualStockRequest request)
    {
        var result = await _slottingService.ReceiveGoodsAsync(warehouseId, request.TransportOrderId, request.CargoLineId, request.Qty, 1, tripId: null, chosenBinId: request.ChosenBinId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{warehouseId:int}/cargo-lines/{cargoLineId:int}/available-bins")]
    public async Task<ActionResult<List<StorageBinDto>>> GetAvailableBins(int warehouseId, int cargoLineId)
        => Ok(await _slottingService.GetAvailableBinsForCargoAsync(warehouseId, cargoLineId));

    /// <summary>Xuất kho THỦ CÔNG — không gắn Trip nào.</summary>
    [HttpPost("{warehouseId:int}/issue-manual")]
    public async Task<ActionResult<IssueGoodsResult>> IssueManual(int warehouseId, ManualStockRequest request)
    {
        var result = await _slottingService.IssueGoodsAsync(warehouseId, request.TransportOrderId, request.CargoLineId, request.Qty, 1, tripId: null);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("orders/{transportOrderId:int}/cargo-lines")]
    public async Task<ActionResult<List<CargoLineOptionDto>>> GetCargoLines(int transportOrderId)
        => Ok(await _slottingService.GetCargoLinesForOrderAsync(transportOrderId));

    [HttpGet("{warehouseId:int}/transactions")]
    public async Task<ActionResult<List<WarehouseTransactionDto>>> GetTransactions(int warehouseId)
        => Ok(await _slottingService.GetTransactionHistoryAsync(warehouseId));
}