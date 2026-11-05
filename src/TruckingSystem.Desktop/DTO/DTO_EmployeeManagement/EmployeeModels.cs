namespace TruckingSystem.Desktop.DTO_EmployeeManagement
{
    public class EmployeeListItem
    {
        public int Id { get; set; }
        public string Code { get; set; } = "";
        public string FullName { get; set; } = "";
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? IdentityCardNo { get; set; } = "";
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public DateTime HireDate { get; set; }
        public DateTime? TerminationDate { get; set; }
        public int? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public int? PositionId { get; set; }
        public string? PositionName { get; set; }
        public string Status { get; set; } = "";
        public bool IsDriver { get; set; }
    }

    public class LookupItem
    {
        public int? DepartmentId { get; set; }
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public override string ToString() => Name;
    }

    public class EmployeeActionResultItem
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
    }

    public class DriverProfileItem
    {
        public string LicenseNumber { get; set; } = "";
        public string LicenseClass { get; set; } = "";
        public DateTime LicenseExpiryDate { get; set; }
        public DateTime? HealthCertExpiryDate { get; set; }
        public int YearsOfExperience { get; set; }
        public string? GoogleCalendarId { get; set; }
        public int? WarehouseId { get; set; }
    }

    /// <summary>1 ca làm việc đã đăng ký cho tài xế — Greedy dùng để chỉ chọn
    /// tài xế thật sự đang trong ca hôm nay khi chia xe.</summary>
    public class DriverScheduleItem
    {
        public int Id { get; set; }
        public DateTime ShiftDate { get; set; }
        public string ShiftType { get; set; } = "";
    }
}
