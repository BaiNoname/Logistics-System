namespace TruckingSystem.Desktop.DTO_VehicleManagement
{
    public class VehicleListItem
    {
        public int Id { get; set; }
        public string PlateNumber { get; set; } = "";
        public string VehicleType { get; set; } = "";
        public decimal CapacityQty { get; set; }
        public decimal CapacityKg { get; set; }
        public decimal CapacityM3 { get; set; }
        public string Status { get; set; } = "";
        public decimal? FuelConsumptionPer100Km { get; set; }
        public string? FuelType { get; set; }
        public int? WarehouseId { get; set; }
        public string? WarehouseName { get; set; }
        /// <summary>Lý do giấy tờ khiến xe không Chia xe được hôm nay (null = ổn).</summary>
        public string? DocumentBlockReason { get; set; }
        /// <summary>Dung tích bình (lít) — null = chưa khai báo.</summary>
        public decimal? FuelTankCapacityLiters { get; set; }
        /// <summary>Nhiên liệu HIỆN CÓ (ước tính: mức đo + đổ thêm − tiêu hao theo GPS) — null = chưa khai mức đo.</summary>
        public decimal? FuelCurrentLiters { get; set; }
        public DateTime? FuelLevelMeasuredAt { get; set; }
    }

    /// <summary>Chi tiết mức nhiên liệu: mức đo gần nhất + đổ thêm − tiêu hao ước tính = hiện có.</summary>
    public class VehicleFuelLevelItem
    {
        public int VehicleId { get; set; }
        public string PlateNumber { get; set; } = "";
        public decimal? TankCapacityLiters { get; set; }
        public decimal? MeasuredLiters { get; set; }
        public DateTime? MeasuredAt { get; set; }
        public decimal RefueledLiters { get; set; }
        public decimal ConsumedLiters { get; set; }
        public decimal ConsumedKm { get; set; }
        public decimal? CurrentLiters { get; set; }
        public decimal? FuelConsumptionPer100Km { get; set; }
    }

    public class VehicleActionResultItem
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
    }

    public class VehicleDocumentItem
    {
        public int Id { get; set; }
        public string DocType { get; set; } = "";
        public DateTime IssueDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string? FileUrl { get; set; }
    }
}
