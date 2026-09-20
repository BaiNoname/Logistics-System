namespace TruckingSystem.Desktop.DTO_WarehouseManagement;

public class WarehouseListItem
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? Address { get; set; }
    public decimal? CapacityM3 { get; set; }
    public decimal? LengthMeters { get; set; }
    public decimal? WidthMeters { get; set; }

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
    public int Row { get; set; }
    public int Column { get; set; }
}

public class SuggestBinResult
{
    public bool Found { get; set; }
    public int? StorageBinId { get; set; }
    public string? BinCode { get; set; }
    public string? ZoneName { get; set; }
    public decimal? RemainingAfterM3 { get; set; }
    public string? Message { get; set; }
}