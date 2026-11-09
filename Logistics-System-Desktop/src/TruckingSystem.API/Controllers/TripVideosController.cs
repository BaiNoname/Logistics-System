using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using TruckingSystem.Application.TripVideos;

namespace TruckingSystem.API.Controllers;

/// <summary>Video hành trình của chuyến.
///   POST /api/trips/{id}/videos           — app tài xế gửi 1 đoạn video (giống /ping: không cần đăng nhập)
///   GET  /api/trips/{id}/videos           — Desktop lấy danh sách đoạn + link xem có chữ ký
///   GET  /api/trips/videos/{id}/stream    — phát video (hỗ trợ tua), kiểm tra chữ ký link
///   GET  /api/trips/videos/{id}/verify    — đối chiếu SHA-256, chứng minh video chưa bị sửa
///
/// Vì sao link xem cần "chữ ký": thẻ &lt;video&gt; trong WebView2 không gửi được
/// header Authorization (JWT). Nên API cấp link có hạn 6 giờ kèm chữ ký HMAC
/// (khóa bí mật = Jwt:Key) — ai không có link hợp lệ thì không xem được.</summary>
[ApiController]
[Route("api/trips")]
[Authorize(Roles = "Admin,Dispatcher")]
public class TripVideosController : ControllerBase
{
    private const long MaxUploadBytes = 200L * 1024 * 1024; // 200 MB/đoạn — đoạn 1 phút 480p chỉ ~8-12 MB
    private static readonly TimeSpan LinkLifetime = TimeSpan.FromHours(6);

    private readonly ITripVideoService _videoService;
    private readonly byte[] _signingKey;

    public TripVideosController(ITripVideoService videoService, IConfiguration configuration)
    {
        _videoService = videoService;
        var key = configuration["VideoStorage:SigningKey"];
        if (string.IsNullOrWhiteSpace(key)) key = configuration["Jwt:Key"] ?? "trip-video-default-key";
        _signingKey = Encoding.UTF8.GetBytes(key);
    }

    [HttpPost("{id:int}/videos")]
    [AllowAnonymous]
    [RequestSizeLimit(MaxUploadBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxUploadBytes)]
    public async Task<IActionResult> Upload(int id, IFormFile? file, [FromForm] string? startedAt, [FromForm] string? endedAt, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { success = false, message = "Thiếu file video." });
        if (!TryParseUtc(startedAt, out var start) || !TryParseUtc(endedAt, out var end))
            return BadRequest(new { success = false, message = "startedAt/endedAt phải là thời gian ISO-8601 (VD 2026-10-06T01:30:00.000Z)." });

        await using var stream = file.OpenReadStream();
        var result = await _videoService.UploadAsync(id, stream, file.FileName, file.ContentType, start, end, ct);
        if (!result.Success)
        {
            // 409 = "tạm thời chưa nhận, gửi lại sau" -> app GIỮ file; 400 = từ chối hẳn.
            if (result.Retryable) return StatusCode(StatusCodes.Status409Conflict, new { success = false, retryable = true, message = result.Message });
            return BadRequest(new { success = false, message = result.Message });
        }
        return Ok(new { success = true, message = result.Message, segmentId = result.SegmentId });
    }

    [HttpGet("{id:int}/videos")]
    public async Task<IActionResult> List(int id)
    {
        var segments = await _videoService.GetSegmentsAsync(id);
        var exp = DateTimeOffset.UtcNow.Add(LinkLifetime).ToUnixTimeSeconds();
        return Ok(segments.Select(s => new
        {
            s.Id,
            s.TripId,
            s.StartedAt,
            s.EndedAt,
            s.DurationSeconds,
            s.SizeBytes,
            s.ContentType,
            s.Sha256,
            StreamUrl = $"/api/trips/videos/{s.Id}/stream?exp={exp}&sig={Sign(s.Id, exp)}",
        }));
    }

    [HttpGet("videos/{segmentId:int}/stream")]
    [AllowAnonymous]
    public async Task<IActionResult> Stream(int segmentId, [FromQuery] long exp, [FromQuery] string? sig)
    {
        if (string.IsNullOrEmpty(sig) || DateTimeOffset.UtcNow.ToUnixTimeSeconds() > exp)
            return Unauthorized(new { message = "Link xem video đã hết hạn — mở lại màn hình lộ trình." });

        var expected = Encoding.ASCII.GetBytes(Sign(segmentId, exp));
        var given = Encoding.ASCII.GetBytes(sig);
        if (!CryptographicOperations.FixedTimeEquals(expected, given))
            return Unauthorized(new { message = "Link xem video không hợp lệ." });

        var file = await _videoService.GetFileAsync(segmentId);
        if (file is null) return NotFound(new { message = "Không tìm thấy file video." });

        // enableRangeProcessing: trình phát tải đúng đoạn cần xem khi tua, không phải tải cả file
        return PhysicalFile(file.FullPath, file.ContentType, enableRangeProcessing: true);
    }

    [HttpGet("videos/{segmentId:int}/verify")]
    public async Task<IActionResult> Verify(int segmentId, CancellationToken ct)
    {
        var result = await _videoService.VerifyAsync(segmentId, ct);
        return result is null ? NotFound(new { message = "Không tìm thấy đoạn video." }) : Ok(result);
    }

    private string Sign(int segmentId, long exp)
    {
        using var hmac = new HMACSHA256(_signingKey);
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes($"trip-video:{segmentId}:{exp}"));
        return WebEncoders.Base64UrlEncode(hash);
    }

    private static bool TryParseUtc(string? value, out DateTime result)
        => DateTime.TryParse(value, CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out result);
}
