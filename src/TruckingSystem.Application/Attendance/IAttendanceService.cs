namespace TruckingSystem.Application.Attendance;

/// <summary>Chấm công QR + GPS. Các hàm "My*"/Scan nhận userId từ JWT — nhân viên được xác định qua
/// Employee.UserId ở backend, KHÔNG nhận EmployeeId do thiết bị gửi lên.</summary>
public interface IAttendanceService
{
    // ---- Mobile (chính nhân viên) ----
    Task<MobileTodayDto?> GetMyTodayAsync(string userId);
    Task<AttendanceScanResponse> ScanAsync(string userId, AttendanceScanRequest request);
    Task<List<AttendanceDayDto>?> GetMyHistoryAsync(string userId, DateTime from, DateTime to);
    Task<List<AdjustmentRequestDto>?> GetMyAdjustmentsAsync(string userId);
    Task<AttendanceResult> CreateAdjustmentAsync(string userId, CreateAdjustmentRequest request);
    List<AdjustmentReasonDto> GetAdjustmentReasons();

    // ---- Desktop (quản lý) ----
    Task<List<AttendanceSiteDto>> GetSitesAsync();
    Task<AttendanceResult> UpdateSiteAsync(int warehouseId, UpdateAttendanceSiteRequest request);
    Task<(AttendanceResult Result, AttendanceQrDto? Qr)> GetOrCreateQrAsync(int warehouseId, DateTime workDate, string createdBy);
    Task<List<AttendanceRecordDto>> GetRecordsAsync(DateTime from, DateTime to);
    Task<List<AttendanceScanLogDto>> GetScansAsync(DateTime workDate);
    Task<List<AdjustmentRequestDto>> GetAdjustmentsAsync(string? status);
    Task<AttendanceResult> ReviewAdjustmentAsync(int id, ReviewAdjustmentRequest request, string reviewer);
}
