using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_EmployeeManagement;

namespace TruckingSystem.Desktop.SERVICE_EmployeeManagement
{
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

        public Task<EmployeeActionResultItem> CreateEmployeeAsync(string code, string fullName, string? phone, int? departmentId, int? positionId, string? identityCardNo = null, string? address = null)
            => _conn.PostAsync<EmployeeActionResultItem>("/api/employees", new { code, fullName, phone, departmentId, positionId, identityCardNo, address })!;

        public Task<EmployeeActionResultItem> UpdateEmployeeAsync(int id, string code, string fullName, string? phone, int? departmentId, int? positionId, string status, string? identityCardNo = null, string? address = null)
            => _conn.PutAsync<EmployeeActionResultItem>($"/api/employees/{id}", new { code, fullName, phone, departmentId, positionId, status, identityCardNo, address })!;

        /// <summary>Tạo hồ sơ tài xế cho 1 nhân viên đã có sẵn.</summary>
        public Task<EmployeeActionResultItem> CreateDriverProfileAsync(int employeeId, string licenseNumber, string licenseClass,
            DateTime licenseExpiryDate, DateTime? healthCertExpiryDate, int yearsOfExperience, string? googleCalendarId, int? warehouseId)
            => _conn.PostAsync<EmployeeActionResultItem>($"/api/employees/{employeeId}/driver-profile", new
            {
                licenseNumber,
                licenseClass,
                licenseExpiryDate,
                healthCertExpiryDate,
                yearsOfExperience,
                googleCalendarId,
                warehouseId,
            })!;

        /// <summary>Lấy hồ sơ tài xế hiện tại — null nếu chưa có.</summary>
        public Task<DriverProfileItem?> GetDriverProfileAsync(int employeeId)
            => _conn.GetAsync<DriverProfileItem?>($"/api/employees/{employeeId}/driver-profile");

        /// <summary>Sửa hồ sơ tài xế đã có.</summary>
        public Task<EmployeeActionResultItem> UpdateDriverProfileAsync(int employeeId, string licenseNumber, string licenseClass,
            DateTime licenseExpiryDate, DateTime? healthCertExpiryDate, int yearsOfExperience, string? googleCalendarId, int? warehouseId)
            => _conn.PutAsync<EmployeeActionResultItem>($"/api/employees/{employeeId}/driver-profile", new
            {
                licenseNumber,
                licenseClass,
                licenseExpiryDate,
                healthCertExpiryDate,
                yearsOfExperience,
                googleCalendarId,
                warehouseId,
            })!;

        /// <summary>Danh sách ca làm việc đã đăng ký của 1 tài xế.</summary>
        public Task<List<DriverScheduleItem>> GetSchedulesAsync(int employeeId)
            => _conn.GetAsync<List<DriverScheduleItem>>($"/api/employees/{employeeId}/schedules")!;

        /// <summary>Đăng ký 1 ca làm việc mới.</summary>
        public Task<EmployeeActionResultItem> CreateScheduleAsync(int employeeId, DateTime shiftDate, string shiftType)
            => _conn.PostAsync<EmployeeActionResultItem>($"/api/employees/{employeeId}/schedules", new { shiftDate, shiftType })!;

        public Task<EmployeeActionResultItem> DeleteScheduleAsync(int scheduleId)
            => _conn.DeleteAsync<EmployeeActionResultItem>($"/api/employees/schedules/{scheduleId}")!;
    }
}
