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
                e.Department != null ? e.Department.Name : null,
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
        });
        await _db.SaveChangesAsync();
        return new EmployeeActionResult(true, null);
    }

    public async Task<DriverProfileDto?> GetDriverProfileAsync(int employeeId)
    {
        var profile = await _db.DriverProfiles.FirstOrDefaultAsync(dp => dp.EmployeeId == employeeId);
        if (profile is null) return null;

        return new DriverProfileDto(profile.LicenseNumber, profile.LicenseClass, profile.LicenseExpiryDate, profile.HealthCertExpiryDate, profile.YearsOfExperience);
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

        await _db.SaveChangesAsync();
        return new EmployeeActionResult(true, null);
    }

    public async Task<EmployeeListItemDto> CreateAsync(CreateEmployeeRequest request)
    {
        var employee = new Employee
        {
            Code = request.Code,
            FullName = request.FullName,
            Phone = request.Phone,
            DepartmentId = request.DepartmentId,
            PositionId = request.PositionId,
            HireDate = DateTime.UtcNow,
            Status = EmployeeStatus.Active,
        };
        _db.Employees.Add(employee);
        await _db.SaveChangesAsync();

        var dept = request.DepartmentId.HasValue ? await _db.Departments.FindAsync(request.DepartmentId) : null;
        var pos = request.PositionId.HasValue ? await _db.Positions.FindAsync(request.PositionId) : null;

        return new EmployeeListItemDto(employee.Id, employee.Code, employee.FullName, employee.Phone,
            dept?.Name, pos?.Name, employee.Status.ToString(), IsDriver: false);
    }

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