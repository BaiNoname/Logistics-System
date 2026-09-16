using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TruckingSystem.Application.TransportOrders;
using TruckingSystem.Application.VehicleAllocation;

namespace TruckingSystem.API.Controllers;

[ApiController]
[Route("api/transport-orders")]
[Authorize]
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

    // Endpoint thể hiện thuật toán Greedy — chia xe tự động cho 1 đơn vận chuyển
    [HttpPost("{id:int}/allocate-vehicles")]
    public async Task<ActionResult<AllocateVehiclesResponse>> AllocateVehicles(int id)
    {
        var result = await _allocationService.AllocateAsync(id);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
