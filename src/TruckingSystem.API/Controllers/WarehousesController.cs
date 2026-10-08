using System.Security.Claims;
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
        // Màn "Kiểm tra Best-Fit": chỉ gợi ý, nhưng số lượng phải theo đúng quy
        // tắc nhập kho (không vượt phần còn được nhập của dòng hàng).
        var result = await _slottingService.PreviewSuggestBinAsync(warehouseId, request);
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
        return Ok(result); // luôn 200 OK — Desktop tự đọc result.Success/Message, tránh PostAsync throw exception khi 400 làm mất thông báo rõ ràng
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
        return Ok(result); // luôn 200 OK — Desktop tự đọc result.Success/Message, tránh PostAsync throw exception khi 400 làm mất thông báo rõ ràng
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
        return Ok(result); // luôn 200 OK — Desktop tự đọc result.Success/Message, tránh PostAsync throw exception khi 400 làm mất thông báo rõ ràng
    }

    [HttpDelete("{warehouseId:int}")]
    public async Task<ActionResult<WarehouseActionResult>> DeleteWarehouse(int warehouseId)
    {
        var result = await _slottingService.DeleteWarehouseAsync(warehouseId);
        return Ok(result); // luôn 200 OK — Desktop tự đọc result.Success/Message, tránh PostAsync throw exception khi 400 làm mất thông báo rõ ràng
    }

    [HttpDelete("zones/{zoneId:int}")]
    public async Task<ActionResult<WarehouseActionResult>> DeleteZone(int zoneId)
    {
        var result = await _slottingService.DeleteZoneAsync(zoneId);
        return Ok(result); // luôn 200 OK — Desktop tự đọc result.Success/Message, tránh PostAsync throw exception khi 400 làm mất thông báo rõ ràng
    }

    [HttpDelete("zones/bins/{binId:int}")]
    public async Task<ActionResult<WarehouseActionResult>> DeleteBin(int binId)
    {
        var result = await _slottingService.DeleteBinAsync(binId);
        return Ok(result); // luôn 200 OK — Desktop tự đọc result.Success/Message, tránh PostAsync throw exception khi 400 làm mất thông báo rõ ràng
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
        // Người thực hiện = tài khoản đang đăng nhập (trước đây gán CỨNG nhân
        // viên số 1 -> lịch sử kho ghi sai người, VD hiện tên 1 tài xế bất kỳ).
        var performer = await CurrentPerformerAsync();
        var result = await _slottingService.ReceiveGoodsAsync(warehouseId, request.TransportOrderId, request.CargoLineId, request.Qty,
            performer.EmployeeId, tripId: null, chosenBinId: request.ChosenBinId, performedByUserName: performer.UserName,
            isManual: true);
        // LUÔN trả về 200 OK (kể cả khi thất bại) — để Desktop nhận đúng
        // object ReceiveGoodsResult và tự đọc result.Success/result.Message
        // hiện thông báo SẠCH đã thiết kế sẵn. Trước đây trả 400 khi thất bại
        // khiến PostAsync bên Desktop THROW EXCEPTION thay vì trả về object,
        // làm mất luôn thông báo rõ ràng, chỉ còn thấy "Lỗi API (400): {json
        // thô}" khó hiểu.
        return Ok(result);
    }

    [HttpGet("{warehouseId:int}/cargo-lines/{cargoLineId:int}/available-bins")]
    public async Task<ActionResult<List<StorageBinDto>>> GetAvailableBins(int warehouseId, int cargoLineId)
        => Ok(await _slottingService.GetAvailableBinsForCargoAsync(warehouseId, cargoLineId));

    /// <summary>Xuất kho THỦ CÔNG — không gắn Trip nào.</summary>
    [HttpPost("{warehouseId:int}/issue-manual")]
    public async Task<ActionResult<IssueGoodsResult>> IssueManual(int warehouseId, ManualStockRequest request)
    {
        var performer = await CurrentPerformerAsync();
        var result = await _slottingService.IssueManualAsync(warehouseId, request.TransportOrderId, request.CargoLineId, request.Qty, performer);
        return Ok(result); // luôn 200 OK — cùng lý do như ReceiveManual ở trên
    }

    /// <summary>Hoàn tác 1 phiếu xuất THỦ CÔNG làm nhầm (trả hàng lại vào kho).</summary>
    [HttpPost("issues/{issueId:int}/reverse")]
    public async Task<ActionResult<WarehouseActionResult>> ReverseManualIssue(int issueId)
        => Ok(await _slottingService.ReverseManualIssueAsync(issueId, User.FindFirstValue(ClaimTypes.Name)));

    /// <summary>Chỉ các đơn liên quan tới kho này — dùng cho màn Nhập/Xuất kho.</summary>
    [HttpGet("{warehouseId:int}/orders")]
    public async Task<ActionResult<List<WarehouseOrderDto>>> GetOrdersForWarehouse(int warehouseId)
        => Ok(await _slottingService.GetOrdersForWarehouseAsync(warehouseId));

    /// <summary>Toàn cảnh hàng của 1 đơn trong 1 kho (đã nhập/xuất/còn/đang giữ + từng chuyến).</summary>
    [HttpGet("{warehouseId:int}/orders/{transportOrderId:int}/stock-flow")]
    public async Task<ActionResult<OrderStockFlowDto>> GetOrderStockFlow(int warehouseId, int transportOrderId)
    {
        var flow = await _slottingService.GetOrderStockFlowAsync(transportOrderId, warehouseId);
        return flow is null ? NotFound() : Ok(flow);
    }

    private async Task<StockPerformer> CurrentPerformerAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userName = User.FindFirstValue(ClaimTypes.Name);
        return new StockPerformer(await _slottingService.ResolveEmployeeIdAsync(userId), userName);
    }

    [HttpGet("orders/{transportOrderId:int}/cargo-lines")]
    public async Task<ActionResult<List<CargoLineOptionDto>>> GetCargoLines(int transportOrderId, [FromQuery] int warehouseId)
        => Ok(await _slottingService.GetCargoLinesForOrderAsync(transportOrderId, warehouseId));

    [HttpGet("{warehouseId:int}/transactions")]
    public async Task<ActionResult<List<WarehouseTransactionDto>>> GetTransactions(int warehouseId)
        => Ok(await _slottingService.GetTransactionHistoryAsync(warehouseId));
}