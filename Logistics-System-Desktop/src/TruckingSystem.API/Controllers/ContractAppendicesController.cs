using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TruckingSystem.Application.Contracts;

namespace TruckingSystem.API.Controllers;

[ApiController]
[Route("api/contract-appendices")]
[Authorize(Roles = "Admin,Dispatcher,Accountant")]
public class ContractAppendicesController(IContractAppendixService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<ContractAppendixDto>>> GetAll()
        => Ok(await service.GetAllAsync());

    [HttpGet("lookups")]
    public async Task<ActionResult<ContractAppendixLookupsDto>> GetLookups()
        => Ok(await service.GetLookupsAsync());

    [HttpPost]
    public async Task<ActionResult<ContractAppendixDto>> Create(SaveContractAppendixRequest request)
    {
        try { return Ok(await service.CreateAsync(request, User.IsInRole("Admin"))); }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, SaveContractAppendixRequest request)
    {
        try { return await service.UpdateAsync(id, request, User.IsInRole("Admin")) ? NoContent() : NotFound(); }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
        => await service.DeleteAsync(id) ? NoContent() : NotFound();
}
