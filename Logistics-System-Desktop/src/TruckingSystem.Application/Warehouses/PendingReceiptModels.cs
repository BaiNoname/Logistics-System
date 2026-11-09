namespace TruckingSystem.Application.Warehouses;

/// <summary>1 dòng hàng của 1 chuyến Lấy hàng ĐÃ VỀ KHO (đã Hoàn tất) nhưng CHƯA nhập
/// (hết) vào kho — Vận hành nhập trên Desktop: tự chọn vị trí hoặc để Best-Fit gợi ý.
/// ZoneType: khu bắt buộc theo tính chất hàng ("Normal" / "Fragile" / "Hazmat").
/// WholeUnits: hàng đếm theo đơn vị nguyên (không nhập số lẻ).</summary>
public record PendingReceiptLineDto(int TripId, int TransportOrderId, string OrderCode, string CustomerName,
    string DriverName, string PlateNumber, DateTime? ArrivedAt,
    int CargoLineId, string CargoType, string ZoneType, bool WholeUnits,
    decimal QtyPlanned, decimal QtyReceived, decimal QtyPending,
    DateTime? DeliveryLatestStart = null); // chặng Giao của đơn phải xuất phát trước ngày này (null = chưa chia / không có hạn) — hàng gấp nhập trước

/// <summary>Nhập THỦ CÔNG 1 phần hàng của chuyến Lấy hàng vào 1 vị trí do người dùng chọn.
/// BinId = null: để Best-Fit tự chọn (có thể chia nhiều vị trí) cho riêng số lượng này.</summary>
public record PendingManualReceiveRequest(int CargoLineId, decimal Qty, int? BinId);
