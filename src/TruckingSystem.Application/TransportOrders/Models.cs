namespace TruckingSystem.Application.TransportOrders;

public record CargoLineDto(string CargoType, int PackageCount, decimal WeightKg, decimal VolumeM3, bool IsFragile, bool IsHazmat, decimal QtyTotal);

public record CreateTransportOrderRequest(
    int CustomerId,
    string PickupPoint,
    string DropoffPoint,
    DateTime? CommittedDeliveryDate,
    List<CargoLineDto> CargoLines,
    int? PickupLocationId = null,
    int? DropoffLocationId = null
);

public record TransportOrderListItemDto(
    int Id, string Code, string CustomerName, string Status,
    decimal ProgressPercent, string PickupPoint, string DropoffPoint, string PublicTrackingToken
);

public record TripSummaryDto(int Id, string VehiclePlate, string DriverName, string Status, decimal QtyPlanned);

public record TransportOrderDetailDto(
    int Id, string Code, string CustomerName, string Status, decimal ProgressPercent,
    string PickupPoint, string DropoffPoint, DateTime? CommittedDeliveryDate,
    string PublicTrackingToken, List<CargoLineDto> CargoLines, List<TripSummaryDto> Trips,
    double? PickupLat, double? PickupLng, double? DropoffLat, double? DropoffLng
);
