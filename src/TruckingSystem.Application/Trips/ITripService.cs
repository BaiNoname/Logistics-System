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

    /// <summary>Toàn bộ điểm GPS thật đã ghi nhận của 1 chuyến — dùng để vẽ
    /// đường đi thực tế trên bản đồ, sắp theo thời gian.</summary>
    Task<List<WaypointDto>> GetWaypointsAsync(int tripId);

    /// <summary>Tọa độ đầu-cuối ĐÚNG theo chặng của 1 Trip cụ thể — dùng cho
    /// trang điều hướng công khai (navigate.html) app tài xế mở qua WebView,
    /// không cần đăng nhập.</summary>
    Task<TripRouteDto?> GetTripRouteAsync(int tripId);

    /// <summary>Tính chi phí xăng ƯỚC TÍNH từ khoảng cách GPS THẬT (Waypoints)
    /// x định mức tiêu hao xe x giá xăng mới nhất — so sánh với chi phí THỰC
    /// TẾ đã ghi (FuelLog) để đo lường chênh lệch.</summary>
    Task<FuelEstimateDto> GetFuelEstimateAsync(int tripId);

    /// <summary>Danh sách sự cố của 1 chuyến.</summary>
    Task<List<IncidentReportDto>> GetIncidentsAsync(int tripId);

    /// <summary>Ghi nhận sự cố mới xảy ra trong chuyến.</summary>
    Task<IncidentReportDto> CreateIncidentAsync(int tripId, CreateIncidentReportRequest request);

    Task<TripActionResult> DeleteIncidentAsync(int incidentId);

    /// <summary>Đồng bộ lịch chuyến này lên Google Calendar của tài xế và khách
    /// hàng (nếu đã khai báo GoogleCalendarId) — kèm link theo dõi công khai
    /// trong mô tả sự kiện. Gọi lại nhiều lần chỉ CẬP NHẬT, không tạo trùng.</summary>
    Task<CalendarSyncResultDto> SyncCalendarAsync(int tripId, string publicTrackingBaseUrl);

    /// <summary>Danh sách chi phí khác (ngoài xăng) đã ghi cho 1 chuyến.</summary>
    Task<List<TripExpenseDto>> GetExpensesAsync(int tripId);

    Task<TripExpenseDto> CreateExpenseAsync(int tripId, CreateTripExpenseRequest request);

    Task<TripActionResult> DeleteExpenseAsync(int expenseId);

    /// <summary>Tính khoảng cách/thời gian ước tính từ VietMap cho ĐÚNG chặng
    /// của chuyến này (Lấy hàng: round-trip Kho→Đối tác→Kho; Giao hàng: 1
    /// chiều Kho→Điểm giao) — LƯU LẠI vào Trip để dùng cho thống kê sau này.</summary>
    Task<TripRouteEstimateDto> CalculateAndSaveRouteEstimateAsync(int tripId);
}