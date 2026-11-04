using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TruckingSystem.Application.Surcharges;

namespace TruckingSystem.API.Controllers;

[ApiController]
[Route("api/surcharge-lists")]
[Authorize(Roles = "Admin,Accountant,Dispatcher")]
public sealed class SurchargeListsController(ISurchargeListService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List() => Ok(await service.ListAsync());

    [HttpPost, Authorize(Roles = "Admin,Accountant")]
    public Task<IActionResult> Create(SaveDraft input) => Write(() => service.CreateAsync(input));

    [HttpPost("{id:int}/copy"), Authorize(Roles = "Admin,Accountant")]
    public Task<IActionResult> Copy(int id, CopyDraft input) => Write(() => service.CopyAsync(id, input));

    [HttpPut("{id:int}"), Authorize(Roles = "Admin,Accountant")]
    public Task<IActionResult> Update(int id, SaveDraft input) => Write(() => service.UpdateAsync(id, input));

    [HttpPost("{id:int}/publish"), Authorize(Roles = "Admin,Accountant")]
    public Task<IActionResult> Publish(int id, PublishDraft input) => Write(() => service.PublishAsync(id, input));

    private async Task<IActionResult> Write(Func<Task<SurchargeListDto>> action)
    {
        try { return Ok(await action()); }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
    }
}
