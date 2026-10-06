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

public record UploadTripVideoResult(bool Success, string? Message, int? SegmentId);

public record TripVideoFile(string FullPath, string ContentType);

public record VerifyTripVideoResult(int SegmentId, bool FileExists, bool Intact, string StoredSha256, string? ActualSha256, string Message);
