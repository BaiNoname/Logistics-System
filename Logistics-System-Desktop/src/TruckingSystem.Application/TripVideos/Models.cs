namespace TruckingSystem.Application.TripVideos;

public record TripVideoSegmentDto(
    int Id,
    int TripId,
    DateTime StartedAt,
    DateTime EndedAt,
    double DurationSeconds,
    long SizeBytes,
    string ContentType,
    string Sha256);

/// <summary>Retryable = true: chưa nhận được LÚC NÀY nhưng sẽ nhận sau (VD chuyến chưa
/// được ghi nhận bắt đầu vì GPS chưa gửi được) — API trả 409 để app GIỮ file và gửi lại,
/// không xóa như lỗi 400.</summary>
public record UploadTripVideoResult(bool Success, string? Message, int? SegmentId, bool Retryable = false);

public record TripVideoFile(string FullPath, string ContentType);

public record VerifyTripVideoResult(int SegmentId, bool FileExists, bool Intact, string StoredSha256, string? ActualSha256, string Message);
