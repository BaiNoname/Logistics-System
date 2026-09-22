using TruckingSystem.Domain.Common;

namespace TruckingSystem.Domain.Entities;

/// <summary>Kho trung chuyển — nơi lưu tạm hàng khách gửi trước khi gom đủ tải để chạy chuyến giao.</summary>
public class Warehouse : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public decimal? CapacityM3 { get; set; }

    /// <summary>Tọa độ thật của kho — BẮT BUỘC có để tính lộ trình đúng (chặng
    /// Pickup: Khách gửi → Kho; chặng Delivery: Kho → Khách nhận), lấy từ
    /// VietMap Autocomplete khi tạo kho, không nhập tay.</summary>
    public double? Lat { get; set; }
    public double? Lng { get; set; }

    // Kích thước thật của kho (mét) — dùng để vẽ sơ đồ tổng quan đúng tỉ lệ,
    // đặt đúng vị trí từng WarehouseZone bên trong. Không dùng cho tính toán
    // Best-Fit (vẫn dùng CapacityM3/StorageBin như trước).
    public decimal? LengthMeters { get; set; }
    public decimal? WidthMeters { get; set; }

    public int? BranchId { get; set; }
    public Branch? Branch { get; set; }

    public ICollection<GoodsReceipt> GoodsReceipts { get; set; } = new List<GoodsReceipt>();
    public ICollection<GoodsIssue> GoodsIssues { get; set; } = new List<GoodsIssue>();
    public ICollection<WarehouseStock> Stocks { get; set; } = new List<WarehouseStock>();
    public ICollection<WarehouseZone> Zones { get; set; } = new List<WarehouseZone>();
}