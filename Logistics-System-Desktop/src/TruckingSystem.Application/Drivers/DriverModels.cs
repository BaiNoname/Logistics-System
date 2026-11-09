namespace TruckingSystem.Application.Drivers;

/// <summary>1 dòng màn Tài xế. Danh sách = nhân viên ĐANG LÀM có chức danh Tài xế
/// (xem DriverPositionRules), cộng thêm người đã có hồ sơ tài xế nhưng chức danh khác /
/// không còn đang làm mà vẫn còn chuyến chưa xong (hiện kèm Cảnh báo để Vận hành xử lý).
///
/// HasProfile = đã có DriverProfile (bằng lái, kho đóng quân) — chưa có thì các cột bằng
/// lái để trống và KHÔNG được Chia xe / đăng ký ngày làm cho tới khi tạo hồ sơ.
/// ProfileStatus: "Chưa có hồ sơ" / "Bằng lái hết hạn" / "Bằng lái sắp hết hạn" / "Đủ".
///
/// Availability — 3 giá trị (khớp điều kiện thật của Chia xe):
///   "Đang làm"        — đang có Trip Planned/InProgress/Chờ xác nhận
///   "Sẵn sàng"         — đang làm việc, có hồ sơ, bằng lái còn hạn, CÓ ngày làm hôm nay
///   "Không sẵn sàng"   — các trường hợp còn lại</summary>
public record DriverListItemDto(
    int EmployeeId,
    string Code,
    string FullName,
    string? Phone,
    string? PositionName,
    string EmployeeStatus,
    bool HasProfile,
    string? LicenseNumber,
    string? LicenseClass,
    DateTime? LicenseExpiryDate,
    DateTime? HealthCertExpiryDate,
    int? YearsOfExperience,
    int? WarehouseId,
    string? WarehouseName,
    string ProfileStatus,
    string? Warning,
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
