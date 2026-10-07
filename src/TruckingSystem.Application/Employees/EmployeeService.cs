using Microsoft.EntityFrameworkCore;
using TruckingSystem.Application.Common.Interfaces;
using TruckingSystem.Domain.Entities;
using TruckingSystem.Domain.Enums;

namespace TruckingSystem.Application.Employees;

public class EmployeeService : IEmployeeService
{
    private readonly IApplicationDbContext _db;

    public EmployeeService(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<List<EmployeeListItemDto>> GetAllAsync()
    {
        var driverEmployeeIds = await _db.DriverProfiles.Select(dp => dp.EmployeeId).ToListAsync();

        return await _db.Employees
            .Include(e => e.Department)
            .Include(e => e.Position)
            .OrderBy(e => e.FullName)
            .Select(e => new EmployeeListItemDto(
                e.Id, e.Code, e.FullName, e.Phone,
                e.DepartmentId,
                e.Department != null ? e.Department.Name : null,
                e.PositionId,
                e.Position != null ? e.Position.Name : null,
                e.Status.ToString(),
                driverEmployeeIds.Contains(e.Id)))
            .ToListAsync();
    }

    public async Task<EmployeeActionResult> CreateDriverProfileAsync(int employeeId, CreateDriverProfileRequest request)
    {
        var employee = await _db.Employees.FindAsync(employeeId);
        if (employee is null) return new EmployeeActionResult(false, "Không tìm thấy nhân viên.");

        var existing = await _db.DriverProfiles.AnyAsync(dp => dp.EmployeeId == employeeId);
        if (existing) return new EmployeeActionResult(false, "Nhân viên này đã có hồ sơ tài xế rồi.");

        _db.DriverProfiles.Add(new DriverProfile
        {
            EmployeeId = employeeId,
            LicenseNumber = request.LicenseNumber,
            LicenseClass = request.LicenseClass,
            LicenseExpiryDate = request.LicenseExpiryDate,
            HealthCertExpiryDate = request.HealthCertExpiryDate,
            YearsOfExperience = request.YearsOfExperience,
            GoogleCalendarId = request.GoogleCalendarId,
            WarehouseId = request.WarehouseId,
        });
        await _db.SaveChangesAsync();
        return new EmployeeActionResult(true, null);
    }

    public async Task<DriverProfileDto?> GetDriverProfileAsync(int employeeId)
    {
        var profile = await _db.DriverProfiles.FirstOrDefaultAsync(dp => dp.EmployeeId == employeeId);
        if (profile is null) return null;

        return new DriverProfileDto(profile.LicenseNumber, profile.LicenseClass, profile.LicenseExpiryDate, profile.HealthCertExpiryDate, profile.YearsOfExperience, profile.GoogleCalendarId, profile.WarehouseId);
    }

    public async Task<EmployeeActionResult> UpdateDriverProfileAsync(int employeeId, UpdateDriverProfileRequest request)
    {
        var profile = await _db.DriverProfiles.FirstOrDefaultAsync(dp => dp.EmployeeId == employeeId);
        if (profile is null) return new EmployeeActionResult(false, "Nhân viên này chưa có hồ sơ tài xế.");

        profile.LicenseNumber = request.LicenseNumber;
        profile.LicenseClass = request.LicenseClass;
        profile.LicenseExpiryDate = request.LicenseExpiryDate;
        profile.HealthCertExpiryDate = request.HealthCertExpiryDate;
        profile.YearsOfExperience = request.YearsOfExperience;
        profile.GoogleCalendarId = request.GoogleCalendarId;
        profile.WarehouseId = request.WarehouseId;

        await _db.SaveChangesAsync();
        return new EmployeeActionResult(true, null);
    }

    public async Task<EmployeeActionResult> CreateAsync(CreateEmployeeRequest request)
    {
        var validation = await ValidateAsync(request.Code, request.FullName, request.DepartmentId, request.PositionId);
        if (validation is not null) return new EmployeeActionResult(false, validation);

        var employee = new Employee
        {
            Code = request.Code.Trim(),
            FullName = request.FullName.Trim(),
            Phone = NullIfWhiteSpace(request.Phone),
            DepartmentId = request.DepartmentId,
            PositionId = request.PositionId,
            HireDate = DateTime.UtcNow,
            Status = EmployeeStatus.Active,
        };
        _db.Employees.Add(employee);
        await _db.SaveChangesAsync();

        return new EmployeeActionResult(true, null);
    }

    public async Task<EmployeeActionResult> UpdateAsync(int id, UpdateEmployeeRequest request)
    {
        var employee = await _db.Employees.FindAsync(id);
        if (employee is null) return new EmployeeActionResult(false, "Không tìm thấy nhân viên.");

        var validation = await ValidateAsync(request.Code, request.FullName, request.DepartmentId, request.PositionId, id);
        if (validation is not null) return new EmployeeActionResult(false, validation);
        if (!Enum.TryParse<EmployeeStatus>(request.Status, true, out var status))
            return new EmployeeActionResult(false, "Trạng thái nhân viên không hợp lệ.");

        employee.Code = request.Code.Trim();
        employee.FullName = request.FullName.Trim();
        employee.Phone = NullIfWhiteSpace(request.Phone);
        employee.DepartmentId = request.DepartmentId;
        employee.PositionId = request.PositionId;
        employee.Status = status;

        await _db.SaveChangesAsync();
        return new EmployeeActionResult(true, null);
    }

    private async Task<string?> ValidateAsync(string code, string fullName, int? departmentId, int? positionId, int? excludedId = null)
    {
        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(fullName))
            return "Mã nhân viên và họ tên không được để trống.";
        if (await _db.Employees.AnyAsync(e => e.Code == code.Trim() && (!excludedId.HasValue || e.Id != excludedId.Value)))
            return "Mã nhân viên đã tồn tại.";
        if (departmentId.HasValue && !await _db.Departments.AnyAsync(d => d.Id == departmentId.Value))
            return "Phòng ban không tồn tại.";
        if (positionId.HasValue && !await _db.Positions.AnyAsync(p => p.Id == positionId.Value))
            return "Chức danh không tồn tại.";

        return null;
    }

    private static string? NullIfWhiteSpace(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    public async Task<List<LookupItemDto>> GetDepartmentsAsync()
        => await _db.Departments.Select(d => new LookupItemDto(d.Id, d.Name)).ToListAsync();

    public async Task<List<LookupItemDto>> GetPositionsAsync()
        => await _db.Positions.Select(p => new LookupItemDto(p.Id, p.Name)).ToListAsync();

    public async Task<List<DriverScheduleDto>> GetSchedulesAsync(int employeeId)
    {
        return await _db.DriverSchedules
            .Where(s => s.DriverEmployeeId == employeeId)
            .OrderBy(s => s.ShiftDate)
            .Select(s => new DriverScheduleDto(s.Id, s.ShiftDate, s.ShiftType.ToString()))
            .ToListAsync();
    }

    public async Task<EmployeeActionResult> CreateScheduleAsync(int employeeId, CreateDriverScheduleRequest request)
    {
        if (!Enum.TryParse<ShiftType>(request.ShiftType, out var shiftType))
            return new EmployeeActionResult(false, "Loại ca không hợp lệ.");

        // CHẶN đăng ký trùng — 1 tài xế không thể có 2 dòng lịch cho CÙNG 1
        // ngày + CÙNG 1 ca (dù có thể đăng ký NHIỀU ca khác nhau trong cùng 1
        // ngày, VD Sáng + Chiều, nhưng không trùng lặp y hệt).
        var duplicateExists = await _db.DriverSchedules.AnyAsync(s =>
            s.DriverEmployeeId == employeeId && s.ShiftDate.Date == request.ShiftDate.Date && s.ShiftType == shiftType);
        if (duplicateExists)
            return new EmployeeActionResult(false, "Tài xế này đã có đúng ca này vào ngày này rồi.");

        _db.DriverSchedules.Add(new DriverSchedule
        {
            DriverEmployeeId = employeeId,
            ShiftDate = request.ShiftDate.Date,
            ShiftType = shiftType,
        });
        await _db.SaveChangesAsync();
        return new EmployeeActionResult(true, null);
    }

    public async Task<EmployeeActionResult> DeleteScheduleAsync(int scheduleId)
    {
        var schedule = await _db.DriverSchedules.FindAsync(scheduleId);
        if (schedule is null) return new EmployeeActionResult(false, "Không tìm thấy lịch làm việc.");

        _db.DriverSchedules.Remove(schedule);
        await _db.SaveChangesAsync();
        return new EmployeeActionResult(true, null);
    }
}
