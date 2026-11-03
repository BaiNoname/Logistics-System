using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TruckingSystem.Application.Pricing;

namespace TruckingSystem.API.Controllers;

[ApiController]
[Route("api/price-lists")]
[Authorize(Roles = "Admin,Dispatcher,Accountant")]
public class PriceListsController(IPricingService service) : ControllerBase
{
    [HttpGet("surcharges")]
    public async Task<IActionResult> HistoricalRates([FromServices] TruckingSystem.Application.Surcharges.ISurchargeListService lists)
        => Ok((await lists.ListAsync()).Where(p => p.Published)
            .Select(p => new SurchargeRates(p.EffectiveDate, p.FragilePercent, p.HazmatPercent)));

    [HttpPost("surcharges")]
    public IActionResult RetiredSurchargeWrite() => StatusCode(410, new { message = "Vui lòng cập nhật Desktop và công bố từ mục Phụ phí." });

    [HttpGet]
    public async Task<ActionResult<List<PriceListDto>>> GetAll()
        => Ok(await service.GetPriceListsAsync());

    [HttpGet("vehicle-types")]
    public async Task<ActionResult<List<string>>> GetVehicleTypes()
        => Ok(await service.GetVehicleTypesAsync());

    [HttpPost]
    public async Task<ActionResult<PriceListDto>> Create(SavePriceListRequest request)
    {
        try { return Ok(await service.CreatePriceListAsync(request)); }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpPost("{id:int}/copy")]
    public async Task<ActionResult<PriceListDto>> Copy(int id, CopyPriceListRequest request)
    {
        try
        {
            var copy = await service.CopyPriceListAsync(id, request);
            return copy is null
                ? NotFound(new { message = "Bảng giá nguồn không tồn tại. Vui lòng làm mới danh sách." })
                : Ok(copy);
        }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, SavePriceListRequest request)
    {
        try { return await service.UpdatePriceListAsync(id, request) ? NoContent() : NotFound(); }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        try { return await service.DeletePriceListAsync(id) ? NoContent() : NotFound(); }
        catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
    }

    [HttpGet("{priceListId:int}/tariff-rules")]
    public async Task<ActionResult<List<TariffRuleDto>>> GetRules(int priceListId)
        => Ok(await service.GetTariffRulesAsync(priceListId));

    [HttpPost("{priceListId:int}/tariff-rules")]
    public async Task<ActionResult<TariffRuleDto>> CreateRule(int priceListId, SaveTariffRuleRequest request)
    {
        try { return Ok(await service.CreateTariffRuleAsync(priceListId, request)); }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
    }

    [HttpPut("{priceListId:int}/tariff-rules/{id:int}")]
    public async Task<IActionResult> UpdateRule(int priceListId, int id, SaveTariffRuleRequest request)
    {
        try { return await service.UpdateTariffRuleAsync(priceListId, id, request) ? NoContent() : NotFound(); }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
    }

    [HttpDelete("{priceListId:int}/tariff-rules/{id:int}")]
    public async Task<IActionResult> DeleteRule(int priceListId, int id)
    {
        try { return await service.DeleteTariffRuleAsync(priceListId, id) ? NoContent() : NotFound(); }
        catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
    }
}
