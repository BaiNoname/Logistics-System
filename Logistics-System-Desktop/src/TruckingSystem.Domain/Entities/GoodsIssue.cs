using TruckingSystem.Domain.Common;

namespace TruckingSystem.Domain.Entities;

/// <summary>Biên nhận hàng XUẤT kho — hàng được đưa lên xe để đi giao (gắn với Trip chặng Delivery).</summary>
public class GoodsIssue : BaseEntity
{
    public int TransportOrderId { get; set; }
    public TransportOrder TransportOrder { get; set; } = null!;

    public int CargoLineId { get; set; }
    public CargoLine CargoLine { get; set; } = null!;

    public int WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; } = null!;

    /// <summary>Chuyến xe đi giao hàng (chặng Delivery) — null nếu xuất kho
    /// THỦ CÔNG (không gắn với chuyến nào, VD: điều chỉnh tồn kho, trả hàng).</summary>
    public int? TripId { get; set; }
    public Trip? Trip { get; set; }

    /// <summary>Nhân viên chịu trách nhiệm: xuất theo chuyến = TÀI XẾ của chuyến
    /// đó; xuất thủ công = nhân viên gắn với tài khoản đang đăng nhập (null nếu
    /// tài khoản chưa gắn nhân viên — xem PerformedByUserName).</summary>
    public int? IssuedByEmployeeId { get; set; }
    public Employee? IssuedByEmployee { get; set; }

    /// <summary>Tài khoản đăng nhập đã bấm xuất (chỉ có với thao tác THỦ CÔNG trên Desktop).</summary>
    public string? PerformedByUserName { get; set; }

    /// <summary>Phiếu xuất thủ công đã được HOÀN TÁC (hàng đã trả lại vào kho).
    /// Không xóa phiếu để giữ nguyên lịch sử — chỉ đánh dấu.</summary>
    public DateTime? ReversedAt { get; set; }
    public string? ReversedByUserName { get; set; }

    public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
    public decimal QtyIssued { get; set; }
}
