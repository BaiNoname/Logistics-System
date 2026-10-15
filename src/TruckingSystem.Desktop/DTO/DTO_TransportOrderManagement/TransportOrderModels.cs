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

    /// <summary>Hiện kèm tên khách hàng trong dropdown — trước đây chỉ hiện mã
    /// đơn, khó phân biệt khi có nhiều đơn (nhân viên kho dễ chọn nhầm đơn).</summary>
    public string DisplayText => $"{Code} — {CustomerName}";
    public string PublicTrackingToken { get; set; } = "";

    /// <summary>Số chuyến chưa hủy — > 0 nghĩa là đơn đã được chia xe.</summary>
    public int ActiveTripCount { get; set; }

    /// <summary>Hạn giao (giờ VN) và kho trung chuyển — API danh sách đơn trả về, để lọc.</summary>
    public DateTime? CommittedDeliveryDate { get; set; }
    public string? WarehouseName { get; set; }

    /// <summary>"CompanyPickup" / "SelfDelivery" — chỉ API danh sách đơn theo kho trả về.</summary>
    public string PickupMethod { get; set; } = "CompanyPickup";
    /// <summary>Hàng của đơn đang nằm trong kho đang xem — chỉ API danh sách đơn theo kho trả về.</summary>
    public decimal QtyOnHand { get; set; }
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
    /// <summary>Pickup / Delivery / Return.</summary>
    public string LegType { get; set; } = "";
}

public class TransportOrderDetailItem
{
    public int Id { get; set; }
    public string Code { get; set; } = "";
    public string CustomerName { get; set; } = "";
    public int CustomerId { get; set; }
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
    public double? WarehouseLat { get; set; }
    public double? WarehouseLng { get; set; }
    public string PickupMethod { get; set; } = "CompanyPickup";
    public int WarehouseId { get; set; }
    public string WarehouseName { get; set; } = "";
    public int? PickupLocationId { get; set; }
    public int? DropoffLocationId { get; set; }
}

/// <summary>Kết quả thao tác Xác nhận/Hủy đơn.</summary>
public class OrderActionResultItem
{
    public bool Success { get; set; }
    public string? Message { get; set; }
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
    public int EstimatedTripDays { get; set; } = 1;
    /// <summary>Chưa chia vì không kịp hạn giao — hỏi người dùng rồi gọi lại với acceptLate.</summary>
    public bool NeedsLateConfirmation { get; set; }
}