using Microsoft.EntityFrameworkCore;
using TruckingSystem.Application.Common.Interfaces;
using TruckingSystem.Application.TripVideos;
using TruckingSystem.Domain.Entities;
using TruckingSystem.Domain.Enums;

namespace TruckingSystem.Application.TripPhotos;

/// <summary>Kiểm hàng + ảnh hàng hóa khi tài xế xác nhận chặng:
///  - Lấy hàng: đối tác PHẢI giao ĐỦ số lượng trên đơn — tài xế tick từng dòng hàng
///    "đã nhận đủ" + chụp ảnh hàng trên xe. Thiếu hàng thì KHÔNG xác nhận được (gọi
///    Điều phối) — hệ thống không xử lý giao thiếu (xem phần hạn chế).
///  - Giao hàng / Trả hàng: tick "đã giao đủ" + chụp ảnh lúc bàn giao (bằng chứng giao hàng).
/// Ảnh dùng chung kho lưu trữ với video hành trình (IVideoStorage), thư mục "photos/".</summary>
public class TripPhotoService : ITripPhotoService
{
    public const int MinPhotos = 1;
    public const int MaxPhotos = 10;
    private const long MaxBytes = 15L * 1024 * 1024; // 15 MB/ảnh (app nén ~0,5 — thường 0,3–2 MB)

    private static readonly Dictionary<string, string> AllowedTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        [".jpg"] = "image/jpeg",
        [".jpeg"] = "image/jpeg",
        [".png"] = "image/png",
        [".webp"] = "image/webp",
    };

    private readonly IApplicationDbContext _db;
    private readonly IVideoStorage _storage;

    public TripPhotoService(IApplicationDbContext db, IVideoStorage storage)
    {
        _db = db;
        _storage = storage;
    }

    public async Task<TripCargoChecklistDto?> GetChecklistAsync(int tripId)
    {
        var trip = await _db.Trips
            .Where(t => t.Id == tripId)
            .Select(t => new { t.Id, t.LegType, t.Status, OrderCode = t.TransportOrder.Code, t.ConfirmRejectedAt })
            .FirstOrDefaultAsync();
        if (trip is null) return null;

        var lines = await _db.TripLines
            .Where(l => l.TripId == tripId)
            .OrderBy(l => l.Id)
            .Select(l => new
            {
                l.CargoLineId, l.QtyPlanned, l.CargoLine.CargoType, l.CargoLine.QtyTotal, l.CargoLine.PackageCount,
                l.CargoLine.WeightKg, l.CargoLine.VolumeM3, l.CargoLine.IsFragile, l.CargoLine.IsHazmat,
            })
            .ToListAsync();

        // Số kiện / kg / m³ của PHẦN hàng chuyến này chở (dòng hàng có thể chia nhiều xe).
        var dtos = lines.Select(l =>
        {
            var ratio = l.QtyTotal > 0 ? l.QtyPlanned / l.QtyTotal : 0;
            return new TripCargoLineDto(l.CargoLineId, l.CargoType, l.QtyPlanned,
                (int)Math.Round(l.PackageCount * ratio, MidpointRounding.AwayFromZero),
                Math.Round(l.WeightKg * ratio, 2), Math.Round(l.VolumeM3 * ratio, 4), l.IsFragile, l.IsHazmat);
        }).ToList();

        // Chỉ đếm ảnh của LẦN xác nhận hiện tại (sau lần bị Vận hành từ chối gần nhất).
        var since = trip.ConfirmRejectedAt ?? DateTime.MinValue;
        var photoCount = await _db.TripCargoPhotos.CountAsync(p => p.TripId == tripId && p.CreatedAt > since);
        return new TripCargoChecklistDto(trip.Id, trip.LegType.ToString(), trip.Status.ToString(), trip.OrderCode,
            dtos, photoCount, MinPhotos, MaxPhotos);
    }

    public async Task<UploadTripPhotoResult> UploadAsync(int tripId, Stream content, string? fileName, string? contentType,
        DateTime? takenAt, CancellationToken ct = default)
    {
        var trip = await _db.Trips.Where(t => t.Id == tripId).Select(t => new { t.Status, t.ConfirmRejectedAt }).FirstOrDefaultAsync(ct);
        if (trip is null) return new UploadTripPhotoResult(false, "Không tìm thấy chuyến.", null, 0);
        // Ảnh hàng chụp lúc tài xế đang ở điểm lấy / giao — chuyến phải ĐANG CHẠY.
        if (trip.Status == TripStatus.Planned)
            return new UploadTripPhotoResult(false, "GPS chưa ghi nhận chuyến bắt đầu — chờ khoảng 30 giây rồi gửi lại ảnh.", null, 0);
        if (trip.Status != TripStatus.InProgress)
            return new UploadTripPhotoResult(false, "Chuyến không ở trạng thái Đang chạy — không nhận ảnh hàng.", null, 0);

        var since = trip.ConfirmRejectedAt ?? DateTime.MinValue;
        var count = await _db.TripCargoPhotos.CountAsync(p => p.TripId == tripId && p.CreatedAt > since, ct);
        if (count >= MaxPhotos)
            return new UploadTripPhotoResult(false, $"Mỗi chuyến tối đa {MaxPhotos} ảnh.", null, count);

        var ext = Path.GetExtension(fileName ?? "");
        if (string.IsNullOrEmpty(ext))
            ext = contentType?.ToLowerInvariant() switch { "image/png" => ".png", "image/webp" => ".webp", _ => ".jpg" };
        if (!AllowedTypes.TryGetValue(ext, out var mappedType))
            return new UploadTripPhotoResult(false, "Chỉ nhận ảnh JPG, PNG, WEBP.", null, count);

        var now = DateTime.UtcNow;
        var taken = takenAt.HasValue ? DateTime.SpecifyKind(takenAt.Value.ToUniversalTime(), DateTimeKind.Utc) : now;
        if (taken > now.AddMinutes(5) || taken < now.AddDays(-1)) taken = now; // giờ máy sai -> dùng giờ server

        var relativePath = $"photos/trip-{tripId}/{taken:yyyyMMdd-HHmmss}-{Guid.NewGuid().ToString("N")[..8]}{ext.ToLowerInvariant()}";
        var (size, sha) = await _storage.SaveAsync(relativePath, content, ct);
        if (size == 0 || size > MaxBytes)
        {
            _storage.Delete(relativePath);
            return new UploadTripPhotoResult(false, size == 0 ? "File ảnh rỗng." : "Ảnh quá lớn (tối đa 15 MB).", null, count);
        }

        var photo = new TripCargoPhoto
        {
            TripId = tripId,
            StoragePath = relativePath,
            ContentType = mappedType,
            SizeBytes = size,
            Sha256 = sha,
            TakenAt = DateTime.SpecifyKind(taken, DateTimeKind.Unspecified),
        };
        _db.TripCargoPhotos.Add(photo);
        await _db.SaveChangesAsync(ct);
        return new UploadTripPhotoResult(true, null, photo.Id, count + 1);
    }

    public async Task<List<TripPhotoDto>> GetPhotosAsync(int tripId)
    {
        var rows = await _db.TripCargoPhotos
            .Where(p => p.TripId == tripId)
            .OrderBy(p => p.TakenAt)
            .Select(p => new { p.Id, p.TripId, p.TakenAt, p.SizeBytes, p.ContentType })
            .ToListAsync();
        return rows.Select(p => new TripPhotoDto(p.Id, p.TripId, DateTime.SpecifyKind(p.TakenAt, DateTimeKind.Utc), p.SizeBytes, p.ContentType)).ToList();
    }

    public async Task<TripPhotoFile?> GetFileAsync(int photoId)
    {
        var photo = await _db.TripCargoPhotos.FindAsync(photoId);
        if (photo is null || !_storage.Exists(photo.StoragePath)) return null;
        return new TripPhotoFile(_storage.GetFullPath(photo.StoragePath), photo.ContentType);
    }
}
