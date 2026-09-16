namespace TruckingSystem.Application.Trips;

/// <summary>
/// Use case liên quan tới Trip. Đây là nơi chứa TOÀN BỘ business logic
/// (trước đây từng nằm sai chỗ, trực tiếp trong TripsController).
/// Controller giờ chỉ còn 2-3 dòng: nhận request -> gọi service -> trả response.
/// </summary>
public interface ITripService
{
    Task<ServiceResult> PingAsync(int tripId, GpsPingRequest request);
    Task<ServiceResult> ConfirmArrivalAsync(int tripId, ConfirmArrivalRequest request);
}
