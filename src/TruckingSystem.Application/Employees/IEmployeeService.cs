namespace TruckingSystem.Application.Employees;

public interface IEmployeeService
{
    Task<List<EmployeeListItemDto>> GetAllAsync();
    Task<EmployeeListItemDto> CreateAsync(CreateEmployeeRequest request);
    Task<List<LookupItemDto>> GetDepartmentsAsync();
    Task<List<LookupItemDto>> GetPositionsAsync();

    /// <summary>Tạo hồ sơ tài xế cho 1 nhân viên đã có sẵn — cần thiết để
    /// nhân viên đó xuất hiện trong danh sách tài xế khả dụng của Greedy.</summary>
    Task<EmployeeActionResult> CreateDriverProfileAsync(int employeeId, CreateDriverProfileRequest request);

    /// <summary>Lấy hồ sơ tài xế hiện tại của 1 nhân viên (để nạp sẵn khi Sửa) — null nếu chưa có.</summary>
    Task<DriverProfileDto?> GetDriverProfileAsync(int employeeId);

    /// <summary>Sửa hồ sơ tài xế đã có.</summary>
    Task<EmployeeActionResult> UpdateDriverProfileAsync(int employeeId, UpdateDriverProfileRequest request);

    /// <summary>Danh sách ca làm việc đã đăng ký của 1 tài xế.</summary>
    Task<List<DriverScheduleDto>> GetSchedulesAsync(int employeeId);

    /// <summary>Đăng ký 1 ca làm việc mới cho tài xế.</summary>
    Task<EmployeeActionResult> CreateScheduleAsync(int employeeId, CreateDriverScheduleRequest request);

    Task<EmployeeActionResult> DeleteScheduleAsync(int scheduleId);
}