using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TruckingSystem.Application.Invoices;

namespace TruckingSystem.API.Controllers;

[ApiController]
[Route("api/invoices")]
[Authorize]
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
}
