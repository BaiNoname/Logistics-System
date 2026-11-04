using TruckingSystem.Domain.Common;
using TruckingSystem.Domain.Enums;

namespace TruckingSystem.Domain.Entities;

/// <summary>Mã QR chấm công của 1 kho trong 1 ngày (giờ VN). Mọi nhân viên tại kho quét chung mã này;
/// Token ngẫu nhiên, không đoán được — điện thoại chỉ gửi Token, backend tự tra ra kho và ngày.</summary>
public class AttendanceQrCode : BaseEntity
{
    public int WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; } = null!;
    public DateTime WorkDate { get; set; }
    public string Token { get; set; } = "";
    public string CreatedBy { get; set; } = "";
}

/// <summary>Nhật ký quét GỐC — chỉ thêm, không sửa/xóa (kể cả lượt bị từ chối) để đối chiếu về sau.</summary>
public class AttendanceScan : BaseEntity
{
    public int EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;
    public int? QrCodeId { get; set; }
    public AttendanceQrCode? QrCode { get; set; }
    public int? WarehouseId { get; set; }
    public Warehouse? Warehouse { get; set; }
    public DateTime WorkDate { get; set; }
    /// <summary>Thời điểm do MÁY CHỦ ghi nhận (UTC) — điện thoại không khai báo giờ.</summary>
    public DateTime ScannedAt { get; set; }
    public double? Lat { get; set; }
    public double? Lng { get; set; }
    public double? AccuracyMeters { get; set; }
    public double? DistanceMeters { get; set; }
    public bool IsMockLocation { get; set; }
    public AttendanceScanAction Action { get; set; }
    public AttendanceScanResult Result { get; set; }
    public string? RejectReason { get; set; }
}

/// <summary>Bản ghi công 1 ngày của 1 nhân viên (1 ca/ngày). Giờ quét gốc (CheckInAt/CheckOutAt) giữ
/// nguyên; điều chỉnh được duyệt ghi vào Adjusted* — giờ hiệu lực = Adjusted ?? gốc.
/// Công hưởng lương được quy đổi từ giờ hiệu lực theo AttendanceRules khi tạo bảng lương tháng; không nhập lại công tại module Lương.</summary>
public class AttendanceRecord : BaseEntity
{
    public int EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;
    public DateTime WorkDate { get; set; }
    public DateTime? CheckInAt { get; set; }
    public int? CheckInWarehouseId { get; set; }
    public Warehouse? CheckInWarehouse { get; set; }
    public DateTime? CheckOutAt { get; set; }
    public int? CheckOutWarehouseId { get; set; }
    public Warehouse? CheckOutWarehouse { get; set; }
    public DateTime? AdjustedCheckInAt { get; set; }
    public DateTime? AdjustedCheckOutAt { get; set; }
    public int? AdjustmentRequestId { get; set; }

    public DateTime? EffectiveCheckInAt => AdjustedCheckInAt ?? CheckInAt;
    public DateTime? EffectiveCheckOutAt => AdjustedCheckOutAt ?? CheckOutAt;
}

/// <summary>Yêu cầu điều chỉnh công do nhân viên gửi từ Mobile, Admin/Dispatcher duyệt trên Desktop.</summary>
public class AttendanceAdjustmentRequest : BaseEntity
{
    public int EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;
    public DateTime WorkDate { get; set; }
    /// <summary>Giờ đề xuất (UTC); null = không đề xuất sửa lượt đó.</summary>
    public DateTime? RequestedCheckInAt { get; set; }
    public DateTime? RequestedCheckOutAt { get; set; }
    public AttendanceAdjustmentReason Reason { get; set; }
    public string Note { get; set; } = "";
    public AttendanceAdjustmentStatus Status { get; set; } = AttendanceAdjustmentStatus.Pending;
    public string? ReviewedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewNote { get; set; }
}
