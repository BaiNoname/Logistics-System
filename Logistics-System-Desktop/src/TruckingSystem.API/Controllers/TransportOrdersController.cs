using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TruckingSystem.Application.TransportOrders;
using TruckingSystem.Application.Trips;
using TruckingSystem.Application.VehicleAllocation;

namespace TruckingSystem.API.Controllers;

[ApiController]
[Route("api/transport-orders")]
[Authorize(Roles = "Admin,Dispatcher")]
public class TransportOrdersController : ControllerBase
{
    private readonly ITransportOrderService _service;
    private readonly IVehicleAllocationService _allocationService;
    private readonly ITripService _tripService;

    public TransportOrdersController(ITransportOrderService service, IVehicleAllocationService allocationService, ITripService tripService)
    {
        _service = service;
        _allocationService = allocationService;
        _tripService = tripService;
    }

    [HttpGet]
    public async Task<ActionResult<List<TransportOrderListItemDto>>> GetAll()
        => Ok(await _service.GetAllAsync());

    /// <summary>Gợi ý "Loại hàng" cho form tạo đơn.</summary>
    [HttpGet("cargo-types")]
    public async Task<ActionResult<List<string>>> GetCargoTypes()
        => Ok(await _service.GetCargoTypeSuggestionsAsync());

    /// <summary>Hợp đồng đang hiệu lực của khách — dropdown "Hợp đồng" trên form tạo / sửa đơn.</summary>
    [HttpGet("contract-options")]
    public async Task<ActionResult<List<OrderContractOptionDto>>> GetContractOptions([FromQuery] int customerId)
        => Ok(await _service.GetContractOptionsAsync(customerId));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TransportOrderDetailDto>> GetById(int id)
    {
        var order = await _service.GetByIdAsync(id);
        return order is null ? NotFound() : Ok(order);
    }

    [HttpPost]
    public async Task<ActionResult<TransportOrderDetailDto>> Create(CreateTransportOrderRequest request)
    {
        try
        {
            var created = await _service.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>Sửa đơn — CHỈ áp dụng được khi đơn còn Draft.</summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<OrderActionResult>> Update(int id, UpdateTransportOrderRequest request)
    {
        var result = await _service.UpdateAsync(id, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Draft -> Confirmed.</summary>
    [HttpPost("{id:int}/confirm")]
    public async Task<ActionResult<OrderActionResult>> Confirm(int id)
    {
        var result = await _service.ConfirmAsync(id);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>-> Cancelled.</summary>
    [HttpPost("{id:int}/cancel")]
    public async Task<ActionResult<OrderActionResult>> Cancel(int id)
    {
        var result = await _service.CancelAsync(id);
        // Lịch Google: các chuyến bị hủy chuyển màu ĐỎ (không xóa nữa); chuyến Trả hàng mới sinh ra được
        // tạo sự kiện nếu đơn đã từng đồng bộ lịch.
        if (result.Success) await _tripService.SyncOrderCalendarsAsync(id, PublicBaseUrl, createMissing: true, onlyIfOrderSynced: true);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Phí lưu kho ƯỚC TÍNH nếu đổi hạn giao sang newDate (requestedBy = Customer / Company).</summary>
    [HttpGet("{id:int}/storage-fee-preview")]
    public async Task<ActionResult<StorageFeePreviewDto>> PreviewStorageFee(int id, [FromQuery] DateTime newDate, [FromQuery] string? requestedBy)
    {
        var result = await _service.PreviewStorageFeeAsync(id, newDate, requestedBy);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>PHÍ LƯU KHO của đơn do khách dời hạn giao — từng lần đổi hạn + tổng (phần hóa đơn dùng số này).</summary>
    [HttpGet("{id:int}/storage-fee")]
    public async Task<ActionResult<StorageFeeDto>> GetStorageFee(int id)
    {
        var result = await _service.GetStorageFeeAsync(id);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Ghi nhận BỔ SUNG phí lưu kho do khách chậm nhận hàng (không đổi hạn giao) — dùng được cả khi
    /// đơn đã Hoàn tất / Đã hủy (nhân viên quên "Đổi hạn giao" lúc khách dời ngày).</summary>
    [HttpPost("{id:int}/storage-fee-records")]
    public async Task<ActionResult<OrderActionResult>> RecordStorageFee(int id, RecordStorageFeeRequest request)
    {
        var result = await _service.RecordStorageFeeAsync(id, request,
            User.FindFirstValue(ClaimTypes.NameIdentifier), User.FindFirstValue(ClaimTypes.Name));
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Hàng nằm kho QUÁ HẠN GIAO: chọn do khách hàng (tính phí lưu kho) hay do công ty (0 đ).</summary>
    [HttpPost("{id:int}/storage-responsibility")]
    public async Task<ActionResult<OrderActionResult>> SetStorageResponsibility(int id, StorageResponsibilityRequest request)
    {
        var result = await _service.SetStorageResponsibilityAsync(id, request,
            User.FindFirstValue(ClaimTypes.NameIdentifier), User.FindFirstValue(ClaimTypes.Name));
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Đổi / bổ sung hạn giao cho đơn Đã xác nhận / Đang chạy (bắt buộc lý do, ghi nhật ký).</summary>
    [HttpPost("{id:int}/deadline")]
    public async Task<ActionResult<OrderActionResult>> ChangeDeadline(int id, ChangeDeadlineRequest request)
    {
        var result = await _service.ChangeDeadlineAsync(id, request,
            User.FindFirstValue(ClaimTypes.NameIdentifier), User.FindFirstValue(ClaimTypes.Name));
        if (result.Success)
        {
            // Lịch Google ghi hạn giao trong nội dung sự kiện -> cập nhật các chuyến đã đồng bộ.
            await _tripService.SyncOrderCalendarsAsync(id, PublicBaseUrl, createMissing: false, onlyIfOrderSynced: false);
        }
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Ước tính số ngày chạy của tuyến — form tạo đơn dùng để GỢI Ý hạn giao.</summary>
    [HttpGet("estimate-days")]
    public async Task<ActionResult<object>> EstimateDays([FromQuery] int? pickupLocationId, [FromQuery] int? dropoffLocationId,
        [FromQuery] int warehouseId, [FromQuery] string pickupMethod = "CompanyPickup")
        => Ok(new { days = await _allocationService.EstimateRouteDaysAsync(pickupLocationId, dropoffLocationId, warehouseId, pickupMethod) });

    // Endpoint thể hiện thuật toán Greedy — chia xe tự động cho 1 đơn vận chuyển
    [HttpPost("{id:int}/allocate-vehicles")]
    // acceptLate=true: người dùng đã xác nhận vẫn chia dù ước tính không kịp hạn giao.
    // plannedStart (yyyy-MM-dd): ngày xuất phát dự kiến — null = hôm nay.
    public async Task<ActionResult<AllocateVehiclesResponse>> AllocateVehicles(int id, [FromQuery] bool acceptLate = false, [FromQuery] DateTime? plannedStart = null)
    {
        var result = await _allocationService.AllocateAsync(id, acceptLate,
            User.FindFirstValue(ClaimTypes.NameIdentifier), User.FindFirstValue(ClaimTypes.Name), plannedStart);
        // Chia xe xong -> TỰ tạo sự kiện lịch (màu xám "Chưa chạy") cho tài xế / khách đã khai Google Calendar ID:
        // tài xế nhận ngay MÃ CHUYẾN để mở app, không phải chờ Vận hành bấm Đồng bộ lịch.
        if (result.Success) await _tripService.SyncOrderCalendarsAsync(id, PublicBaseUrl, createMissing: true, onlyIfOrderSynced: false);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    private string PublicBaseUrl => PublicUrl.For(Request);
}
