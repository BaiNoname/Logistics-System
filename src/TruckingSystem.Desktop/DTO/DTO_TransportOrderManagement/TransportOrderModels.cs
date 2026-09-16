namespace TruckingSystem.Desktop.DTO_TransportOrderManagement;

public class TransportOrderListItem
{
    public int Id { get; set; }
    public string Code { get; set; } = "";
    public string CustomerName { get; set; } = "";
    public string Status { get; set; } = "";
    public decimal ProgressPercent { get; set; }
    public string PickupPoint { get; set; } = "";
    public string DropoffPoint { get; set; } = "";
    public string PublicTrackingToken { get; set; } = "";
}

public class CargoLineItem
{
    public string CargoType { get; set; } = "";
    public int PackageCount { get; set; }
    public decimal WeightKg { get; set; }
    public decimal VolumeM3 { get; set; }
    public bool IsFragile { get; set; }
    public bool IsHazmat { get; set; }
    public decimal QtyTotal { get; set; }
}

public class TripSummaryItem
{
    public int Id { get; set; }
    public string VehiclePlate { get; set; } = "";
    public string DriverName { get; set; } = "";
    public string Status { get; set; } = "";
    public decimal QtyPlanned { get; set; }
}

public class TransportOrderDetailItem
{
    public int Id { get; set; }
    public string Code { get; set; } = "";
    public string CustomerName { get; set; } = "";
    public string Status { get; set; } = "";
    public decimal ProgressPercent { get; set; }
    public string PickupPoint { get; set; } = "";
    public string DropoffPoint { get; set; } = "";
    public DateTime? CommittedDeliveryDate { get; set; }
    public string PublicTrackingToken { get; set; } = "";
    public List<CargoLineItem> CargoLines { get; set; } = new();
    public List<TripSummaryItem> Trips { get; set; } = new();
    public double? PickupLat { get; set; }
    public double? PickupLng { get; set; }
    public double? DropoffLat { get; set; }
    public double? DropoffLng { get; set; }
}

public class AllocationResultItemDto
{
    public int TripId { get; set; }
    public string VehiclePlate { get; set; } = "";
    public decimal QtyAssigned { get; set; }
}

public class AllocateVehiclesResult
{
    public bool Success { get; set; }
    public decimal TotalQtyToShip { get; set; }
    public decimal TotalQtyAssigned { get; set; }
    public List<AllocationResultItemDto> Trips { get; set; } = new();
    public string? Message { get; set; }
}
