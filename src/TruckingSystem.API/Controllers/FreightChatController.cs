using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TruckingSystem.Application.FreightChat;

namespace TruckingSystem.API.Controllers;

[ApiController, AllowAnonymous, Route("api/public/freight-chat")]
[EnableRateLimiting("freight-chat")]
public class FreightChatController(FreightChatService service) : ControllerBase
{
    [HttpPost, RequestSizeLimit(120000)]
    public async Task<IActionResult> Chat(FreightChatRequest request, CancellationToken ct)
    {
        try { return Ok(await service.ChatAsync(request, ct)); }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { return StatusCode(499); }
        catch (FreightServiceUnavailableException ex) { return StatusCode(503, new { message = ex.Message }); }
        catch (Exception) { return StatusCode(503, new { message = "Dịch vụ tra cước chưa sẵn sàng. Vui lòng thử lại sau." }); }
    }
}
