using TruckingSystem.Domain.Common;
using TruckingSystem.Domain.Enums;

namespace TruckingSystem.Domain.Entities;

/// <summary>LỊCH SỬ ĐỔI HẠN GIAO của 1 đơn — mỗi lần đổi 1 dòng: hạn cũ → hạn mới, BÊN YÊU CẦU, lý do,
/// người thao tác. Khách yêu cầu dời trễ hơn thì hàng nằm kho thêm các ngày đó → khách chịu PHÍ LƯU KHO
/// (m³ hàng còn trong kho × số ngày × đơn giá của kho). Công ty yêu cầu (công ty trễ) thì không tính.
/// Đơn giá và số m³ lúc đổi được CHỐT lại (đổi giá kho về sau không làm đổi phí của lần đổi cũ);
/// phí chính thức tính theo số ngày hàng THỰC SỰ còn trong kho (xem StorageFeeRules).</summary>
public class DeliveryDeadlineChange : BaseEntity
{
    public int TransportOrderId { get; set; }
    public TransportOrder TransportOrder { get; set; } = null!;

    /// <summary>Hạn giao trước khi đổi (ngày VN); null = đơn cũ chưa có hạn (bổ sung hạn).</summary>
    public DateTime? OldDate { get; set; }
    public DateTime NewDate { get; set; }

    public DeadlineChangeRequester RequestedBy { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? ChangedByUserName { get; set; }

    /// <summary>Số ngày dời thêm (hạn mới - hạn cũ; dời sớm lại = 0).</summary>
    public int DelayDays { get; set; }

    /// <summary>Đơn giá lưu kho (đ/m³/ngày) của kho tại thời điểm đổi — chốt lại.</summary>
    public decimal StorageFeePerM3PerDay { get; set; }

    /// <summary>Số m³ hàng của đơn đang nằm trong kho lúc đổi + phí ước tính (để báo khách ngay).</summary>
    public decimal VolumeM3AtChange { get; set; }
    public decimal EstimatedFee { get; set; }

    /// <summary>true = GHI NHẬN BỔ SUNG phí lưu kho (khách chậm nhận hàng mà nhân viên quên đổi hạn, kể cả
    /// đơn đã Hoàn tất / Hủy): KHÔNG đổi hạn giao của đơn (tỉ lệ giao đúng hạn giữ nguyên), chỉ đánh dấu
    /// khoảng OldDate+1 → NewDate là do khách để tính phí theo ngày hàng thực sự còn trong kho.</summary>
    public bool IsFeeOnly { get; set; }
}
