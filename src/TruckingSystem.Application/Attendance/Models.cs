namespace TruckingSystem.Application.Attendance;

// Mọi mốc giờ trong DTO là GIỜ VIỆT NAM (không kèm múi giờ); DB lưu UTC.

public record AttendanceResult(bool Success, string? Message);

public record MobileProfileDto(int EmployeeId, string Code, string FullName, string? DepartmentName, string? PositionName, bool IsDriver);

/// <summary>Status: NotCheckedIn | CheckedIn | CheckedOut | MissingCheckOut.</summary>
public record AttendanceDayDto(
    DateTime WorkDate,
    DateTime? CheckInAt, string? CheckInSite,
    DateTime? CheckOutAt, string? CheckOutSite,
    bool IsAdjusted, string Status, string StatusText, string? PendingAdjustmentText);

public record MobileTodayDto(MobileProfileDto Employee, DateTime ServerTime, AttendanceDayDto Today);

public record AttendanceScanRequest(
    string QrContent, double Latitude, double Longitude, double AccuracyMeters,
    bool IsMockLocation, bool ConfirmCheckOut);

/// <summary>Code: CheckedIn | CheckedOut | ConfirmCheckOut | InvalidQr | QrExpired | SiteNotReady |
/// LocationRejected | TooSoon | AlreadyCheckedOut | NotEmployee | Conflict.</summary>
public record AttendanceScanResponse(bool Success, string Code, string Message, string? SiteName, AttendanceDayDto? Today);

public record CreateAdjustmentRequest(
    DateTime WorkDate, DateTime? RequestedCheckInAt, DateTime? RequestedCheckOutAt, string Reason, string? Note);

public record ReviewAdjustmentRequest(bool Approve, string? Note);

public record AdjustmentRequestDto(
    int Id, int EmployeeId, string EmployeeCode, string EmployeeName, DateTime WorkDate,
    DateTime? CurrentCheckInAt, DateTime? CurrentCheckOutAt,
    DateTime? RequestedCheckInAt, DateTime? RequestedCheckOutAt,
    string Reason, string ReasonText, string Note, string Status, string StatusText,
    DateTime CreatedAt, string? ReviewedBy, DateTime? ReviewedAt, string? ReviewNote);

public record AdjustmentReasonDto(string Value, string Text);

public record AttendanceSiteDto(
    int WarehouseId, string Name, string? Address, double? Lat, double? Lng,
    int RadiusMeters, int MaxAccuracyMeters, bool IsReady);

public record UpdateAttendanceSiteRequest(int RadiusMeters, int MaxAccuracyMeters);

public record AttendanceQrDto(int WarehouseId, string WarehouseName, DateTime WorkDate, string QrContent);

public record AttendanceRecordDto(
    int Id, int EmployeeId, string EmployeeCode, string EmployeeName, DateTime WorkDate,
    DateTime? CheckInAt, string? CheckInSite, DateTime? CheckOutAt, string? CheckOutSite,
    DateTime? AdjustedCheckInAt, DateTime? AdjustedCheckOutAt, string StatusText);

public record AttendanceScanLogDto(
    int Id, string EmployeeCode, string EmployeeName, DateTime ScannedAt, string? SiteName,
    string Action, string Result, double? DistanceMeters, double? AccuracyMeters, string? RejectReason);
