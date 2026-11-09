using TruckingSystem.Domain.Common;

namespace TruckingSystem.Domain.Entities;

/// <summary>
/// Địa điểm đã lưu — cho phép tái sử dụng (không phải gõ lại địa chỉ mỗi lần
/// tạo đơn), đồng thời lưu sẵn tọa độ (Lat/Lng) phục vụ vẽ bản đồ và tính
/// khoảng cách sau này (tích hợp VietMap Geocoding). Khác với PickupPoint/
/// DropoffPoint dạng text tự do trên TransportOrder — Location là "địa chỉ
/// đã được chuẩn hóa", có thể gắn với 1 khách hàng cụ thể (kho/văn phòng
/// quen thuộc của họ) hoặc dùng chung cho mọi khách.
/// </summary>
public class Location : BaseEntity
{
    public string Name { get; set; } = string.Empty;     // vd: "Kho Quận 7", "Văn phòng ABC - Thủ Đức"
    public string Address { get; set; } = string.Empty;

    public double? Lat { get; set; }
    public double? Lng { get; set; }

    /// <summary>Null nếu là địa điểm dùng chung; có giá trị nếu là địa điểm riêng của 1 khách hàng.</summary>
    public int? CustomerId { get; set; }
    public Customer? Customer { get; set; }
}
