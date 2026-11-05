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
                e.Id, e.Code, e.FullName, e.Phone, e.HireDate,
                e.DepartmentId,
                e.Department != null ? e.Department.Name : null,
                e.PositionId,
                e.Position != null ? e.Position.Name : null,
                e.Status.ToString(),
                driverEmployeeIds.Contains(e.Id),
                e.IdentityCardNo, e.Address, e.TerminationDate, e.DateOfBirth, e.Gender.HasValue ? e.Gender.Value.ToString() : null))
            .ToListAsync();
    }

    public async Task<EmployeeActionResult> CreateDriverProfileAsync(int employeeId, CreateDriverProfileRequest request)
    {
        var employee = await _db.Employees.FindAsync(employeeId);
        if (employee is null) return new EmployeeActionResult(false, "Không tìm thấy nhân viên.");

        var existing = await _db.DriverProfiles.AnyAsync(dp => dp.EmployeeId == employeeId);
        if (existing) return new EmployeeActionResult(false, "Nhân viên này đã có hồ sơ tài xế rồi.");
        var profileError = await ValidateDriverProfileAsync(employeeId, request.LicenseNumber, request.LicenseClass,
            request.LicenseExpiryDate, request.HealthCertExpiryDate, request.YearsOfExperience, request.WarehouseId);
        if (profileError is not null) return new EmployeeActionResult(false, profileError);

        _db.DriverProfiles.Add(new DriverProfile
        {
            EmployeeId = employeeId,
            LicenseNumber = request.LicenseNumber.Trim().ToUpperInvariant(),
            LicenseClass = TruckingSystem.Application.Drivers.DriverLicenseRules.NormalizeClass(request.LicenseClass),
            LicenseExpiryDate = request.LicenseExpiryDate,
            HealthCertExpiryDate = request.HealthCertExpiryDate,
            YearsOfExperience = request.YearsOfExperience,
            GoogleCalendarId = request.GoogleCalendarId,
            WarehouseId = request.WarehouseId,
        });
        await _db.SaveChangesAsync();
        return new EmployeeActionResult(true, null);
    }

    /// <summary>Kiểm tra hồ sơ tài xế ở SERVER (trước đây chỉ Desktop bắt số bằng trống — gọi thẳng
    /// API thì số bằng trống, bằng đã hết hạn, hạng bằng gõ tay ngoài danh mục đều lưu được):
    ///  - Số bằng lái bắt buộc, 6–20 ký tự chữ / số, không trùng tài xế khác.
    ///  - Hạng bằng thuộc danh mục hạng ô tô (B, B2, C1, C, CE, D..., FC) — Chia xe dựa vào đây.
    ///  - Bằng lái và giấy khám sức khỏe BẮT BUỘC, còn hạn (tính theo ngày Việt Nam).
    ///  - Kinh nghiệm 0–60 năm; kho đóng quân (nếu chọn) phải tồn tại.</summary>
    private async Task<string?> ValidateDriverProfileAsync(int employeeId, string? licenseNumber, string? licenseClass,
        DateTime licenseExpiry, DateTime? healthCertExpiry, int yearsOfExperience, int? warehouseId)
    {
        var number = (licenseNumber ?? "").Trim().ToUpperInvariant();
        if (number.Length == 0) return "Vui lòng nhập số bằng lái.";
        if (number.Length < 6 || number.Length > 20 || !number.All(char.IsLetterOrDigit))
            return "Số bằng lái không hợp lệ — chỉ gồm chữ / số, 6–20 ký tự (bằng mới: 12 chữ số).";
        if (await _db.DriverProfiles.AnyAsync(dp => dp.EmployeeId != employeeId && dp.LicenseNumber.ToUpper() == number))
            return $"Số bằng lái {number} đã thuộc hồ sơ tài xế khác.";
        if (string.IsNullOrWhiteSpace(licenseClass)) return "Vui lòng chọn hạng bằng lái.";
        if (!TruckingSystem.Application.Drivers.DriverLicenseRules.IsKnownClass(licenseClass))
            return $"Hạng bằng \"{licenseClass}\" không hợp lệ — chọn trong danh sách (B, B2, C1, C, CE, D, E, FC...).";
        var today = TruckingSystem.Application.Common.VnClock.Today;
        if (licenseExpiry.Date < today)
            return $"Bằng lái đã hết hạn ({licenseExpiry:dd/MM/yyyy}) — nhập ngày hết hạn của bằng mới (đã đổi bằng).";
        if (healthCertExpiry is null) return "Vui lòng nhập ngày hết hạn giấy khám sức khỏe — tài xế bắt buộc có giấy khám còn hạn.";
        if (healthCertExpiry.Value.Date < today)
            return $"Giấy khám sức khỏe đã hết hạn ({healthCertExpiry.Value:dd/MM/yyyy}) — nhập ngày hết hạn của giấy khám mới.";
        if (yearsOfExperience < 0 || yearsOfExperience > 60) return "Số năm kinh nghiệm phải từ 0 đến 60.";
        if (warehouseId is int wid && !await _db.Warehouses.AnyAsync(w => w.Id == wid))
            return "Kho đóng quân đã chọn không tồn tại.";
        return null;
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
        var profileError = await ValidateDriverProfileAsync(employeeId, request.LicenseNumber, request.LicenseClass,
            request.LicenseExpiryDate, request.HealthCertExpiryDate, request.YearsOfExperience, request.WarehouseId);
        if (profileError is not null) return new EmployeeActionResult(false, profileError);
        // CÙNG quy tắc với xe: tài xế còn chuyến chưa xong thì chưa đổi kho đóng quân.
        if (request.WarehouseId != profile.WarehouseId)
        {
            var activeTripIds = await _db.Trips
                .Where(t => t.DriverEmployeeId == employeeId
                            && (t.Status == TripStatus.Planned || t.Status == TripStatus.InProgress || t.Status == TripStatus.PendingConfirmation))
                .OrderBy(t => t.Id).Select(t => t.Id).Take(5).ToListAsync();
            if (activeTripIds.Count > 0)
                return new EmployeeActionResult(false,
                    $"Tài xế còn chuyến chưa xong (#{string.Join(", #", activeTripIds)}) — chưa thể đổi kho đóng quân. " +
                    "Đổi tài xế khác cho các chuyến chưa chạy (màn Chuyến → Đổi xe / tài xế) hoặc chờ chuyến xong.");
        }

        // Hạng bằng / hạn bằng lái / giấy khám MỚI vẫn phải đáp ứng các chuyến CHƯA CHẠY đã chia cho tài xế.
        var plannedTrips = await _db.Trips
            .Where(t => t.DriverEmployeeId == employeeId && t.Status == TripStatus.Planned)
            .Select(t => new { t.Id, t.PlannedStartDate, t.EstimatedDurationMinutes, t.Vehicle.PlateNumber, t.Vehicle.CapacityKg, t.Vehicle.VehicleType })
            .ToListAsync();
        if (plannedTrips.Count > 0)
        {
            var today = TruckingSystem.Application.Common.VnClock.Today;
            var newClass = TruckingSystem.Application.Drivers.DriverLicenseRules.NormalizeClass(request.LicenseClass);
            foreach (var t in plannedTrips.OrderBy(t => t.Id))
            {
                var from = t.PlannedStartDate is DateTime p && p.Date > today ? p.Date : today;
                var lastDay = from.AddDays(Math.Max(1, (int)Math.Ceiling((t.EstimatedDurationMinutes ?? 0) / 480.0)) - 1);
                string? reason = TruckingSystem.Application.Drivers.DriverLicenseRules.BlockReason(newClass, t.CapacityKg, t.VehicleType) is { } lic
                    ? $"không lái được xe {t.PlateNumber} ({lic})"
                    : request.LicenseExpiryDate.Date < lastDay ? $"bằng lái hết hạn {request.LicenseExpiryDate:dd/MM/yyyy}, trước khi chuyến xong ({lastDay:dd/MM/yyyy})"
                    : request.HealthCertExpiryDate is DateTime hc && hc.Date < lastDay ? $"giấy khám sức khỏe hết hạn {hc:dd/MM/yyyy}, trước khi chuyến xong ({lastDay:dd/MM/yyyy})"
                    : null;
                if (reason is not null)
                    return new EmployeeActionResult(false,
                        $"Với thông tin mới, tài xế {reason} — chuyến chưa chạy #{t.Id} sẽ không xuất phát được. " +
                        "Đổi tài xế cho chuyến đó trước (màn Chuyến → Đổi xe / tài xế) rồi mới lưu hồ sơ.");
            }
        }

        profile.LicenseNumber = request.LicenseNumber.Trim().ToUpperInvariant();
        profile.LicenseClass = TruckingSystem.Application.Drivers.DriverLicenseRules.NormalizeClass(request.LicenseClass);
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
        if (!Employee.IsValidNewEmployeeDateOfBirth(request.DateOfBirth, TruckingSystem.Application.Common.VnClock.Today))
            return new(false, "Vui lòng chọn ngày sinh từ năm 1900 và nhân viên phải trên 18 tuổi.");
        if (request.TerminationDate.HasValue)
            return new(false, "Nhân viên mới mặc định đang làm. Ngày nghỉ việc chỉ cập nhật khi sửa nhân viên.");
        var personalError = ValidatePersonalDetails(request.DateOfBirth, request.Gender, out var gender);
        if (personalError is not null) return new(false, personalError);
        var validation = await ValidateAsync(request.Code, request.FullName, request.DepartmentId, request.PositionId);
        if (validation is not null) return new EmployeeActionResult(false, validation);
        var hireDate = request.HireDate?.Date ?? DateTime.UtcNow;
        if (hireDate.Year is < 2000 or > 2100)
            return new(false, "Ngày vào làm phải từ năm 2000–2100.");

        var employee = new Employee
        {
            Code = request.Code.Trim(),
            FullName = request.FullName.Trim(),
            DateOfBirth = request.DateOfBirth?.Date,
            Gender = gender,
            Phone = NullIfWhiteSpace(request.Phone),
            IdentityCardNo = NullIfWhiteSpace(request.IdentityCardNo),
            Address = NullIfWhiteSpace(request.Address),
            DepartmentId = request.DepartmentId,
            PositionId = request.PositionId,
            HireDate = hireDate,
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

        var personalError = ValidatePersonalDetails(request.DateOfBirth, request.Gender, out var gender);
        if (personalError is not null) return new(false, personalError);
        var validation = await ValidateAsync(request.Code, request.FullName, request.DepartmentId, request.PositionId, id);
        if (validation is not null) return new EmployeeActionResult(false, validation);
        if (!Enum.TryParse<EmployeeStatus>(request.Status, true, out var status))
            return new EmployeeActionResult(false, "Trạng thái nhân viên không hợp lệ.");

        // Older clients omit both dates; a supplied hire date also permits clearing termination.
        if (request.HireDate.HasValue || request.TerminationDate.HasValue)
        {
            var hireDate = request.HireDate?.Date ?? employee.HireDate.Date;
            if (hireDate.Year is < 2000 or > 2100 || request.TerminationDate?.Date < hireDate)
                return new(false, "Ngày vào làm phải từ năm 2000–2100; ngày nghỉ việc không được trước ngày vào làm.");
            employee.HireDate = hireDate;
            employee.TerminationDate = request.TerminationDate?.Date;
        }

        employee.Code = request.Code.Trim();
        employee.FullName = request.FullName.Trim();
        // Older clients omit both fields; an empty gender allows clearing both values.
        if (request.DateOfBirth.HasValue || request.Gender is not null)
        {
            employee.DateOfBirth = request.DateOfBirth?.Date;
            employee.Gender = gender;
        }
        employee.Phone = NullIfWhiteSpace(request.Phone);
        // Keep these fields when older clients omit them; an empty string clears the value.
        if (request.IdentityCardNo is not null)
            employee.IdentityCardNo = NullIfWhiteSpace(request.IdentityCardNo);
        if (request.Address is not null)
            employee.Address = NullIfWhiteSpace(request.Address);
        employee.DepartmentId = request.DepartmentId;
        employee.PositionId = request.PositionId;
        employee.Status = status;

        await _db.SaveChangesAsync();
        return new EmployeeActionResult(true, null);
    }

    private static string? ValidatePersonalDetails(DateTime? dateOfBirth, string? value, out EmployeeGender? gender)
    {
        gender = null;
        if (!Employee.IsValidDateOfBirth(dateOfBirth, TruckingSystem.Application.Common.VnClock.Today))
            return "Ngày sinh phải từ năm 1900 đến ngày hiện tại.";
        if (string.IsNullOrWhiteSpace(value)) return null;
        if (!Enum.TryParse<EmployeeGender>(value.Trim(), true, out var parsed) || !Enum.IsDefined(parsed))
            return "Giới tính không hợp lệ. Chọn Nam, Nữ hoặc Khác.";
        gender = parsed;
        return null;
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

        if (positionId.HasValue && await _db.Positions.AnyAsync(p => p.Id == positionId && p.DepartmentId != null && p.DepartmentId != departmentId))
            return "Chức danh không thuộc phòng ban đã chọn.";
        return null;
    }

    private static string? NullIfWhiteSpace(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    public async Task<List<LookupItemDto>> GetDepartmentsAsync()
        => await _db.Departments.Select(d => new LookupItemDto(d.Id, d.Name, null)).ToListAsync();

    public async Task<List<LookupItemDto>> GetPositionsAsync()
        => await _db.Positions.Select(p => new LookupItemDto(p.Id, p.Name, p.DepartmentId)).ToListAsync();

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
