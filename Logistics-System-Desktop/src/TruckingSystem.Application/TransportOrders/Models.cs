namespace TruckingSystem.Application.TransportOrders;

public record CargoLineDto(string CargoType, int PackageCount, decimal WeightKg, decimal VolumeM3, bool IsFragile, bool IsHazmat, decimal QtyTotal);

public record CreateTransportOrderRequest(
    int CustomerId,
    string PickupPoint,
    string DropoffPoint,
    int WarehouseId,
    DateTime? CommittedDeliveryDate,
    List<CargoLineDto> CargoLines,
    int? PickupLocationId = null,
    int? DropoffLocationId = null,
    string PickupMethod = "CompanyPickup", // "CompanyPickup" | "SelfDelivery"
    int? ContractId = null                 // hợp đồng của khách (BẮT BUỘC — máy chủ báo lỗi nếu trống)
);

/// <summary>Sửa đơn — CHỈ cho phép khi đơn còn ở trạng thái Draft (chưa xác
/// nhận). Không có CustomerId — không cho đổi khách hàng của 1 đơn đã tạo.</summary>
public record UpdateTransportOrderRequest(
    string PickupPoint,
    string DropoffPoint,
    int WarehouseId,
    DateTime? CommittedDeliveryDate,
    List<CargoLineDto> CargoLines,
    int? PickupLocationId = null,
    int? DropoffLocationId = null,
    string PickupMethod = "CompanyPickup",
    int? ContractId = null
);

public record TransportOrderListItemDto(
    int Id, string Code, string CustomerName, string Status,
    decimal ProgressPercent, string PickupPoint, string DropoffPoint, string PublicTrackingToken,
    int ActiveTripCount = 0, // số chuyến chưa hủy — Desktop dùng để khóa nút "Chia xe" khi đơn đã chia rồi
    DateTime? CommittedDeliveryDate = null, // hạn giao (giờ VN) — để lọc "quá hạn / sắp tới hạn"
    string? WarehouseName = null,           // kho trung chuyển — để lọc theo kho
    string? ContractCode = null,            // mã hợp đồng (null = đơn không theo hợp đồng)
    int OverdueStorageDays = 0,             // hàng nằm kho sau hạn tính trễ bao nhiêu ngày mà CHƯA chọn do bên nào (0 = không)
    DateTime? LateBasisDate = null,         // HẠN TÍNH TRỄ (DeadlineBasisRules) — khác hạn giao khi công ty đã dời hạn
    int StorageDay = 0,                     // hàng đang nằm kho: ngày lưu kho thứ mấy (0 = không có hàng trong kho)
    int FreeDaysLeft = 0,                   // còn mấy ngày miễn phí lưu kho (0 = hết / không có; -1 = kho không tính phí)
    decimal StorageFeeToDate = 0,           // phí lưu kho tính cho khách tới hôm nay (tạm tính)
    decimal StockM3 = 0                     // m³ hàng của đơn đang nằm kho
);

/// <summary>LegType: Pickup / Delivery / Return — cùng 1 lô hàng đi qua nhiều chặng, nên
/// cộng "số lượng chở" của mọi chuyến sẽ ra gấp đôi/gấp ba; Desktop hiện cột Chặng.</summary>
public record TripSummaryDto(int Id, string VehiclePlate, string DriverName, string Status, decimal QtyPlanned, string LegType = "Delivery",
    string? FromName = null, string? ToName = null); // điểm đi -> điểm đến của chặng (xem TripRouteText)

/// <summary>Hợp đồng chọn được cho đơn: của đúng khách hàng, đang hiệu lực hôm nay.</summary>
/// <summary>Type: "OneTime" | "LongTerm" | null (hợp đồng cũ). UsedByOrder*: hợp đồng một lần đã gắn với đơn chưa hủy đó.</summary>
public record OrderContractOptionDto(int Id, string Code, DateTime StartDate, DateTime? EndDate, string? PriceListName,
    string? Type = null, decimal? DiscountPercent = null, int? UsedByOrderId = null, string? UsedByOrderCode = null);

/// <summary>Kết quả của các thao tác đổi trạng thái đơn (Xác nhận/Hủy) — không
/// trả nguyên TransportOrderDetailDto vì Desktop chỉ cần biết có thành công
/// không và lý do nếu thất bại, rồi tự gọi lại GetByIdAsync để load lại mới nhất.</summary>
public record OrderActionResult(bool Success, string? Message,
    List<TruckingSystem.Application.Common.FixHint>? Fixes = null); // cách sửa nhanh khi bị chặn (xem FixHint)

/// <summary>Đổi hạn giao cam kết của đơn ĐÃ XÁC NHẬN / ĐANG CHẠY (khách xin dời, thỏa thuận lùi
/// hạn...) — bắt buộc ghi lý do, hệ thống ghi nhật ký hạn cũ -> hạn mới.</summary>
public record ChangeDeadlineRequest(DateTime NewDeliveryDate, string Reason,
    string? RequestedBy = null); // "Customer" | "Company" — BẮT BUỘC chọn (khách dời -> đổi hạn tính trễ; công ty dời -> giữ hạn tính trễ)

/// <summary>1 ngày hàng của đơn nằm kho: m³ trong ngày, phí ngày đó, Kind = "Free" (trong số ngày miễn phí) /
/// "Charged" (tính cho khách) / "AfterBasis" (sau hạn tính trễ — tính hay không tùy bên chịu), Event = nhập / xuất trong ngày.</summary>
public record StorageFeeDayDto(DateTime Day, decimal VolumeM3, decimal Fee, string Kind, string? Event);

/// <summary>1 lần đổi hạn giao + HẠN TÍNH TRỄ sau lần đổi đó (khách dời -> theo hạn mới; công ty dời -> giữ nguyên).</summary>
public record DeadlineChangeDto(DateTime ChangedAt, DateTime? OldDate, DateTime NewDate, string RequestedBy, string Reason,
    string? ChangedBy, DateTime? LateBasisAfter);

/// <summary>PHÍ LƯU KHO của đơn (StorageFeeRules) — TotalFee đưa vào hóa đơn (phần hóa đơn).
/// IsFinal = số đã chốt (hàng đã rời kho, không còn phần chờ chọn bên chịu); false = tạm tính.</summary>
public record StorageFeeDto(int TransportOrderId, decimal TotalFee, bool IsFinal, string Note,
    string WarehouseName, decimal RatePerM3PerDay, int FreeDays,
    DateTime? ReceivedDate, DateTime? FreeUntil, DateTime? LateBasisDate, DateTime? CommittedDeliveryDate,
    DateTime? LastIssueDate, bool StillInStock, decimal VolumeNowM3,
    decimal CompanyBorneFee, decimal PendingFee, int AfterBasisDays, string? AfterBasisDecision, string? AfterBasisReason,
    List<StorageFeeDayDto> Days, List<DeadlineChangeDto> History);

public record TransportOrderDetailDto(
    int Id, string Code, string CustomerName, string Status, decimal ProgressPercent,
    string PickupPoint, string DropoffPoint, DateTime? CommittedDeliveryDate,
    string PublicTrackingToken, List<CargoLineDto> CargoLines, List<TripSummaryDto> Trips,
    double? PickupLat, double? PickupLng, double? DropoffLat, double? DropoffLng,
    string PickupMethod, int CustomerId, int WarehouseId, string WarehouseName,
    int? PickupLocationId = null, int? DropoffLocationId = null, // để form Sửa đơn chọn đúng Địa điểm theo Id
    double? WarehouseLat = null, double? WarehouseLng = null,   // lộ trình đơn đi QUA kho trung chuyển
    int? ContractId = null, string? ContractCode = null,         // hợp đồng của đơn (tuỳ chọn)
    string? AllocateBlockReason = null, // đơn chờ Chia xe mà CHƯA chia được (hàng khách chưa mang tới đủ / kho thiếu khu) — Desktop mờ nút + ghi lý do
    string? LateWarning = null,          // chuyến chưa chạy / đang chạy đã KHÔNG KỊP hạn giao — câu giải thích (null = ổn)
    DateTime? SuggestedDeadline = null,  // ngày giao sớm nhất nếu xuất phát ngay — gợi ý khi dời hạn
    OverdueStorageDto? OverdueStorage = null, // hàng nằm kho sau hạn tính trễ (null = không)
    DateTime? LateBasisDate = null,           // HẠN TÍNH TRỄ — khác hạn giao khi công ty đã dời hạn
    List<TruckingSystem.Application.Common.FixHint>? AllocateFixes = null // cách sửa nhanh cho AllocateBlockReason
);

/// <summary>Hàng nằm kho SAU HẠN TÍNH TRỄ (và đã hết ngày miễn phí): số ngày, m³ trung bình, đơn giá kho, phí của những
/// ngày đó, đã chọn do bên nào (Decision null = chưa chọn -> chưa tính, Desktop báo đỏ; "Customer" / "Company").</summary>
public record OverdueStorageDto(int Days, decimal VolumeM3, decimal RatePerM3PerDay, decimal EstimatedFee, string? Decision,
    DateTime? LateBasisDate = null);

/// <summary>Chọn bên chịu phí lưu kho cho phần hàng nằm kho quá hạn: Party = "Customer" | "Company"; Reason bắt buộc khi Customer.</summary>
public record StorageResponsibilityRequest(string Party, string? Reason);