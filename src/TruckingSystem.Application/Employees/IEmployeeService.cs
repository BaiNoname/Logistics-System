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
}