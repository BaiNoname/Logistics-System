using TruckingSystem.Domain.Common;
using TruckingSystem.Domain.Enums;

namespace TruckingSystem.Domain.Entities;

/// <summary>
/// Đơn vận chuyển thật sự cần điều hành — lõi của toàn hệ thống.
/// KHÔNG chứa tồn kho, vì hàng là của khách; công ty chỉ chở.
/// </summary>
public class TransportOrder : BaseEntity
{
    public string Code { get; set; } = string.Empty;

    public int? SalesOrderId { get; set; }
    public SalesOrder? SalesOrder { get; set; }

    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;

    /// <summary>Hợp đồng mà đơn này chạy theo (tuỳ chọn — khách lẻ không có hợp đồng). Phải là
    /// hợp đồng CỦA ĐÚNG khách hàng và còn hiệu lực lúc tạo / xác nhận đơn. Bên hóa đơn dùng để
    /// biết đơn tính tiền theo bảng giá của hợp đồng nào, đối soát / doanh thu theo hợp đồng.</summary>
    public int? ContractId { get; set; }
    public Contract? Contract { get; set; }

    public string PickupPoint { get; set; } = string.Empty;
    public string DropoffPoint { get; set; } = string.Empty;

    /// <summary>Kho trung chuyển dùng cho đơn này — chọn NGAY LÚC TẠO ĐƠN, do
    /// Dispatcher biết rõ tuyến đường (VD: đơn từ Bình Dương ra Hà Nội thì nên
    /// chọn Kho Hà Nội hoặc kho gần điểm giao nhất). Trước đây hệ thống TỰ
    /// ĐỘNG lấy kho ĐẦU TIÊN trong hệ thống bất kể đơn đi đâu — sai thực tế,
    /// gây ra lộ trình vô lý (kho không liên quan gì tới tuyến đường của đơn).
    /// Cả 2 chặng (Pickup + Delivery) đều dùng ĐÚNG kho này làm điểm giữa.</summary>
    public int WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; } = null!;

    // Tùy chọn — nếu chọn từ Location đã lưu thì có tọa độ, nếu gõ tay thì chỉ có text ở trên.
    public int? PickupLocationId { get; set; }
    public Location? PickupLocation { get; set; }
    public int? DropoffLocationId { get; set; }
    public Location? DropoffLocation { get; set; }
    public TransportOrderStatus Status { get; set; } = TransportOrderStatus.Draft;

    /// <summary>Chặng 1 lấy hàng: công ty cử xe đến lấy, hay khách tự mang tới kho.</summary>
    public PickupMethod PickupMethod { get; set; } = PickupMethod.CompanyPickup;

    /// <summary>Ngày hẹn giao đã cam kết với khách — dùng để hệ thống tự nhắc Dispatcher xuất kho đúng hạn.</summary>
    public DateTime? CommittedDeliveryDate { get; set; }

    /// <summary>Tiến độ %, tính lại mỗi khi TripLine cập nhật QtyDelivered.</summary>
    public decimal ProgressPercent { get; set; }



    /// <summary>Token ngẫu nhiên (GUID) dùng cho link công khai /track/{token} — không cần đăng nhập.</summary>
    public string PublicTrackingToken { get; set; } = Guid.NewGuid().ToString("N");

    public ICollection<CargoLine> CargoLines { get; set; } = new List<CargoLine>();
    public ICollection<Trip> Trips { get; set; } = new List<Trip>();
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
    public ICollection<GoodsReceipt> GoodsReceipts { get; set; } = new List<GoodsReceipt>();
    public ICollection<GoodsIssue> GoodsIssues { get; set; } = new List<GoodsIssue>();
    public ICollection<DeliveryDeadlineChange> DeadlineChanges { get; set; } = new List<DeliveryDeadlineChange>();
    public CustomerFeedback? Feedback { get; set; }
}