using TruckingSystem.Domain.Common;

namespace TruckingSystem.Domain.Entities;

/// <summary>
/// Mở rộng riêng cho nhân viên là tài xế — 1-1 với Employee.
/// </summary>
public class DriverProfile : BaseEntity
{
    public int EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;

    public string LicenseNumber { get; set; } = string.Empty;
    public string LicenseClass { get; set; } = string.Empty; // B2, C, FC...
    public DateTime LicenseExpiryDate { get; set; }
    public DateTime? HealthCertExpiryDate { get; set; }
    public int YearsOfExperience { get; set; }

    // Calendar ID của tài xế — NHẬP TAY bởi nhân viên/kỹ thuật (không qua
    // OAuth), giống hệt cơ chế của Customer.GoogleCalendarId. Tài xế tự share
    // lịch cá nhân cho 1 email Service Account cố định, gửi lại Calendar ID
    // này để nhập vào hệ thống — làm 1 lần duy nhất, không cần đăng nhập OAuth.
    public string? GoogleCalendarId { get; set; }

    /// <summary>Kho/chi nhánh mà tài xế này ĐÓNG QUÂN — dùng để Greedy CHỈ
    /// chọn tài xế đang ở ĐÚNG kho của đơn hàng, cùng logic với
    /// Vehicle.WarehouseId (thường xe và tài xế ở cùng 1 kho). NULL = tài xế
    /// CHƯA gán kho cố định, vẫn coi là dùng được cho MỌI đơn (áp dụng mềm).</summary>
    public int? WarehouseId { get; set; }
    public Warehouse? Warehouse { get; set; }
}
