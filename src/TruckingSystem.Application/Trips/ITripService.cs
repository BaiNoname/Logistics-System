namespace TruckingSystem.Application.Trips;

public interface ITripService
{
    // ---- Quản lý Trip (Desktop, Dispatcher) ----
    Task<List<TripListItemDto>> GetAllAsync();

    /// <summary>Planned -> InProgress. Thao tác THỦ CÔNG do Dispatcher xác nhận
    /// (chưa tích hợp GPS thật để tự động chuyển).</summary>
    Task<TripActionResult> StartAsync(int id);

    /// <summary>InProgress -> Completed. Thao tác thủ công tương tự — ÁP DỤNG
    /// ĐÚNG logic phân biệt theo LegType (xem TripService.HandleArrivalAsync).</summary>
    Task<TripActionResult> CompleteAsync(int id);

    /// <summary>Ghi nhận 1 lần đổ xăng thực tế — tạo FuelLog, cộng dồn vào Trip.ActualFuelCost.</summary>
    Task<TripActionResult> RecordFuelAsync(int id, RecordFuelRequest request);

    // ---- App tài xế (đã có từ trước, giữ nguyên chữ ký) ----
    Task<ServiceResult> PingAsync(int tripId, GpsPingRequest request);
    Task<ServiceResult> ConfirmArrivalAsync(int tripId, ConfirmArrivalRequest request);
}