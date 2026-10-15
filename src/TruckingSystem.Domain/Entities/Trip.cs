using TruckingSystem.Domain.Common;
using TruckingSystem.Domain.Enums;

namespace TruckingSystem.Domain.Entities;

/// <summary>
/// Chuyến đi thực tế — đây là entity "mở rộng" tương ứng delivery.trip bên Odoo cũ.
/// Giữ nguyên các field GPS/route đã có, chỉ thêm liên kết TransportOrder + TripLine.
/// </summary>
public class Trip : BaseEntity
{
    public int TransportOrderId { get; set; }
    public TransportOrder TransportOrder { get; set; } = null!;

    public int VehicleId { get; set; }
    public Vehicle Vehicle { get; set; } = null!;

    public int DriverEmployeeId { get; set; }
    public Employee DriverEmployee { get; set; } = null!;

    public DateTime? StartTime { get; set; }

    /// <summary>NGÀY XUẤT PHÁT DỰ KIẾN (lịch Việt Nam) chọn lúc Chia xe — mọi kiểm tra giấy tờ xe,
    /// bằng lái, giấy khám sức khỏe, ngày làm của tài xế tính trên KHOẢNG [ngày này .. ngày cuối
    /// chuyến dự kiến], không mặc định "từ hôm nay" nữa. null = dữ liệu cũ (coi như từ hôm nay).</summary>
    public DateTime? PlannedStartDate { get; set; }
    public DateTime? EndTime { get; set; }
    public TripStatus Status { get; set; } = TripStatus.Planned;

    /// <summary>Chặng lấy hàng về kho, hay chặng xuất kho đi giao. Mặc định Delivery cho mô hình cũ (đi thẳng).</summary>
    public TripLegType LegType { get; set; } = TripLegType.Delivery;

    /// <summary>Kho liên quan tới chặng này — chặng Pickup thì đây là kho hàng
    /// SẼ VỀ; chặng Delivery thì đây là kho hàng XUẤT ĐI. Null nếu đơn không
    /// qua kho trung chuyển (hiếm, thường luôn có giá trị với thiết kế hiện tại).</summary>
    public int? WarehouseId { get; set; }
    public Warehouse? Warehouse { get; set; }

    public decimal PlannedFuelCost { get; set; }

    // Số liệu ƯỚC TÍNH từ VietMap (khoảng cách/thời gian THEO ĐƯỜNG THẬT,
    // không phải đường chim bay) — LƯU LẠI ngay khi tính (tự động lúc Chia
    // xe, hoặc lúc Dispatcher bấm "📏 Ước tính") để SAU NÀY làm được bảng
    // thống kê so sánh với số liệu THỰC TẾ (StartTime/EndTime, khoảng cách
    // GPS thật) — nếu chỉ hiện tạm thời trong hộp thoại rồi bỏ đi thì không
    // còn gì để tra cứu lại phục vụ thống kê.
    public decimal? EstimatedDistanceKm { get; set; }
    public int? EstimatedDurationMinutes { get; set; }
    public decimal ActualFuelCost { get; set; }

    // Route/GPS — giữ tối giản, mở rộng thêm khi tích hợp VietMap thật
    public double? StartLat { get; set; }
    public double? StartLng { get; set; }
    public double? EndLat { get; set; }
    public double? EndLng { get; set; }
    public decimal? PlannedDistanceKm { get; set; }

    public string? CancelReason { get; set; }

    /// <summary>Lần gần nhất Vận hành TỪ CHỐI xác nhận của tài xế — ảnh hàng chụp TRƯỚC mốc
    /// này không còn tính cho lần xác nhận sau (tài xế phải chụp ảnh mới).</summary>
    public DateTime? ConfirmRejectedAt { get; set; }

    // ---- Số liệu nhiên liệu CHỐT lúc chuyến BẮT ĐẦU ----
    // Báo cáo xăng / lợi nhuận của chuyến đã chạy dùng ĐÚNG số liệu lúc chạy: sửa loại nhiên
    // liệu / định mức của xe, hay thêm / xóa giá xăng sau này không làm đổi số các tháng đã qua.
    // Null = chuyến chạy trước khi có tính năng này -> báo cáo tính theo xe + bảng giá hiện tại.
    public string? FuelTypeAtStart { get; set; }
    public decimal? FuelConsumptionPer100KmAtStart { get; set; }
    /// <summary>Đơn giá tham chiếu (giá thị trường chung ngày bắt đầu, chưa có thì giá hãng mới nhất).</summary>
    public decimal? FuelUnitPriceAtStart { get; set; }

    public ICollection<TripLine> TripLines { get; set; } = new List<TripLine>();
    public ICollection<Waypoint> Waypoints { get; set; } = new List<Waypoint>();
    public ICollection<IncidentReport> IncidentReports { get; set; } = new List<IncidentReport>();
    public ICollection<TripExpense> TripExpenses { get; set; } = new List<TripExpense>();
    public ICollection<FuelLog> FuelLogs { get; set; } = new List<FuelLog>();
}