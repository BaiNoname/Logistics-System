using TruckingSystem.Domain.Common;

namespace TruckingSystem.Domain.Entities;

/// <summary>
/// Biên nhận hàng NHẬP kho. TripId có thể NULL nếu khách tự mang hàng tới
/// (PickupMethod = SelfDelivery), không cần xe của công ty đi lấy.
/// </summary>
public class GoodsReceipt : BaseEntity
{
    public int TransportOrderId { get; set; }
    public TransportOrder TransportOrder { get; set; } = null!;

    public int CargoLineId { get; set; }
    public CargoLine CargoLine { get; set; } = null!;

    public int WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; } = null!;

    /// <summary>Chuyến xe đi lấy hàng (chặng Pickup) — null nếu khách tự mang tới.</summary>
    public int? TripId { get; set; }
    public Trip? Trip { get; set; }

    /// <summary>Nhập theo chuyến = TÀI XẾ chuyến Lấy hàng; nhập thủ công = nhân
    /// viên gắn với tài khoản đang đăng nhập (null nếu chưa gắn).</summary>
    public int? ReceivedByEmployeeId { get; set; }
    public Employee? ReceivedByEmployee { get; set; }

    /// <summary>Tài khoản đăng nhập đã bấm nhập (chỉ có với thao tác THỦ CÔNG trên Desktop).</summary>
    public string? PerformedByUserName { get; set; }

    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
    public decimal QtyReceived { get; set; }
}
