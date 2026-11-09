namespace TruckingSystem.Desktop.DTO_TripManagement;

/// <summary>1 đoạn video hành trình (~1 phút) của chuyến.</summary>
public class TripVideoItem
{
    public int Id { get; set; }
    public int TripId { get; set; }
    public DateTime StartedAt { get; set; }   // UTC
    public DateTime EndedAt { get; set; }     // UTC
    public double DurationSeconds { get; set; }
    public long SizeBytes { get; set; }
    public string ContentType { get; set; } = "video/mp4";
    public string Sha256 { get; set; } = "";
    /// <summary>Link xem có chữ ký, hạn 6 giờ (đường dẫn tương đối, ghép với địa chỉ API).</summary>
    public string StreamUrl { get; set; } = "";
}

public class TripVideoVerifyItem
{
    public int SegmentId { get; set; }
    public bool FileExists { get; set; }
    public bool Intact { get; set; }
    public string StoredSha256 { get; set; } = "";
    public string? ActualSha256 { get; set; }
    public string Message { get; set; } = "";
}
