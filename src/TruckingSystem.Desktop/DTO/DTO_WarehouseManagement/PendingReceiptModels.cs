namespace TruckingSystem.Desktop.DTO_WarehouseManagement
{
    /// <summary>1 dòng hàng của 1 chuyến Lấy hàng đã về kho nhưng CHƯA nhập (hết) vào kho.</summary>
    public class PendingReceiptLineItem
    {
        public int TripId { get; set; }
        public int TransportOrderId { get; set; }
        public string OrderCode { get; set; } = "";
        public string CustomerName { get; set; } = "";
        public string DriverName { get; set; } = "";
        public string PlateNumber { get; set; } = "";
        /// <summary>Giờ tài xế báo về tới kho (UTC từ server).</summary>
        public DateTime? ArrivedAt { get; set; }
        public int CargoLineId { get; set; }
        public string CargoType { get; set; } = "";
        /// <summary>"Normal" / "Fragile" / "Hazmat" — khu bắt buộc theo tính chất hàng.</summary>
        public string ZoneType { get; set; } = "";
        /// <summary>Hàng đếm theo đơn vị nguyên — không nhập số lẻ.</summary>
        public bool WholeUnits { get; set; }
        public decimal QtyPlanned { get; set; }
        public decimal QtyReceived { get; set; }
        public decimal QtyPending { get; set; }
        /// <summary>Chặng Giao của đơn phải xuất phát trước ngày này (null = chưa chia / không có hạn / API cũ).</summary>
        public DateTime? DeliveryLatestStart { get; set; }
    }
}
