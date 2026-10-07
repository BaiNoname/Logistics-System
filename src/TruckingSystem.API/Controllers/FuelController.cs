using Microsoft.AspNetCore.Mvc;
using TruckingSystem.Application.Fuel;
using Microsoft.AspNetCore.Authorization;

namespace TruckingSystem.API.Controllers;

[ApiController]
[Route("api/fuel")]
[Authorize(Roles = "Admin,Accountant")]
public class FuelController : ControllerBase
{
    private readonly IFuelService _service;

    public FuelController(IFuelService service)
    {
        _service = service;
    }

    [HttpGet("providers")]
    public async Task<ActionResult<List<FuelProviderDto>>> GetProviders()
        => Ok(await _service.GetProvidersAsync());

    [HttpPost("providers")]
    public async Task<ActionResult<FuelProviderDto>> CreateProvider(CreateFuelProviderRequest request)
        => Ok(await _service.CreateProviderAsync(request));

    [HttpDelete("providers/{id:int}")]
    public async Task<ActionResult<FuelActionResult>> DeleteProvider(int id)
    {
        var result = await _service.DeleteProviderAsync(id);
        return Ok(result); // luôn 200 OK — Desktop tự đọc result.Success/Message, tránh PostAsync throw exception khi 400 làm mất thông báo rõ ràng
    }

    /// <summary>providerId bỏ trống lấy toàn bộ lịch sử giá.</summary>
    [HttpGet("prices")]
    public async Task<ActionResult<List<FuelPriceHistoryDto>>> GetPrices([FromQuery] int? providerId)
        => Ok(await _service.GetPriceHistoryAsync(providerId));

    [HttpPost("prices")]
    public async Task<ActionResult<FuelPriceHistoryDto>> CreatePrice(CreateFuelPriceRequest request)
    {
        try
        {
            return Ok(await _service.CreatePriceAsync(request));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("prices/{priceId:int}")]
    public async Task<ActionResult<FuelActionResult>> DeletePrice(int priceId)
        => Ok(await _service.DeletePriceAsync(priceId));
}
