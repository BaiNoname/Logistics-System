using TruckingSystem.Domain.Common;
using TruckingSystem.Domain.Enums;

namespace TruckingSystem.Domain.Entities;

/// <summary>
/// Hồ sơ nhân viên thật (khác tài khoản đăng nhập AspNetUsers).
/// UserId có thể null vì không phải nhân viên nào cũng cần đăng nhập hệ thống.
/// </summary>
public class Employee : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public EmployeeGender? Gender { get; set; }
    public string? Phone { get; set; }
    public string? IdentityCardNo { get; set; }
    public string? Address { get; set; }
    public DateTime HireDate { get; set; }
    public DateTime? TerminationDate { get; set; }
    public EmployeeStatus Status { get; set; } = EmployeeStatus.Active;

    public int? DepartmentId { get; set; }
    public Department? Department { get; set; }

    public int? PositionId { get; set; }
    public Position? Position { get; set; }

    // FK tới AspNetUsers.Id (kiểu string vì Identity dùng string Id) — nullable
    public string? UserId { get; set; }



    public DriverProfile? DriverProfile { get; set; }
    public ICollection<EmployeeDocument> Documents { get; set; } = new List<EmployeeDocument>();

    public static bool IsValidDateOfBirth(DateTime? dateOfBirth, DateTime today)
        => dateOfBirth is null || (dateOfBirth.Value.Year >= 1900 && dateOfBirth.Value.Date <= today.Date);
}
