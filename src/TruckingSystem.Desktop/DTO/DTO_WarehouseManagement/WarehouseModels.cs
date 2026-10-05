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
        public override string ToString() => $"{CargoType} — còn {QtyRemaining}/{QtyTotal} (đơn {OrderCode})";
    }

    public class WarehouseTransactionItem
    {
        public string Type { get; set; } = "";
        public DateTime OccurredAt { get; set; }
        public string OrderCode { get; set; } = "";
        public string CargoType { get; set; } = "";
        public decimal Qty { get; set; }
        public string ByEmployeeName { get; set; } = "";
    }
}