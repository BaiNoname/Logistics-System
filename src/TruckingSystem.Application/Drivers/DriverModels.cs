namespace TruckingSystem.Application.Drivers;

/// <summary>Danh sách Tài xế riêng biệt (KHÔNG chung với Employee) — mục đích
/// chính là hiển thị Availability khớp đúng với logic thật dùng khi Chia xe
/// (VehicleAllocationService), giúp Dispatcher đối chiếu nhanh ai đang rảnh/
/// bận/chưa đăng ký ca trước khi bấm Chia xe.
///
/// Availability — 3 giá trị:
///   "Đang làm"        — đang có Trip Planned/InProgress
///   "Sẵn sàng"         — không đang chạy chuyến, CÓ đăng ký ca hôm nay
///   "Không sẵn sàng"   — chưa đăng ký ca làm việc hôm nay</summary>
public record DriverListItemDto(
    int EmployeeId,
    string Code,
    string FullName,
    string? Phone,
    string LicenseNumber,
    string LicenseClass,
    DateTime LicenseExpiryDate,
    int YearsOfExperience,
    int? WarehouseId,
    string? WarehouseName,
    string Availability
);