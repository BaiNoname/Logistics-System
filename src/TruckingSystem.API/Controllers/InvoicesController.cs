using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TruckingSystem.Application.Invoices;

namespace TruckingSystem.API.Controllers;

[ApiController]
[Route("api/invoices")]
[Authorize(Roles = "Admin,Accountant")]
public class InvoicesController : ControllerBase
{
    private readonly IInvoiceService _service;

    public InvoicesController(IInvoiceService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<List<InvoiceListItemDto>>> GetAll()
        => Ok(await _service.GetAllAsync());

    [HttpGet("{id:int}")]
    public async Task<ActionResult<InvoiceDetailDto>> GetDetail(int id)
    {
        try { return Ok(await _service.GetDetailAsync(id)); }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
    }

    [HttpGet("eligible-orders")]
    public async Task<ActionResult<List<InvoiceOrderOptionDto>>> GetEligibleOrders()
        => Ok(await _service.GetEligibleOrdersAsync());

    [HttpGet("orders/{orderId:int}/quote")]
    public async Task<ActionResult<InvoiceQuoteDto>> GetQuote(int orderId)
    {
        try { return Ok(await _service.GetQuoteAsync(orderId)); }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
    }

    [HttpPost]
    public async Task<ActionResult<InvoiceListItemDto>> Create(CreateInvoiceRequest request)
    {
        try { return Ok(await _service.CreateAsync(request)); }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
    }
}
