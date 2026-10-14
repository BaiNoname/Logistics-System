namespace TruckingSystem.Application.Drivers;

public interface IDriverService
{
    /// <summary>Danh sách tài xế = nhân viên Đang làm có chức danh Tài xế (+ người đã có hồ sơ
    /// tài xế cần lưu ý), kèm trạng thái hồ sơ và Availability tính theo đúng logic Chia xe
    /// — xem chi tiết trong DriverService.GetAllAsync().</summary>
    Task<List<DriverListItemDto>> GetAllAsync();

    // ---- Ngày làm việc (bảng DriverSchedules) — đăng ký theo NGÀY, không chia ca ----
    Task<List<DriverWorkDayDto>> GetWorkDaysAsync(int employeeId);
    Task<DriverWorkDayResult> RegisterWorkDaysAsync(int employeeId, RegisterWorkDaysRequest request);
    Task<DriverWorkDayResult> DeleteWorkDayAsync(int workDayId);
}