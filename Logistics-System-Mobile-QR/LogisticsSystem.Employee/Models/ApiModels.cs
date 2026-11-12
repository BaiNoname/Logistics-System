namespace LogisticsSystem.Employee.Models;

// DTO khớp với TruckingSystem.API (Application/Auth/Models.cs, Application/Attendance/Models.cs).
// Mọi mốc giờ là GIỜ VIỆT NAM không kèm múi giờ, nên dùng DateTime (không dùng DateTimeOffset).

// ---------- Auth ----------

public sealed class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public sealed class LoginResult
{
    public bool Success { get; set; }
    public string? Token { get; set; }
    public string? Message { get; set; }
    public List<string>? Roles { get; set; }
}

// ---------- Mobile: hôm nay / lịch sử ----------

public sealed class MobileProfileDto
{
    public int EmployeeId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? DepartmentName { get; set; }
    public string? PositionName { get; set; }
    public bool IsDriver { get; set; }
}

/// <summary>Status: NotCheckedIn | CheckedIn | CheckedOut | MissingCheckOut.</summary>
public sealed class AttendanceDayDto
{
    public DateTime WorkDate { get; set; }
    public DateTime? CheckInAt { get; set; }
    public string? CheckInSite { get; set; }
    public DateTime? CheckOutAt { get; set; }
    public string? CheckOutSite { get; set; }
    public bool IsAdjusted { get; set; }
    public string Status { get; set; } = string.Empty;
    public string StatusText { get; set; } = string.Empty;
    public string? PendingAdjustmentText { get; set; }
}

public sealed class MobileTodayDto
{
    public MobileProfileDto Employee { get; set; } = new();
    public DateTime ServerTime { get; set; }
    public AttendanceDayDto Today { get; set; } = new();
}

// ---------- Mobile: quét QR ----------

public sealed class AttendanceScanRequest
{
    public string QrContent { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double AccuracyMeters { get; set; }
    public bool IsMockLocation { get; set; }
    /// <summary>false ở lần quét đầu; server trả Code "ConfirmCheckOut" khi cần xác nhận, khi đó gửi lại true.</summary>
    public bool ConfirmCheckOut { get; set; }
}

/// <summary>Code: CheckedIn | CheckedOut | ConfirmCheckOut | InvalidQr | QrExpired | SiteNotReady |
/// LocationRejected | TooSoon | AlreadyCheckedOut | NotEmployee | Conflict.</summary>
public sealed class AttendanceScanResponse
{
    public bool Success { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? SiteName { get; set; }
    public AttendanceDayDto? Today { get; set; }
}

// ---------- Mobile: điều chỉnh ----------

public sealed class CreateAdjustmentRequest
{
    public DateTime WorkDate { get; set; }
    public DateTime? RequestedCheckInAt { get; set; }
    public DateTime? RequestedCheckOutAt { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? Note { get; set; }
}

public sealed class AttendanceResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }
}

public sealed class AdjustmentReasonDto
{
    public string Value { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
}

public sealed class AdjustmentRequestDto
{
    public int Id { get; set; }
    public DateTime WorkDate { get; set; }
    public DateTime? CurrentCheckInAt { get; set; }
    public DateTime? CurrentCheckOutAt { get; set; }
    public DateTime? RequestedCheckInAt { get; set; }
    public DateTime? RequestedCheckOutAt { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string ReasonText { get; set; } = string.Empty;
    public string? Note { get; set; }
    public string Status { get; set; } = string.Empty;
    public string StatusText { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewNote { get; set; }
}
