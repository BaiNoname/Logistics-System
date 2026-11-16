using TruckingSystem.Domain.Common;
using TruckingSystem.Domain.Enums;

namespace TruckingSystem.Domain.Entities;

/// <summary>LỊCH SỬ ĐỔI HẠN GIAO của 1 đơn — mỗi lần đổi 1 dòng: hạn cũ → hạn mới, BÊN YÊU CẦU, lý do,
/// người thao tác. Bên yêu cầu quyết định HẠN TÍNH TRỄ (DeadlineBasisRules): khách dời thì hạn mới là mốc tính đúng
/// hạn; công ty dời thì vẫn tính trễ theo hạn cũ. Đổi hạn không tự sinh phí lưu kho (xem StorageFeeRules).
/// Dòng IsFeeOnly = lựa chọn bên chịu phí cho các ngày hàng nằm kho sau hạn tính trễ (OldDate = hạn tính trễ).</summary>
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

    /// <summary>Đơn giá lưu kho (đ/m³/ngày) của kho lúc ghi — chỉ để tra cứu (dòng cũ trước khi đổi cách tính phí).</summary>
    public decimal StorageFeePerM3PerDay { get; set; }

    /// <summary>Số m³ hàng của đơn nằm trong kho lúc ghi + phí ước tính cũ — chỉ để tra cứu, không dùng tính phí nữa.</summary>
    public decimal VolumeM3AtChange { get; set; }
    public decimal EstimatedFee { get; set; }

    /// <summary>true = dòng LỰA CHỌN BÊN CHỊU PHÍ cho các ngày hàng nằm kho sau hạn tính trễ (OldDate = hạn tính trễ),
    /// KHÔNG phải lần đổi hạn giao. (Dòng "ghi nhận phí bổ sung" cũ cũng là IsFeeOnly — chức năng đó đã bỏ.)</summary>
    public bool IsFeeOnly { get; set; }
}
