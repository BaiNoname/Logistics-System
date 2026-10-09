using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TruckingSystem.Application.TripPhotos;

namespace TruckingSystem.API.Controllers;

/// <summary>Kiểm hàng + ảnh hàng hóa khi tài xế xác nhận chặng.
///   GET  /api/trips/{id}/cargo-checklist — app tài xế: tên hàng + số lượng hiện SẴN (không cần đăng nhập, giống /ping)
///   POST /api/trips/{id}/photos          — app tài xế gửi 1 ảnh hàng (không cần đăng nhập, giống /videos)
///   GET  /api/trips/{id}/photos          — Desktop: danh sách ảnh để Vận hành kiểm tra trước khi Duyệt
///   GET  /api/trips/photos/{photoId}/file — Desktop: tải ảnh (cần đăng nhập)</summary>
[ApiController]
[Route("api/trips")]
[Authorize(Roles = "Admin,Dispatcher")]
public class TripPhotosController : ControllerBase
{
    private const long MaxUploadBytes = 16L * 1024 * 1024;

    private readonly ITripPhotoService _photoService;

    public TripPhotosController(ITripPhotoService photoService)
    {
        _photoService = photoService;
    }

    [HttpGet("{id:int}/cargo-checklist")]
    [AllowAnonymous]
    public async Task<ActionResult<TripCargoChecklistDto>> GetChecklist(int id)
    {
        var checklist = await _photoService.GetChecklistAsync(id);
        return checklist is null ? NotFound(new { message = "Không tìm thấy chuyến." }) : Ok(checklist);
    }

    [HttpPost("{id:int}/photos")]
    [AllowAnonymous]
    [RequestSizeLimit(MaxUploadBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxUploadBytes)]
    public async Task<IActionResult> Upload(int id, IFormFile? file, [FromForm] DateTime? takenAt, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { success = false, message = "Thiếu file ảnh." });

        await using var stream = file.OpenReadStream();
        var result = await _photoService.UploadAsync(id, stream, file.FileName, file.ContentType, takenAt, ct);
        return result.Success
            ? Ok(new { success = true, photoId = result.PhotoId, photoCount = result.PhotoCount })
            : BadRequest(new { success = false, message = result.Message, photoCount = result.PhotoCount });
    }

    [HttpGet("{id:int}/photos")]
    public async Task<ActionResult<List<TripPhotoDto>>> List(int id)
        => Ok(await _photoService.GetPhotosAsync(id));

    [HttpGet("photos/{photoId:int}/file")]
    public async Task<IActionResult> GetFile(int photoId)
    {
        var file = await _photoService.GetFileAsync(photoId);
        return file is null ? NotFound(new { message = "Không tìm thấy ảnh." }) : PhysicalFile(file.FullPath, file.ContentType);
    }
}
