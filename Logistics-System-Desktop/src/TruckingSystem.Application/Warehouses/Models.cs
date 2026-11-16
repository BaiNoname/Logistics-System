namespace TruckingSystem.Application.Warehouses;

public record SuggestBinRequest(int CargoLineId, decimal QtyToStore);

/// <summary>1 vị trí cụ thể trong gợi ý — có thể có NHIỀU dòng nếu số lượng
/// phải CHIA RA nhiều vị trí/khu vực mới đủ chỗ chứa (1 khu không đủ).</summary>
public record SuggestedAllocationDto(int StorageBinId, string BinCode, string ZoneName, decimal QtyToPlace, decimal RemainingAfterM3);

/// <summary>Found=true khi TỔNG các Allocations cộng lại đủ chứa hết số
/// lượng yêu cầu — có thể là 1 vị trí duy nhất (trường hợp thường gặp) hoặc
/// NHIỀU vị trí/khu vực khác nhau khi 1 chỗ không đủ chứa hết.</summary>
public record SuggestBinResponse(bool Found, List<SuggestedAllocationDto> Allocations, string? Message);

public record WarehouseZoneDto(int Id, string Name, string ZoneType, int BinCount, decimal PosX, decimal PosY, decimal WidthMeters, decimal HeightMeters);
/// <summary>1 lô hàng cụ thể đang nằm trong 1 vị trí — cho biết RÕ đây là hàng
/// của đơn nào, khách nào, loại gì — trước đây sơ đồ chỉ hiện % đầy, hoàn
/// toàn không biết bên trong là hàng của ai, nhân viên kho khó theo dõi.</summary>
public record BinContentItemDto(string OrderCode, string CustomerName, string CargoType, decimal QtyStored, DateTime ReceivedAt);

public record StorageBinDto(int Id, string Code, decimal CapacityM3, decimal CurrentUsedM3, decimal MaxWeightKg, decimal PosX, decimal PosY, decimal WidthMeters, decimal HeightMeters, decimal DistanceFromDoorMeters, List<BinContentItemDto> Contents);
public record WarehouseListItemDto(int Id, string Name, string? Address, decimal? CapacityM3, decimal? LengthMeters, decimal? WidthMeters, double? Lat, double? Lng, string? DoorSide = null, decimal? DoorOffsetMeters = null,
    decimal StorageFeePerM3PerDay = 0, // phí lưu kho (đ/m³/ngày), tính sau số ngày miễn phí
    List<string>? ZoneTypes = null,
    int FreeStorageDays = 5); // loại khu kho đang có (khu có vị trí): "Normal" / "Fragile" / "Hazmat" — màn Tạo đơn báo trước kho thiếu khu

/// <summary>SỐ VIỆC CHỜ cho menu trái — 1 lần gọi, chỉ đếm (thay cho tải nguyên danh sách đơn, chuyến
/// và 2 lần gọi cho MỖI kho như trước -> đăng nhập xong phải chờ lâu mới thấy số).
/// Phần đơn / chuyến = -1 khi tài khoản không có quyền xem (Nhân viên kho).</summary>
public record WorkCountsDto(int DraftOrders, int OrdersWaitAllocation, int TripsWaitApproval, List<WarehouseWorkCountDto> Warehouses,
    int OrdersOverdueStorage = 0); // đơn có hàng nằm kho sau hạn tính trễ, chưa chọn do bên nào
/// <summary>UrgentOut: chuyến chờ xuất có mốc "phải xuất phát trước" là hôm nay hoặc đã qua.</summary>
public record WarehouseWorkCountDto(int WarehouseId, string Name, int InTrips, int OutTrips, int UrgentOut = 0);

/// <summary>Tạo kho mới — cho phép công ty có NHIỀU kho ở nhiều địa điểm khác
/// nhau (VD: TP.HCM, Hà Nội, Đà Nẵng), mỗi kho kích thước thật riêng để vẽ
/// đúng sơ đồ tổng quan. Lat/Lng BẮT BUỘC có (lấy từ VietMap Autocomplete) để
/// tính lộ trình đúng qua kho — không phải chỉ để hiển thị.</summary>
public record CreateWarehouseRequest(string Name, string? Address, decimal? CapacityM3, decimal? LengthMeters, decimal? WidthMeters, double? Lat, double? Lng, string? DoorSide = null, decimal? DoorOffsetMeters = null,
    decimal StorageFeePerM3PerDay = 0, int FreeStorageDays = 5);

/// <summary>Sửa thông tin kho — cùng cấu trúc CreateWarehouseRequest.</summary>
public record UpdateWarehouseRequest(string Name, string? Address, decimal? CapacityM3, decimal? LengthMeters, decimal? WidthMeters, double? Lat, double? Lng, string? DoorSide = null, decimal? DoorOffsetMeters = null,
    decimal StorageFeePerM3PerDay = 0, int FreeStorageDays = 5);

/// <summary>Tạo khu vực (Zone) mới trong 1 kho — có tọa độ/kích thước thật để
/// vẽ đúng vị trí trong sơ đồ tổng quan.</summary>
public record CreateZoneRequest(string Name, string ZoneType, decimal PosX, decimal PosY, decimal WidthMeters, decimal HeightMeters);

/// <summary>Tạo vị trí lưu trữ (StorageBin) mới trong 1 khu vực.</summary>
public record CreateBinRequest(string Code, decimal CapacityM3, decimal MaxWeightKg, decimal PosX, decimal PosY, decimal WidthMeters, decimal HeightMeters, decimal DistanceFromDoorMeters);

/// <summary>Yêu cầu nhập/xuất kho THỦ CÔNG — dùng chung cho cả 2 chiều.
/// ChosenBinId: có giá trị = người dùng tự chọn tay vị trí (chỉ áp dụng khi
/// nhập kho); null = để hệ thống tự động chạy Best-Fit như trước.</summary>
public record ManualStockRequest(int TransportOrderId, int CargoLineId, decimal Qty, int? ChosenBinId = null);

/// <summary>Dùng cho ComboBox chọn dòng hàng trong màn Nhập/Xuất kho thủ công.</summary>
/// <summary>QtyRemaining = còn được NHẬP thêm (so với QtyTotal đã khai báo).
/// QtyOnHand = tồn kho HIỆN TẠI của dòng hàng này trong ĐÚNG kho đang thao
/// tác — dùng để biết còn được XUẤT tối đa bao nhiêu.</summary>
public record CargoLineOptionDto(int Id, string CargoType, decimal QtyTotal, decimal QtyRemaining, decimal QtyOnHand, string OrderCode);

/// <summary>1 dòng lịch sử nhập/xuất kho — gộp chung GoodsReceipt/GoodsIssue để hiện trên cùng 1 bảng.</summary>
/// <summary>1 dòng lịch sử nhập/xuất kho — gộp GoodsReceipt/GoodsIssue.
/// Source cho biết RÕ phiếu sinh ra từ đâu: "Chuyến #12 · Lấy hàng" / "✋ Thủ
/// công" / "Khách tự mang tới"; IsReversed = phiếu xuất tay đã được hoàn tác.</summary>
public record WarehouseTransactionDto(string Type, DateTime OccurredAt, string OrderCode, string CargoType, decimal Qty, string ByEmployeeName,
    int Id = 0, int OrderId = 0, int? TripId = null, string Source = "", bool IsManual = false, bool IsReversed = false, string? Note = null,
    bool CanReverse = false);

public record WarehouseActionResult(bool Success, string? Message);

/// <summary>Kết quả nhập kho — dùng chung bởi VehicleAllocationService (khi
/// SelfDelivery, hàng coi như về kho ngay) và TripService (khi chặng Pickup
/// hoàn tất, xe thật sự chở hàng về).</summary>
/// <summary>BinCodesUsed liệt kê TẤT CẢ vị trí đã dùng để xếp — có thể nhiều
/// hơn 1 nếu số lượng phải chia ra nhiều vị trí/khu vực.</summary>
public record ReceiveGoodsResult(bool Success, string? Message, List<string> BinCodesUsed,
    List<TruckingSystem.Application.Common.FixHint>? Fixes = null); // cách sửa nhanh khi nhập không được (xem FixHint)

/// <summary>Kết quả xuất kho — dùng bởi TripService khi chặng Delivery bắt đầu chạy.</summary>
public record IssueGoodsResult(bool Success, string? Message);

// ================= Tình trạng hàng theo đơn (màn Nhập/Xuất kho) =================

/// <summary>Toàn cảnh hàng của 1 đơn trong 1 kho: từng dòng hàng đã nhập/xuất
/// bao nhiêu, còn bao nhiêu trong kho, bao nhiêu đang GIỮ cho chuyến giao/trả
/// (không được xuất tay), và từng chuyến đã nhập/xuất tới đâu.</summary>
public record OrderStockFlowDto(int OrderId, string OrderCode, string PickupMethod,
    List<CargoStockFlowDto> CargoLines, List<TripStockFlowDto> Trips);

/// <summary>QtyCanReceive / QtyFreeForManualIssue: số lượng NHẬP / XUẤT TAY được thật sự
/// (đúng quy tắc server áp dụng khi bấm). ReceiveBlockReason / IssueBlockReason: lý do
/// không được (null = được) — màn Nhập/Xuất tay dùng để bật/tắt nút và hiện lý do.</summary>
public record CargoStockFlowDto(int CargoLineId, string CargoType, decimal QtyTotal,
    decimal QtyReceived, decimal QtyIssued, decimal QtyOnHand,
    decimal QtyReserved, decimal QtyFreeForManualIssue, string StatusText,
    decimal QtyCanReceive = 0, string? ReceiveBlockReason = null, string? IssueBlockReason = null);

public record TripStockFlowDto(int TripId, string LegType, string Status, string DriverName,
    decimal QtyPlanned, decimal QtyDone, string ProgressText);

/// <summary>Nhập/xuất thủ công: người bấm (lấy từ tài khoản đăng nhập).</summary>
public record StockPerformer(int? EmployeeId, string? UserName);

/// <summary>Đơn có liên quan tới 1 kho: đơn chọn kho này, có chuyến đi qua kho
/// này, hoặc đã từng nhập/xuất hàng tại kho này.</summary>
/// <summary>PickupMethod + QtyOnHand (hàng của đơn đang nằm trong kho này) — để màn Kho lọc đúng
/// đơn cho từng việc: "Nhận hàng khách mang tới" (đơn khách tự mang tới, đang xử lý) và
/// "Xuất hàng tồn" (đơn đã Hoàn tất / Hủy còn hàng trong kho).
/// IsHomeWarehouse: kho này đúng là kho trung chuyển ghi trên đơn (đơn chỉ "đi ngang" qua kho này thì
/// không nhập tay vào đây được). HasQtyToReceive: còn dòng hàng chưa nhập đủ số khai báo.</summary>
public record WarehouseOrderDto(int Id, string Code, string CustomerName, string Status,
    string PickupMethod = "CompanyPickup", decimal QtyOnHand = 0,
    bool IsHomeWarehouse = true, bool HasQtyToReceive = true);
