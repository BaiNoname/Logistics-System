using Microsoft.EntityFrameworkCore;
using TruckingSystem.Application.Common.Interfaces;
using TruckingSystem.Domain.Entities;
using TruckingSystem.Domain.Enums;

namespace TruckingSystem.Application.TripVideos;

/// <summary>Nghiệp vụ video hành trình. Video là TÙY CHỌN — chuyến không có
/// video vẫn hoạt động bình thường (GPS độc lập hoàn toàn với video).</summary>
public class TripVideoService : ITripVideoService
{
    private const double MaxSegmentMinutes = 15; // app quay 1 phút/đoạn; nới rộng để chịu được sai lệch
    private static readonly Dictionary<string, string> AllowedTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        [".mp4"] = "video/mp4",
        [".m4v"] = "video/mp4",
        [".mov"] = "video/quicktime",
        [".3gp"] = "video/3gpp",
        [".webm"] = "video/webm",
    };

    private readonly IApplicationDbContext _db;
    private readonly IVideoStorage _storage;

    public TripVideoService(IApplicationDbContext db, IVideoStorage storage)
    {
        _db = db;
        _storage = storage;
    }

    public async Task<UploadTripVideoResult> UploadAsync(int tripId, Stream content, string originalFileName, string? contentType,
        DateTime startedAt, DateTime endedAt, CancellationToken ct = default)
    {
        var trip = await _db.Trips
            .Where(t => t.Id == tripId)
            .Select(t => new { t.Status, t.StartTime, t.EndTime })
            .FirstOrDefaultAsync(ct);
        if (trip is null) return new UploadTripVideoResult(false, "Không tìm thấy chuyến.", null);

        startedAt = ToUtc(startedAt);
        endedAt = ToUtc(endedAt);
        if (endedAt <= startedAt)
            return new UploadTripVideoResult(false, "Thời gian kết thúc phải sau thời gian bắt đầu.", null);

        // Video chỉ thuộc về khoảng thời gian chuyến chạy. Giờ bắt đầu chuyến = lúc
        // server nhận được GPS ĐẦU TIÊN — nếu xe xuất phát ở chỗ mất sóng, video có thể
        // được quay TRƯỚC giờ đó khá lâu -> cho lệch tới 3 tiếng phía trước.
        //  - Chuyến CHƯA được ghi nhận bắt đầu: KHÔNG từ chối hẳn (Retryable -> 409),
        //    app giữ file và gửi lại sau khi GPS lên được (trước đây trả 400 -> app XÓA video).
        //  - Quay sau khi chuyến kết thúc quá 30 phút: từ chối hẳn.
        if (trip.StartTime is null && trip.Status is TripStatus.Cancelled or TripStatus.Completed)
            return new UploadTripVideoResult(false, "Chuyến đã đóng mà chưa từng chạy — không nhận video.", null);
        if (trip.Status == TripStatus.Planned || trip.StartTime is null)
            return new UploadTripVideoResult(false, "Chuyến chưa được ghi nhận bắt đầu — app sẽ gửi lại sau.", null, Retryable: true);
        if (startedAt < trip.StartTime.Value - TimeSpan.FromHours(3))
            return new UploadTripVideoResult(false, "Đoạn video được quay quá sớm so với giờ bắt đầu chuyến — không nhận.", null);
        if (trip.EndTime is { } end && startedAt > end + TimeSpan.FromMinutes(30))
            return new UploadTripVideoResult(false, "Đoạn video được quay SAU khi chuyến đã kết thúc — không nhận.", null);
        if ((endedAt - startedAt).TotalMinutes > MaxSegmentMinutes)
            return new UploadTripVideoResult(false, $"Mỗi đoạn video tối đa {MaxSegmentMinutes} phút.", null);

        // Một số thư viện upload không gửi kèm tên file (VD chỉ là "blob") -> suy
        // ra đuôi file từ Content-Type; không có nữa thì mặc định .mp4 (Android
        // luôn quay ra mp4).
        var ext = Path.GetExtension(originalFileName ?? "");
        if (string.IsNullOrEmpty(ext))
        {
            ext = contentType?.ToLowerInvariant() switch
            {
                "video/quicktime" => ".mov",
                "video/3gpp" => ".3gp",
                "video/webm" => ".webm",
                _ => ".mp4",
            };
        }
        if (!AllowedTypes.TryGetValue(ext, out var mappedType))
            return new UploadTripVideoResult(false, "Định dạng video không hỗ trợ (chỉ nhận mp4, mov, m4v, 3gp, webm).", null);

        // App gửi lại đoạn cũ khi không nhận được phản hồi -> trả về đoạn đã có,
        // không lưu trùng (khóa nhận diện: cùng chuyến + cùng giờ bắt đầu, lệch < 1s).
        var from = startedAt.AddSeconds(-1);
        var to = startedAt.AddSeconds(1);
        var existing = await _db.TripVideoSegments
            .Where(v => v.TripId == tripId && v.StartedAt >= from && v.StartedAt <= to)
            .Select(v => (int?)v.Id)
            .FirstOrDefaultAsync(ct);
        if (existing.HasValue) return new UploadTripVideoResult(true, "Đoạn video này đã được nhận trước đó.", existing);

        var relativePath = $"trip-{tripId}/{startedAt:yyyyMMdd-HHmmss}-{Guid.NewGuid().ToString("N")[..8]}{ext.ToLowerInvariant()}";
        var (size, sha) = await _storage.SaveAsync(relativePath, content, ct);
        if (size == 0)
        {
            _storage.Delete(relativePath);
            return new UploadTripVideoResult(false, "File video rỗng.", null);
        }

        var segment = new TripVideoSegment
        {
            TripId = tripId,
            StoragePath = relativePath,
            ContentType = mappedType,
            StartedAt = startedAt,
            EndedAt = endedAt,
            SizeBytes = size,
            Sha256 = sha,
        };
        _db.TripVideoSegments.Add(segment);
        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch
        {
            _storage.Delete(relativePath); // không để file "mồ côi" khi ghi DB lỗi
            throw;
        }

        return new UploadTripVideoResult(true, null, segment.Id);
    }

    public async Task<List<TripVideoSegmentDto>> GetSegmentsAsync(int tripId)
    {
        var list = await _db.TripVideoSegments
            .Where(v => v.TripId == tripId)
            .OrderBy(v => v.StartedAt)
            .ToListAsync();

        return list.Select(v => new TripVideoSegmentDto(
            v.Id, v.TripId,
            DateTime.SpecifyKind(v.StartedAt, DateTimeKind.Utc),
            DateTime.SpecifyKind(v.EndedAt, DateTimeKind.Utc),
            Math.Round((v.EndedAt - v.StartedAt).TotalSeconds, 1),
            v.SizeBytes, v.ContentType, v.Sha256)).ToList();
    }

    public async Task<TripVideoFile?> GetFileAsync(int segmentId)
    {
        var v = await _db.TripVideoSegments.FirstOrDefaultAsync(x => x.Id == segmentId);
        if (v is null || !_storage.Exists(v.StoragePath)) return null;
        return new TripVideoFile(_storage.GetFullPath(v.StoragePath), v.ContentType);
    }

    public async Task<VerifyTripVideoResult?> VerifyAsync(int segmentId, CancellationToken ct = default)
    {
        var v = await _db.TripVideoSegments.FirstOrDefaultAsync(x => x.Id == segmentId, ct);
        if (v is null) return null;

        var actual = await _storage.ComputeSha256Async(v.StoragePath, ct);
        if (actual is null)
            return new VerifyTripVideoResult(v.Id, false, false, v.Sha256, null, "Không còn file video trên kho lưu trữ.");

        var intact = string.Equals(actual, v.Sha256, StringComparison.OrdinalIgnoreCase);
        return new VerifyTripVideoResult(v.Id, true, intact, v.Sha256, actual,
            intact ? "Toàn vẹn — video trùng khớp với bản đã nhận từ tài xế."
                   : "CẢNH BÁO — nội dung file đã bị thay đổi sau khi nhận!");
    }

    /// <summary>Chuẩn hóa về UTC: Local -> đổi múi giờ; Unspecified -> coi là UTC
    /// (app luôn gửi chuỗi ISO có hậu tố Z).</summary>
    private static DateTime ToUtc(DateTime t) => t.Kind switch
    {
        DateTimeKind.Local => t.ToUniversalTime(),
        DateTimeKind.Unspecified => DateTime.SpecifyKind(t, DateTimeKind.Utc),
        _ => t,
    };
}