namespace TruckingSystem.Application.TripVideos;

public interface ITripVideoService
{
    /// <summary>Nhận 1 đoạn video từ app tài xế. Gửi lại cùng 1 đoạn (do mạng
    /// chập chờn, app không nhận được phản hồi) không tạo bản trùng.</summary>
    Task<UploadTripVideoResult> UploadAsync(int tripId, Stream content, string originalFileName, string? contentType,
        DateTime startedAt, DateTime endedAt, CancellationToken ct = default);

    Task<List<TripVideoSegmentDto>> GetSegmentsAsync(int tripId);

    Task<TripVideoFile?> GetFileAsync(int segmentId);

    /// <summary>Đối chiếu SHA-256 hiện tại của file với mã đã lưu lúc nhận.</summary>
    Task<VerifyTripVideoResult?> VerifyAsync(int segmentId, CancellationToken ct = default);
}
