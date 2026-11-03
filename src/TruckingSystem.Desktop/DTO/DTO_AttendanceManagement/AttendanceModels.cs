namespace TruckingSystem.Desktop.DTO_AttendanceManagement;

// Khớp TruckingSystem.Application.Attendance — mốc giờ là GIỜ VIỆT NAM.

public record AttendanceResult(bool Success, string? Message);

public record AttendanceSiteDto(int WarehouseId, string Name, string? Address, double? Lat, double? Lng,
    int RadiusMeters, int MaxAccuracyMeters, bool IsReady)
{
    public override string ToString() => IsReady ? Name : Name + " (chưa có tọa độ)";
}

public record UpdateAttendanceSiteRequest(int RadiusMeters, int MaxAccuracyMeters);

/// <summary>Success/Message chỉ có giá trị khi API từ chối (400 {success:false,message}).</summary>
public record AttendanceQrDto(int WarehouseId, string? WarehouseName, DateTime WorkDate, string? QrContent,
    bool? Success = null, string? Message = null);

public record AttendanceRecordDto(int Id, int EmployeeId, string EmployeeCode, string EmployeeName, DateTime WorkDate,
    DateTime? CheckInAt, string? CheckInSite, DateTime? CheckOutAt, string? CheckOutSite,
    DateTime? AdjustedCheckInAt, DateTime? AdjustedCheckOutAt, string StatusText);

public record AttendanceScanLogDto(int Id, string EmployeeCode, string EmployeeName, DateTime ScannedAt, string? SiteName,
    string Action, string Result, double? DistanceMeters, double? AccuracyMeters, string? RejectReason);

public record AdjustmentRequestDto(int Id, int EmployeeId, string EmployeeCode, string EmployeeName, DateTime WorkDate,
    DateTime? CurrentCheckInAt, DateTime? CurrentCheckOutAt, DateTime? RequestedCheckInAt, DateTime? RequestedCheckOutAt,
    string Reason, string ReasonText, string Note, string Status, string StatusText,
    DateTime CreatedAt, string? ReviewedBy, DateTime? ReviewedAt, string? ReviewNote);

public record ReviewAdjustmentRequest(bool Approve, string? Note);
