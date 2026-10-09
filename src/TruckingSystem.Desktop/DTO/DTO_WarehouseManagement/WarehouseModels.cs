namespace TruckingSystem.Desktop.DTO_WarehouseManagement
{
    public class WarehouseListItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string? Address { get; set; }
        public decimal? CapacityM3 { get; set; }
        public decimal? LengthMeters { get; set; }
        public decimal? WidthMeters { get; set; }
        /// <summary>"Top" / "Bottom" / "Left" / "Right" — null = kho chưa khai báo cửa.</summary>
        public string? DoorSide { get; set; }
        public decimal? DoorOffsetMeters { get; set; }
        public double? Lat { get; set; }
        public double? Lng { get; set; }

        public override string ToString() => Name;
    }

    public class WarehouseZoneItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string ZoneType { get; set; } = "";
        public int BinCount { get; set; }
        public decimal PosX { get; set; }
        public decimal PosY { get; set; }
        public decimal WidthMeters { get; set; }
        public decimal HeightMeters { get; set; }
    }

    public class WarehouseActionResultItem
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
    }

    public class StorageBinItem
    {
        public int Id { get; set; }
        public string Code { get; set; } = "";
        public decimal CapacityM3 { get; set; }
        public decimal CurrentUsedM3 { get; set; }
        public decimal MaxWeightKg { get; set; }
        public decimal PosX { get; set; }
        public decimal PosY { get; set; }
        public decimal WidthMeters { get; set; }
        public decimal HeightMeters { get; set; }
        public decimal DistanceFromDoorMeters { get; set; }
        public List<BinContentItem> Contents { get; set; } = new();
    }

    /// <summary>1 lô hàng cụ thể đang nằm trong 1 vị trí — cho biết rõ đây là
    /// hàng của đơn nào, khách nào, loại gì.</summary>
    public class BinContentItem
    {
        public string OrderCode { get; set; } = "";
        public string CustomerName { get; set; } = "";
        public string CargoType { get; set; } = "";
        public decimal QtyStored { get; set; }
        public DateTime ReceivedAt { get; set; }
    }

    public class SuggestedAllocationItem
    {
        public int StorageBinId { get; set; }
        public string BinCode { get; set; } = "";
        public string ZoneName { get; set; } = "";
        public decimal QtyToPlace { get; set; }
        public decimal RemainingAfterM3 { get; set; }
    }

    public class SuggestBinResult
    {
        public bool Found { get; set; }
        public List<SuggestedAllocationItem> Allocations { get; set; } = new();
        public string? Message { get; set; }
    }

    /// <summary>Khớp đúng ReceiveGoodsResult backend — KHÁC SuggestBinResult (dùng
    /// "Success" không phải "Found").</summary>
    public class ReceiveGoodsResultItem
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public List<string> BinCodesUsed { get; set; } = new();
    }

    public class CargoLineOptionItem
    {
        public int Id { get; set; }
        public string CargoType { get; set; } = "";
        public decimal QtyTotal { get; set; }
        public decimal QtyRemaining { get; set; }
        public decimal QtyOnHand { get; set; }
        public string OrderCode { get; set; } = "";
        public override string ToString() => $"{CargoType} — khai {QtyTotal:0.##}, trong kho {QtyOnHand:0.##}";
    }

    public class WarehouseTransactionItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int? TripId { get; set; }
        public string Type { get; set; } = "";
        public DateTime OccurredAt { get; set; }
        public string OrderCode { get; set; } = "";
        public string CargoType { get; set; } = "";
        public decimal Qty { get; set; }
        public string ByEmployeeName { get; set; } = "";
        /// <summary>"Chuyến #12 · Lấy hàng" / "✋ Thủ công" / "Khách tự mang tới kho".</summary>
        public string Source { get; set; } = "";
        public bool IsManual { get; set; }
        public bool IsReversed { get; set; }
        public string? Note { get; set; }
        /// <summary>Server tính: phiếu xuất tay, chưa hoàn tác, đơn chưa Hoàn tất/Hủy.</summary>
        public bool CanReverse { get; set; }
    }

    /// <summary>Toàn cảnh hàng của 1 đơn trong 1 kho.</summary>
    public class OrderStockFlowItem
    {
        public int OrderId { get; set; }
        public string OrderCode { get; set; } = "";
        public string PickupMethod { get; set; } = "";
        public List<CargoStockFlowItem> CargoLines { get; set; } = new();
        public List<TripStockFlowItem> Trips { get; set; } = new();
    }

    public class CargoStockFlowItem
    {
        public int CargoLineId { get; set; }
        public string CargoType { get; set; } = "";
        public decimal QtyTotal { get; set; }
        public decimal QtyReceived { get; set; }
        public decimal QtyIssued { get; set; }
        public decimal QtyOnHand { get; set; }
        /// <summary>Đang giữ cho chuyến Giao/Trả sắp chạy — KHÔNG được xuất tay.</summary>
        public decimal QtyReserved { get; set; }
        public decimal QtyFreeForManualIssue { get; set; }
        public string StatusText { get; set; } = "";
        /// <summary>Số lượng NHẬP TAY được thật sự (đúng quy tắc server: trạng thái đơn, đúng
        /// kho, chặng lấy hàng đã xong, chừa phần chuyến lấy hàng chưa về).</summary>
        public decimal QtyCanReceive { get; set; }
        /// <summary>Lý do không nhập / không xuất tay được (null = được).</summary>
        public string? ReceiveBlockReason { get; set; }
        public string? IssueBlockReason { get; set; }
    }

    public class TripStockFlowItem
    {
        public int TripId { get; set; }
        public string LegType { get; set; } = "";
        public string Status { get; set; } = "";
        public string DriverName { get; set; } = "";
        public decimal QtyPlanned { get; set; }
        public decimal QtyDone { get; set; }
        public string ProgressText { get; set; } = "";
    }
}