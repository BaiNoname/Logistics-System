using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TruckingSystem.Application.TransportOrders;
using TruckingSystem.Application.VehicleAllocation;

namespace TruckingSystem.API.Controllers;

[ApiController]
[Route("api/transport-orders")]
[Authorize(Roles = "Admin,Dispatcher")]
public class TransportOrdersController : ControllerBase
{
    private readonly ITransportOrderService _service;
    private readonly IVehicleAllocationService _allocationService;

    public TransportOrdersController(ITransportOrderService service, IVehicleAllocationService allocationService)
    {
        _service = service;
        _allocationService = allocationService;
    }

    [HttpGet]
    public async Task<ActionResult<List<TransportOrderListItemDto>>> GetAll()
        => Ok(await _service.GetAllAsync());

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

    // Endpoint thể hiện thuật toán Greedy — chia xe tự động cho 1 đơn vận chuyển
    [HttpPost("{id:int}/allocate-vehicles")]
    public async Task<ActionResult<AllocateVehiclesResponse>> AllocateVehicles(int id)
    {
        var result = await _allocationService.AllocateAsync(id);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
