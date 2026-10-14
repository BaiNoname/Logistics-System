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
            // Lịch Google của chặng Giao chưa chạy lấy hạn giao làm mốc giờ -> đồng bộ lại (nếu đã đồng bộ).
            var order = await _service.GetByIdAsync(id);
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            foreach (var trip in order?.Trips.Where(t => t.LegType == "Delivery" && t.Status == "Planned") ?? Enumerable.Empty<TripSummaryDto>())
            {
                try { await _tripService.ResyncCalendarIfSyncedAsync(trip.Id, baseUrl); }
                catch { /* lịch không phải dữ liệu chính — không làm hỏng thao tác đổi hạn */ }
            }
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
    public async Task<ActionResult<AllocateVehiclesResponse>> AllocateVehicles(int id, [FromQuery] bool acceptLate = false)
    {
        var result = await _allocationService.AllocateAsync(id, acceptLate,
            User.FindFirstValue(ClaimTypes.NameIdentifier), User.FindFirstValue(ClaimTypes.Name));
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
