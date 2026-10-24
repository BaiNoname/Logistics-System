using TruckingSystem.Domain.Enums;
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

    // Vị trí CỬA kho — khai báo bằng "tường nào" + "cách góc bao nhiêu mét" để
    // cửa LUÔN nằm đúng trên tường, không thể nhập nhầm vào giữa kho. Từ đây hệ
    // thống TỰ TÍNH khoảng cách tới cửa của từng ô (đi theo lối đi vuông góc),
    // thay cho việc nhập tay từng ô như trước (dễ nhập khống/sai, VD để 0 m hết).
    // Null = kho chưa khai báo cửa -> thuật toán xếp hàng bỏ qua ưu tiên gần/xa cửa.
    public WallSide? DoorSide { get; set; }

    /// <summary>Khoảng cách (mét) từ góc tới cửa, đo dọc theo bức tường: tường
    /// Trên/Dưới đo từ góc TRÁI (theo chiều dài), tường Trái/Phải đo từ góc TRÊN
    /// (theo chiều rộng).</summary>
    public decimal? DoorOffsetMeters { get; set; }

    /// <summary>PHÍ LƯU KHO (đồng / m³ / ngày) — tính cho khách khi KHÁCH yêu cầu dời hạn giao trễ hơn
    /// (hàng phải nằm kho thêm). 0 = kho không tính phí lưu kho.</summary>
    public decimal StorageFeePerM3PerDay { get; set; }



    public ICollection<GoodsReceipt> GoodsReceipts { get; set; } = new List<GoodsReceipt>();
    public ICollection<GoodsIssue> GoodsIssues { get; set; } = new List<GoodsIssue>();
    public ICollection<WarehouseStock> Stocks { get; set; } = new List<WarehouseStock>();
    public ICollection<WarehouseZone> Zones { get; set; } = new List<WarehouseZone>();
}