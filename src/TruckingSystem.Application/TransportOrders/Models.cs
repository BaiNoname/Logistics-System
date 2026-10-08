namespace TruckingSystem.Application.TransportOrders;

public record CargoLineDto(string CargoType, int PackageCount, decimal WeightKg, decimal VolumeM3, bool IsFragile, bool IsHazmat, decimal QtyTotal);

public record CreateTransportOrderRequest(
    int CustomerId,
    string PickupPoint,
    string DropoffPoint,
    int WarehouseId,
    DateTime? CommittedDeliveryDate,
    List<CargoLineDto> CargoLines,
    int? PickupLocationId = null,
    int? DropoffLocationId = null,
    string PickupMethod = "CompanyPickup" // "CompanyPickup" | "SelfDelivery"
);

/// <summary>Sửa đơn — CHỈ cho phép khi đơn còn ở trạng thái Draft (chưa xác
/// nhận). Không có CustomerId — không cho đổi khách hàng của 1 đơn đã tạo.</summary>
public record UpdateTransportOrderRequest(
    string PickupPoint,
    string DropoffPoint,
    int WarehouseId,
    DateTime? CommittedDeliveryDate,
    List<CargoLineDto> CargoLines,
    int? PickupLocationId = null,
    int? DropoffLocationId = null,
    string PickupMethod = "CompanyPickup"
);

public record TransportOrderListItemDto(
    int Id, string Code, string CustomerName, string Status,
    decimal ProgressPercent, string PickupPoint, string DropoffPoint, string PublicTrackingToken,
    int ActiveTripCount = 0 // số chuyến chưa hủy — Desktop dùng để khóa nút "Chia xe" khi đơn đã chia rồi
);

public record TripSummaryDto(int Id, string VehiclePlate, string DriverName, string Status, decimal QtyPlanned);

/// <summary>Kết quả của các thao tác đổi trạng thái đơn (Xác nhận/Hủy) — không
/// trả nguyên TransportOrderDetailDto vì Desktop chỉ cần biết có thành công
/// không và lý do nếu thất bại, rồi tự gọi lại GetByIdAsync để load lại mới nhất.</summary>
public record OrderActionResult(bool Success, string? Message);

public record TransportOrderDetailDto(
    int Id, string Code, string CustomerName, string Status, decimal ProgressPercent,
    string PickupPoint, string DropoffPoint, DateTime? CommittedDeliveryDate,
    string PublicTrackingToken, List<CargoLineDto> CargoLines, List<TripSummaryDto> Trips,
    double? PickupLat, double? PickupLng, double? DropoffLat, double? DropoffLng,
    string PickupMethod, int CustomerId, int WarehouseId, string WarehouseName,
    int? PickupLocationId = null, int? DropoffLocationId = null // để form Sửa đơn chọn đúng Địa điểm theo Id
);