namespace TruckingSystem.Desktop.DTO_DriverManagement
{
    /// <summary>Khớp đúng DriverListItemDto bên API. Danh sách = nhân viên Đang làm có chức
    /// danh Tài xế (+ người đã có hồ sơ tài xế cần lưu ý — xem Warning).
    /// HasProfile = đã có hồ sơ tài xế (bằng lái...) — chưa có thì các cột bằng lái trống.
    /// Availability: "Đang làm" / "Sẵn sàng" / "Không sẵn sàng", tính đúng theo điều kiện
    /// thật dùng khi Chia xe (Greedy).</summary>
    public class DriverListItem
    {
        public int EmployeeId { get; set; }
        public string Code { get; set; } = "";
        public string FullName { get; set; } = "";
        public string? Phone { get; set; }
        public string? PositionName { get; set; }
        public string EmployeeStatus { get; set; } = "";
        public bool HasProfile { get; set; }
        public string? LicenseNumber { get; set; }
        public string? LicenseClass { get; set; }
        public DateTime? LicenseExpiryDate { get; set; }
        public DateTime? HealthCertExpiryDate { get; set; }
        public int? YearsOfExperience { get; set; }
        public int? WarehouseId { get; set; }
        public string? WarehouseName { get; set; }
        public string ProfileStatus { get; set; } = "";
        public string? Warning { get; set; }
        public string Availability { get; set; } = "";
    }
}
