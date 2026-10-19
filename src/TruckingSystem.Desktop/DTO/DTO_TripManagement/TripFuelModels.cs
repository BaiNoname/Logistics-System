namespace TruckingSystem.Desktop.DTO_TripManagement
{
    /// <summary>1 lần đổ xăng đã ghi của chuyến (lịch sử).</summary>
    public class FuelLogItem
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public decimal Liters { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Cost { get; set; }
        public string? StationName { get; set; }

        /// <summary>Giờ Việt Nam để hiển thị (server lưu UTC).</summary>
        public DateTime TimestampLocal => Timestamp.Kind == DateTimeKind.Local
            ? Timestamp
            : DateTime.SpecifyKind(Timestamp, DateTimeKind.Utc).ToLocalTime();
    }

    /// <summary>Đơn giá tự động theo hãng + loại nhiên liệu của xe.</summary>
    public class FuelPriceQuoteItem
    {
        public bool CanRecord { get; set; }
        public bool Found { get; set; }
        public string? FuelType { get; set; }
        public decimal UnitPrice { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public string? SourceText { get; set; }
        public string? Message { get; set; }
        /// <summary>Dung tích bình / nhiên liệu hiện có của xe (null = xe chưa khai báo).</summary>
        public decimal? TankCapacityLiters { get; set; }
        public decimal? CurrentFuelLiters { get; set; }
    }
}
