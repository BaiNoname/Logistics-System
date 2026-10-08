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
/// <summary>1 NGÀY LÀM VIỆC của tài xế (bảng DriverSchedules). Không còn chia ca
/// Sáng/Chiều/Tối — đăng ký theo NGÀY (luôn lưu là FullDay): Chia xe chỉ cần biết
/// tài xế có đi làm ngày đó không; giới hạn giờ lái đã tính trong ước tính số ngày chạy.</summary>
public record DriverWorkDayDto(int Id, DateOnly Date, bool HasOldShiftType);

/// <summary>Đăng ký 1 KHOẢNG ngày (từ – đến), tính cả Thứ 7, Chủ nhật — tài xế chạy
/// theo chuyến, không nghỉ cố định cuối tuần như nhân viên văn phòng. Ngày nào nghỉ
/// thì xóa riêng ngày đó.</summary>
public record RegisterWorkDaysRequest(DateOnly FromDate, DateOnly ToDate);

public record DriverWorkDayResult(bool Success, string? Message, int Added = 0, int Skipped = 0);
