namespace TruckingSystem.Application.Vehicles;

public record CreateVehicleRequest(string PlateNumber, string VehicleType, decimal CapacityQty, decimal CapacityKg, decimal CapacityM3, decimal? FuelConsumptionPer100Km, string? FuelType, int? WarehouseId);
/// <summary>DocumentBlockReason: lý do giấy tờ khiến xe KHÔNG được Chia xe hôm nay (null = ổn)
/// — cùng quy tắc VehicleDocumentRules mà Chia xe / Bắt đầu chuyến dùng.</summary>
public record VehicleListItemDto(int Id, string PlateNumber, string VehicleType, decimal CapacityQty, decimal CapacityKg, decimal CapacityM3, string Status, decimal? FuelConsumptionPer100Km, string? FuelType, int? WarehouseId, string? WarehouseName,
    string? DocumentBlockReason = null);

/// <summary>Sửa thông tin xe — CHO PHÉP đổi cả Status thủ công (VD: đánh dấu
/// đang bảo dưỡng/ngừng hoạt động — Greedy sẽ tự loại trừ xe không Available).</summary>
public record UpdateVehicleRequest(string PlateNumber, string VehicleType, decimal CapacityQty, decimal CapacityKg, decimal CapacityM3, string Status, decimal? FuelConsumptionPer100Km, string? FuelType, int? WarehouseId);

public record VehicleActionResult(bool Success, string? Message);

public record VehicleDocumentDto(int Id, string DocType, DateTime IssueDate, DateTime ExpiryDate, string? FileUrl);

/// <summary>Nhập giấy tờ xe mới (đăng kiểm/bảo hiểm/giấy phép vận tải) — Greedy
/// đã kiểm tra hạn các giấy tờ này khi chia xe, nhưng trước đây chưa có UI
/// để thực sự NHẬP các giấy tờ này vào hệ thống.</summary>
public record CreateVehicleDocumentRequest(string DocType, DateTime IssueDate, DateTime ExpiryDate, string? FileUrl);
