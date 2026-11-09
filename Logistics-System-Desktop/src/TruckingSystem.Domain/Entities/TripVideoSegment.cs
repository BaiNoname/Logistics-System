using TruckingSystem.Domain.Common;

namespace TruckingSystem.Domain.Entities;

/// <summary>1 đoạn video hành trình (camera hành trình) của chuyến — app tài
/// xế quay theo từng đoạn ~1 phút và gửi lên từng đoạn một.
///
/// File video KHÔNG nằm trong database (rất nặng: ~0,5 GB/giờ ở 480p) — chỉ
/// lưu ĐƯỜNG DẪN tới file trên kho lưu trữ (ổ đĩa server) + thông tin mô tả.
/// StartedAt/EndedAt (UTC, lấy theo đồng hồ của CÙNG điện thoại ghi GPS) dùng
/// để khớp video với điểm GPS: bấm 1 điểm trên đường đi -> phát đúng giây đó.
/// Sha256 tính lúc nhận file — đối chiếu lại về sau để chứng minh video không
/// bị chỉnh sửa (minh bạch khi có tranh chấp).</summary>
public class TripVideoSegment : BaseEntity
{
    public int TripId { get; set; }
    public Trip Trip { get; set; } = null!;

    /// <summary>Đường dẫn TƯƠNG ĐỐI trong kho lưu trữ, VD "trip-12/20261006-083015-ab12cd34.mp4".</summary>
    public string StoragePath { get; set; } = string.Empty;
    public string ContentType { get; set; } = "video/mp4";

    public DateTime StartedAt { get; set; }
    public DateTime EndedAt { get; set; }

    public long SizeBytes { get; set; }
    public string Sha256 { get; set; } = string.Empty;
}
