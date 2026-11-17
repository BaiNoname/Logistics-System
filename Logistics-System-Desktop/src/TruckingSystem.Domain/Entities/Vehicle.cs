using TruckingSystem.Domain.Common;
using TruckingSystem.Domain.Enums;

namespace TruckingSystem.Domain.Entities;

public class Vehicle : BaseEntity
{
    /// <summary>8 loại xe chuẩn của công ty (cùng danh mục dropdown Loại xe trên Desktop);
    /// cấu hình KPI tài xế chỉ nhận hệ số H cho đúng các loại này.</summary>
    public static IReadOnlyList<string> StandardTypes { get; } = Array.AsReadOnly(new[]
    {
        "Xe van 1 tấn", "Xe tải 1,5 tấn", "Xe tải 2,5 tấn", "Xe tải 5 tấn",
        "Xe tải 8 tấn", "Xe tải 15 tấn", "Đầu kéo container 20 feet", "Đầu kéo container 40 feet"
    });

    public string PlateNumber { get; set; } = string.Empty;
    public string VehicleType { get; set; } = string.Empty;
    public decimal CapacityQty { get; set; }
    public decimal CapacityKg { get; set; }
    public decimal CapacityM3 { get; set; }
    public VehicleStatus Status { get; set; } = VehicleStatus.Available;

    /// <summary>Định mức tiêu hao nhiên liệu (lít/100km) — dùng để TỰ ĐỘNG
    /// tính chi phí xăng ƯỚC TÍNH dựa trên khoảng cách THẬT đi được (tính từ
    /// GPS Waypoints app tài xế gửi lên), so sánh với chi phí THỰC TẾ ghi tay
    /// (FuelLog) để đo lường mức độ chênh lệch/hiệu quả sau này.</summary>
    public decimal? FuelConsumptionPer100Km { get; set; }

    /// <summary>Loại nhiên liệu xe này dùng (VD: "Dầu DO", "Xăng RON95") —
    /// khớp đúng với FuelType trong bảng giá xăng (FuelPriceHistory) để lấy
    /// đúng giá mới nhất khi tính chi phí ước tính.</summary>
    public string? FuelType { get; set; }

    /// <summary>Dung tích bình nhiên liệu (lít) — null = chưa khai báo.</summary>
    public decimal? FuelTankCapacityLiters { get; set; }

    /// <summary>MỨC NHIÊN LIỆU ĐO / KHAI BÁO gần nhất (lít) và thời điểm đo (UTC). Mức hiện có
    /// TÍNH ĐỘNG = mức đo + các lần đổ xăng sau mốc đo − tiêu hao ước tính (km GPS × định mức)
    /// sau mốc đo (VehicleFuelLevelRules) — không cộng/trừ vào cột này, nên xóa / sửa 1 lần
    /// đổ xăng là mức hiện có tự đúng lại, không lệch dần.</summary>
    public decimal? FuelLevelLiters { get; set; }
    public DateTime? FuelLevelMeasuredAt { get; set; }



    /// <summary>Kho/bãi mà xe này ĐÓNG QUÂN (xuất phát + quay về sau mỗi
    /// chuyến) — dùng để Greedy CHỈ chọn xe đang đậu ĐÚNG kho của đơn hàng,
    /// tránh trường hợp vô lý điều xe đang đậu Hà Nội đi lấy hàng ở Bình
    /// Dương. NULL = xe CHƯA gán kho cố định, vẫn coi là dùng được cho MỌI
    /// đơn (áp dụng mềm, không phá dữ liệu demo/xe cũ chưa kịp gán kho).</summary>
    public int? WarehouseId { get; set; }
    public Warehouse? Warehouse { get; set; }

    public ICollection<VehicleDocument> Documents { get; set; } = new List<VehicleDocument>();
    public ICollection<Trip> Trips { get; set; } = new List<Trip>();
}