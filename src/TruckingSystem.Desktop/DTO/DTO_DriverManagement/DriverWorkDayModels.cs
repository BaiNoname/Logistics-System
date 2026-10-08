namespace TruckingSystem.Desktop.DTO_DriverManagement
{
    /// <summary>1 ngày làm việc của tài xế — khớp DriverWorkDayDto bên API.
    /// HasOldShiftType = ngày này còn dữ liệu ca cũ (Sáng/Chiều/Tối) từ trước khi
    /// chuyển sang đăng ký theo ngày.</summary>
    public class DriverWorkDayItem
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public bool HasOldShiftType { get; set; }
    }

    public class DriverWorkDayResultItem
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public int Added { get; set; }
        public int Skipped { get; set; }
    }
}
