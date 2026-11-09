namespace TruckingSystem.Application.TripPhotos;

public interface ITripPhotoService
{
    /// <summary>Tên hàng + số lượng của chuyến (app tài xế hiện sẵn để kiểm đếm).</summary>
    Task<TripCargoChecklistDto?> GetChecklistAsync(int tripId);

    /// <summary>App tài xế gửi 1 ảnh hàng hóa (chuyến phải đang chạy).</summary>
    Task<UploadTripPhotoResult> UploadAsync(int tripId, Stream content, string? fileName, string? contentType, DateTime? takenAt, CancellationToken ct = default);

    Task<List<TripPhotoDto>> GetPhotosAsync(int tripId);

    Task<TripPhotoFile?> GetFileAsync(int photoId);
}
