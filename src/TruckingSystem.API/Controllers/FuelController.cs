using Microsoft.AspNetCore.Mvc;
using TruckingSystem.Application.Fuel;
using Microsoft.AspNetCore.Authorization;

namespace TruckingSystem.API.Controllers;

[ApiController]
[Route("api/fuel")]
// Đọc danh sách hãng + loại nhiên liệu: MỌI tài khoản đăng nhập (Dispatcher cần
// khi ghi đổ xăng cho chuyến, khi Thêm/Sửa xe). Thêm/xóa hãng, xem/thêm/xóa giá:
// chỉ Admin + Accountant như trước (đặt riêng trên từng endpoint bên dưới).
[Authorize]
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

    /// <summary>Loại nhiên liệu cho dropdown. pricedOnly=true: chỉ loại đã có giá.</summary>
    [HttpGet("types")]
    public async Task<ActionResult<List<string>>> GetFuelTypes([FromQuery] bool pricedOnly = false)
        => Ok(await _service.GetFuelTypesAsync(pricedOnly));

    [HttpPost("providers")]
    [Authorize(Roles = "Admin,Accountant")]
    public async Task<ActionResult<FuelProviderDto>> CreateProvider(CreateFuelProviderRequest request)
        => Ok(await _service.CreateProviderAsync(request));

    [HttpDelete("providers/{id:int}")]
    [Authorize(Roles = "Admin,Accountant")]
    public async Task<ActionResult<FuelActionResult>> DeleteProvider(int id)
    {
        var result = await _service.DeleteProviderAsync(id);
        return Ok(result); // luôn 200 OK — Desktop tự đọc result.Success/Message, tránh PostAsync throw exception khi 400 làm mất thông báo rõ ràng
    }

    /// <summary>providerId bỏ trống lấy toàn bộ lịch sử giá.</summary>
    [HttpGet("prices")]
    [Authorize(Roles = "Admin,Accountant")]
    public async Task<ActionResult<List<FuelPriceHistoryDto>>> GetPrices([FromQuery] int? providerId)
        => Ok(await _service.GetPriceHistoryAsync(providerId));

    [HttpPost("prices")]
    [Authorize(Roles = "Admin,Accountant")]
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
    [Authorize(Roles = "Admin,Accountant")]
    public async Task<ActionResult<FuelActionResult>> DeletePrice(int priceId)
        => Ok(await _service.DeletePriceAsync(priceId));
}
