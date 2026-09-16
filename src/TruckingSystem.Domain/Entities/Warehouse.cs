using TruckingSystem.Domain.Common;

namespace TruckingSystem.Domain.Entities;

/// <summary>Kho trung chuyển — nơi lưu tạm hàng khách gửi trước khi gom đủ tải để chạy chuyến giao.</summary>
public class Warehouse : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public decimal? CapacityM3 { get; set; }

    public int? BranchId { get; set; }
    public Branch? Branch { get; set; }

    public ICollection<GoodsReceipt> GoodsReceipts { get; set; } = new List<GoodsReceipt>();
    public ICollection<GoodsIssue> GoodsIssues { get; set; } = new List<GoodsIssue>();
    public ICollection<WarehouseStock> Stocks { get; set; } = new List<WarehouseStock>();
    public ICollection<WarehouseZone> Zones { get; set; } = new List<WarehouseZone>();
}
