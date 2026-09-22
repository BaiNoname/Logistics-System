namespace TruckingSystem.Desktop.DTO_EmployeeManagement
{
    public class EmployeeListItem
    {
        public int Id { get; set; }
        public string Code { get; set; } = "";
        public string FullName { get; set; } = "";
        public string? Phone { get; set; }
        public string? DepartmentName { get; set; }
        public string? PositionName { get; set; }
        public string Status { get; set; } = "";
        public bool IsDriver { get; set; }
    }

    public class LookupItem
    {
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
    }
}