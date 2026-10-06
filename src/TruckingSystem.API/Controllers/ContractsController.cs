using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TruckingSystem.Application.Contracts;

namespace TruckingSystem.API.Controllers;

[ApiController]
[Route("api/contracts")]
[Authorize]
public class ContractsController(IContractService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<ContractListItemDto>>> GetAll()
        => Ok(await service.GetAllAsync());

    [HttpGet("lookups")]
    public async Task<ActionResult<ContractLookupDto>> GetLookups()
        => Ok(await service.GetLookupsAsync());

    [HttpPost]
    public async Task<ActionResult<ContractListItemDto>> Create(SaveContractRequest request)
    {
        try { return Ok(await service.CreateAsync(request)); }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, SaveContractRequest request)
    {
        try { return await service.UpdateAsync(id, request) ? NoContent() : NotFound(); }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
    }
}
