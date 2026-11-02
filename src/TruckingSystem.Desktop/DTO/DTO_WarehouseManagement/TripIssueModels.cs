namespace TruckingSystem.Desktop.DTO_WarehouseManagement
{
    /// <summary>1 dòng hàng của 1 chuyến GIAO chưa chạy tại kho — màn "📤 Xuất kho theo chuyến".
    /// TripState / CanIssue / CanUndo / BlockReason là của cả chuyến (lặp lại trên mọi dòng).</summary>
    public class TripIssueLineItem
    {
        public int TripId { get; set; }
        public int TransportOrderId { get; set; }
        public string OrderCode { get; set; } = "";
        public string CustomerName { get; set; } = "";
        public string DriverName { get; set; } = "";
        public string PlateNumber { get; set; } = "";
        public DateTime? PlannedStartDate { get; set; }
        public string TripState { get; set; } = "";
        public bool CanIssue { get; set; }
        public bool CanUndo { get; set; }
        public string? BlockReason { get; set; }
        public int CargoLineId { get; set; }
        public string CargoType { get; set; } = "";
        public decimal QtyPlanned { get; set; }
        public decimal QtyIssued { get; set; }
        public decimal QtyPending { get; set; }
        public decimal QtyOnHand { get; set; }
        /// <summary>Hàng đang nằm ở ô nào ("A-01 (5), A-02 (3)") — để soạn hàng.</summary>
        public string BinsText { get; set; } = "";
        /// <summary>Điểm giao của chuyến — xe chở hàng đi đâu.</summary>
        public string Destination { get; set; } = "—";
        /// <summary>Mốc PHẢI xuất phát trước (cùng quy tắc màn Chuyến) — null = đơn chưa có hạn / API cũ.</summary>
        public DateTime? LatestStart { get; set; }
        public DateTime? Deadline { get; set; }
        public string? DeadlineText { get; set; }
    }
}
