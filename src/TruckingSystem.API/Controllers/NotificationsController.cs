using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TruckingSystem.Application.Notifications;

namespace TruckingSystem.API.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _service;

    public NotificationsController(INotificationService service)
    {
        _service = service;
    }

    /// <summary>Quét cảnh báo mới (giấy tờ xe, lịch tài xế) rồi trả về toàn bộ
    /// thông báo gần nhất của người dùng đang đăng nhập.</summary>
    [HttpGet]
    public async Task<ActionResult<List<NotificationDto>>> GetMine()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        return Ok(await _service.CheckAndGetNotificationsAsync(userId));
    }

    [HttpPost("{id:int}/mark-read")]
    public async Task<ActionResult> MarkAsRead(int id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var error = await _service.MarkAsReadAsync(id, userId);
        return error is null ? Ok() : BadRequest(new { message = error });
    }

    [HttpPost("mark-all-read")]
    public async Task<ActionResult> MarkAllAsRead()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        await _service.MarkAllAsReadAsync(userId);
        return Ok();
    }
}
