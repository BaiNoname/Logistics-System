namespace TruckingSystem.Application.Warehouses;

public record SuggestBinRequest(int CargoLineId, decimal QtyToStore);

public record SuggestBinResponse(
    bool Found,
    int? StorageBinId,
    string? BinCode,
    string? ZoneName,
    decimal? RemainingAfterM3,
    string? Message
);

public record WarehouseZoneDto(int Id, string Name, string ZoneType, int BinCount, decimal PosX, decimal PosY, decimal WidthMeters, decimal HeightMeters);
public record StorageBinDto(int Id, string Code, decimal CapacityM3, decimal CurrentUsedM3, decimal MaxWeightKg, int Row, int Column);
public record WarehouseListItemDto(int Id, string Name, string? Address, decimal? CapacityM3, decimal? LengthMeters, decimal? WidthMeters);

/// <summary>Tạo kho mới — cho phép công ty có NHIỀU kho ở nhiều địa điểm khác
/// nhau (VD: TP.HCM, Hà Nội, Đà Nẵng), mỗi kho kích thước thật riêng để vẽ
/// đúng sơ đồ tổng quan.</summary>
public record CreateWarehouseRequest(string Name, string? Address, decimal? CapacityM3, decimal? LengthMeters, decimal? WidthMeters);

/// <summary>Tạo khu vực (Zone) mới trong 1 kho — có tọa độ/kích thước thật để
/// vẽ đúng vị trí trong sơ đồ tổng quan.</summary>
public record CreateZoneRequest(string Name, string ZoneType, decimal PosX, decimal PosY, decimal WidthMeters, decimal HeightMeters);

/// <summary>Tạo vị trí lưu trữ (StorageBin) mới trong 1 khu vực.</summary>
public record CreateBinRequest(string Code, decimal CapacityM3, decimal MaxWeightKg, int Row, int Column);

public record WarehouseActionResult(bool Success, string? Message);

/// <summary>Kết quả nhập kho — dùng chung bởi VehicleAllocationService (khi
/// SelfDelivery, hàng coi như về kho ngay) và TripService (khi chặng Pickup
/// hoàn tất, xe thật sự chở hàng về).</summary>
public record ReceiveGoodsResult(bool Success, string? Message, int? StorageBinId, string? BinCode);

/// <summary>Kết quả xuất kho — dùng bởi TripService khi chặng Delivery bắt đầu chạy.</summary>
public record IssueGoodsResult(bool Success, string? Message);