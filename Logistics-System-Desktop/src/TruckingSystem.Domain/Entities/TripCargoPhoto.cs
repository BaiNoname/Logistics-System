using TruckingSystem.Domain.Common;

namespace TruckingSystem.Domain.Entities;

/// <summary>Ảnh chụp HÀNG HÓA do tài xế chụp lúc xác nhận chặng (lấy hàng tại đối tác,
/// giao hàng cho khách, trả hàng) — làm bằng chứng hàng đủ, đúng tình trạng.
/// File ảnh KHÔNG nằm trong database (dùng chung kho lưu trữ với video hành trình),
/// chỉ lưu đường dẫn + mã SHA-256 để chứng minh ảnh không bị sửa.</summary>
public class TripCargoPhoto : BaseEntity
{
    public int TripId { get; set; }
    public Trip Trip { get; set; } = null!;

    /// <summary>Đường dẫn TƯƠNG ĐỐI trong kho lưu trữ, VD "photos/trip-12/20261009-153015-ab12cd34.jpg".</summary>
    public string StoragePath { get; set; } = string.Empty;
    public string ContentType { get; set; } = "image/jpeg";
    public long SizeBytes { get; set; }
    public string Sha256 { get; set; } = string.Empty;

    /// <summary>Lúc chụp (UTC, theo đồng hồ điện thoại).</summary>
    public DateTime TakenAt { get; set; }
}
