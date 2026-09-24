using TruckingSystem.Domain.Common;
using TruckingSystem.Domain.Enums;

namespace TruckingSystem.Domain.Entities;

public class Vehicle : BaseEntity
{
    public string PlateNumber { get; set; } = string.Empty;
    public string VehicleType { get; set; } = string.Empty;
    public decimal CapacityQty { get; set; }
    public decimal CapacityKg { get; set; }
    public decimal CapacityM3 { get; set; }
    public VehicleStatus Status { get; set; } = VehicleStatus.Available;

    /// <summary>Định mức tiêu hao nhiên liệu (lít/100km) — dùng để TỰ ĐỘNG
    /// tính chi phí xăng ƯỚC TÍNH dựa trên khoảng cách THẬT đi được (tính từ
    /// GPS Waypoints app tài xế gửi lên), so sánh với chi phí THỰC TẾ ghi tay
    /// (FuelLog) để đo lường mức độ chênh lệch/hiệu quả sau này.</summary>
    public decimal? FuelConsumptionPer100Km { get; set; }

    /// <summary>Loại nhiên liệu xe này dùng (VD: "Dầu DO", "Xăng RON95") —
    /// khớp đúng với FuelType trong bảng giá xăng (FuelPriceHistory) để lấy
    /// đúng giá mới nhất khi tính chi phí ước tính.</summary>
    public string? FuelType { get; set; }

    public int? BranchId { get; set; }
    public Branch? Branch { get; set; }

    public ICollection<VehicleDocument> Documents { get; set; } = new List<VehicleDocument>();
    public ICollection<Trip> Trips { get; set; } = new List<Trip>();
}
