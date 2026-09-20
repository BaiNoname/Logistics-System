using TruckingSystem.Domain.Common;
using TruckingSystem.Domain.Enums;

namespace TruckingSystem.Domain.Entities;

/// <summary>
/// Chuyến đi thực tế — đây là entity "mở rộng" tương ứng delivery.trip bên Odoo cũ.
/// Giữ nguyên các field GPS/route đã có, chỉ thêm liên kết TransportOrder + TripLine.
/// </summary>
public class Trip : BaseEntity
{
    public int TransportOrderId { get; set; }
    public TransportOrder TransportOrder { get; set; } = null!;

    public int VehicleId { get; set; }
    public Vehicle Vehicle { get; set; } = null!;

    public int DriverEmployeeId { get; set; }
    public Employee DriverEmployee { get; set; } = null!;

    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public TripStatus Status { get; set; } = TripStatus.Planned;

    /// <summary>Chặng lấy hàng về kho, hay chặng xuất kho đi giao. Mặc định Delivery cho mô hình cũ (đi thẳng).</summary>
    public TripLegType LegType { get; set; } = TripLegType.Delivery;

    /// <summary>Kho liên quan tới chặng này — chặng Pickup thì đây là kho hàng
    /// SẼ VỀ; chặng Delivery thì đây là kho hàng XUẤT ĐI. Null nếu đơn không
    /// qua kho trung chuyển (hiếm, thường luôn có giá trị với thiết kế hiện tại).</summary>
    public int? WarehouseId { get; set; }
    public Warehouse? Warehouse { get; set; }

    public decimal PlannedFuelCost { get; set; }
    public decimal ActualFuelCost { get; set; }

    // Route/GPS — giữ tối giản, mở rộng thêm khi tích hợp VietMap thật
    public double? StartLat { get; set; }
    public double? StartLng { get; set; }
    public double? EndLat { get; set; }
    public double? EndLng { get; set; }
    public decimal? PlannedDistanceKm { get; set; }

    public ICollection<TripLine> TripLines { get; set; } = new List<TripLine>();
    public ICollection<Waypoint> Waypoints { get; set; } = new List<Waypoint>();
    public ICollection<IncidentReport> IncidentReports { get; set; } = new List<IncidentReport>();
    public ICollection<TripExpense> TripExpenses { get; set; } = new List<TripExpense>();
    public ICollection<FuelLog> FuelLogs { get; set; } = new List<FuelLog>();
}