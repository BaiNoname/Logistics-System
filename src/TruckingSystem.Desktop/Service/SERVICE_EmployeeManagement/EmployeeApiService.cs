using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_EmployeeManagement;

namespace TruckingSystem.Desktop.SERVICE_EmployeeManagement;

public class EmployeeApiService
{
    private readonly ApiConnection _conn;

    public EmployeeApiService(ApiConnection conn)
    {
        _conn = conn;
    }

    public Task<List<EmployeeListItem>> GetEmployeesAsync()
        => _conn.GetAsync<List<EmployeeListItem>>("/api/employees")!;

    public Task<List<LookupItem>> GetDepartmentsAsync()
        => _conn.GetAsync<List<LookupItem>>("/api/employees/departments")!;

    public Task<List<LookupItem>> GetPositionsAsync()
        => _conn.GetAsync<List<LookupItem>>("/api/employees/positions")!;

    public Task CreateEmployeeAsync(string code, string fullName, string? phone, int? departmentId, int? positionId)
        => _conn.PostAsync<object>("/api/employees", new { code, fullName, phone, departmentId, positionId });

    /// <summary>Tạo hồ sơ tài xế cho 1 nhân viên đã có sẵn.</summary>
    public Task<EmployeeActionResultItem> CreateDriverProfileAsync(int employeeId, string licenseNumber, string licenseClass,
        DateTime licenseExpiryDate, DateTime? healthCertExpiryDate, int yearsOfExperience)
        => _conn.PostAsync<EmployeeActionResultItem>($"/api/employees/{employeeId}/driver-profile", new
        {
            licenseNumber,
            licenseClass,
            licenseExpiryDate,
            healthCertExpiryDate,
            yearsOfExperience,
        })!;
}