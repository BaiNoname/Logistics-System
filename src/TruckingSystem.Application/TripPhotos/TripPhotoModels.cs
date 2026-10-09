namespace TruckingSystem.Application.TripPhotos;

/// <summary>1 dòng hàng của chuyến — app tài xế hiện SẴN tên hàng + số lượng để tài xế
/// chỉ việc kiểm đếm và tick xác nhận (không nhập tay, tránh gõ sai).</summary>
public record TripCargoLineDto(int CargoLineId, string CargoType, decimal Qty, int PackagesApprox,
    decimal WeightKg, decimal VolumeM3, bool IsFragile, bool IsHazmat);

/// <summary>Danh sách kiểm hàng của 1 chuyến cho app tài xế.</summary>
public record TripCargoChecklistDto(int TripId, string LegType, string Status, string OrderCode,
    List<TripCargoLineDto> Lines, int PhotoCount, int MinPhotos, int MaxPhotos);

public record TripPhotoDto(int Id, int TripId, DateTime TakenAt, long SizeBytes, string ContentType);

public record UploadTripPhotoResult(bool Success, string? Message, int? PhotoId, int PhotoCount);

public record TripPhotoFile(string FullPath, string ContentType);
