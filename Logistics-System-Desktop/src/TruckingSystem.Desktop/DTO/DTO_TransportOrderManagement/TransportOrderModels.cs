namespace TruckingSystem.Desktop.DTO_TransportOrderManagement;

public class TransportOrderListItem
{
    public int Id { get; set; }
    public string Code { get; set; } = "";
    public string CustomerName { get; set; } = "";
    public string Status { get; set; } = "";
    public decimal ProgressPercent { get; set; }
    public string PickupPoint { get; set; } = "";
    public string DropoffPoint { get; set; } = "";

    /// <summary>Hiện kèm tên khách hàng trong dropdown — trước đây chỉ hiện mã
    /// đơn, khó phân biệt khi có nhiều đơn (nhân viên kho dễ chọn nhầm đơn).</summary>
    public string DisplayText => $"{Code} — {CustomerName}";
    public string PublicTrackingToken { get; set; } = "";

    /// <summary>Số chuyến chưa hủy — > 0 nghĩa là đơn đã được chia xe.</summary>
    public int ActiveTripCount { get; set; }

    /// <summary>Hạn giao (giờ VN) và kho trung chuyển — API danh sách đơn trả về, để lọc.</summary>
    public DateTime? CommittedDeliveryDate { get; set; }
    public string? WarehouseName { get; set; }
    /// <summary>Mã hợp đồng của đơn (null = đơn không theo hợp đồng).</summary>
    public string? ContractCode { get; set; }

    /// <summary>"CompanyPickup" / "SelfDelivery" — chỉ API danh sách đơn theo kho trả về.</summary>
    public string PickupMethod { get; set; } = "CompanyPickup";
    /// <summary>Hàng của đơn đang nằm trong kho đang xem — chỉ API danh sách đơn theo kho trả về.</summary>
    public decimal QtyOnHand { get; set; }
    /// <summary>Kho đang xem đúng là kho trung chuyển của đơn (mặc định true khi API cũ chưa trả về).</summary>
    public bool IsHomeWarehouse { get; set; } = true;
    /// <summary>Còn dòng hàng chưa nhập đủ số khai báo (mặc định true khi API cũ chưa trả về).</summary>
    public bool HasQtyToReceive { get; set; } = true;
    /// <summary>Hàng nằm kho sau hạn tính trễ bao nhiêu ngày mà CHƯA chọn do bên nào (0 = không / API cũ).</summary>
    public int OverdueStorageDays { get; set; }

    /// <summary>HẠN TÍNH TRỄ — khác hạn giao khi công ty đã dời hạn (null = API cũ / không có hạn).</summary>
    public DateTime? LateBasisDate { get; set; }
    /// <summary>Hàng đang nằm kho: ngày lưu kho thứ mấy (0 = không có hàng trong kho).</summary>
    public int StorageDay { get; set; }
    /// <summary>Còn mấy ngày miễn phí lưu kho (0 = hết / không có; -1 = kho không tính phí).</summary>
    public int FreeDaysLeft { get; set; }
    /// <summary>Phí lưu kho tính cho khách tới hôm nay (tạm tính).</summary>
    public decimal StorageFeeToDate { get; set; }
    /// <summary>m³ hàng của đơn đang nằm kho.</summary>
    public decimal StockM3 { get; set; }
    /// <summary>Đơn đã hủy còn chuyến Trả hàng chưa xong: số chuyến đó (0 = không).</summary>
    public int OpenReturnTripId { get; set; }

    /// <summary>Công ty đã dời hạn: hạn tính trễ sớm hơn hạn giao hiện tại.</summary>
    public bool HasLateBasis => LateBasisDate is DateTime b && CommittedDeliveryDate is DateTime d && b.Date != d.Date;

    /// <summary>Chữ hiện ở ô chọn đơn của màn Xuất hàng tồn: nói rõ đơn nào xuất tay được, đơn
    /// nào đang chờ xuất theo chuyến giao.</summary>
    public string StockDisplayText => Status switch
    {
        "Done" => $"{Code} — {CustomerName} · Hoàn tất, còn {QtyOnHand:0.##} trong kho",
        "Cancelled" => $"{Code} — {CustomerName} · Đã hủy, còn {QtyOnHand:0.##} trong kho",
        _ => $"{Code} — {CustomerName} · đang vận chuyển, {QtyOnHand:0.##} đang giữ chờ xuất theo chuyến giao",
    };
}

public class CargoLineItem
{
    public string CargoType { get; set; } = "";
    public int PackageCount { get; set; }
    public decimal WeightKg { get; set; }
    public decimal VolumeM3 { get; set; }
    public bool IsFragile { get; set; }
    public bool IsHazmat { get; set; }
    public decimal QtyTotal { get; set; }
}

public class TripSummaryItem
{
    public int Id { get; set; }
    public string VehiclePlate { get; set; } = "";
    public string DriverName { get; set; } = "";
    public string Status { get; set; } = "";
    public decimal QtyPlanned { get; set; }
    /// <summary>Pickup / Delivery / Return.</summary>
    public string LegType { get; set; } = "";
    /// <summary>Điểm đi -> điểm đến của chặng.</summary>
    public string? FromName { get; set; }
    public string? ToName { get; set; }
    /// <summary>Ngày chạy: đã xuất phát = ngày bắt đầu thật, chưa = ngày dự kiến.</summary>
    public DateTime? RunDate { get; set; }
    /// <summary>Km GPS thật (null = chưa có GPS).</summary>
    public decimal? GpsKm { get; set; }
}

/// <summary>Hợp đồng chọn được cho đơn (của đúng khách, đang hiệu lực).</summary>
public class OrderContractOption
{
    public int Id { get; set; }
    public string Code { get; set; } = "";
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? PriceListName { get; set; }
    /// <summary>Đơn cũ đang gắn hợp đồng nay không còn hiệu lực — vẫn hiện để giữ nguyên khi sửa.</summary>
    public bool Inactive { get; set; }
    /// <summary>"OneTime" | "LongTerm" | null (hợp đồng cũ chưa có loại — coi như dài hạn).</summary>
    public string? Type { get; set; }
    public decimal? DiscountPercent { get; set; }
    /// <summary>Hợp đồng MỘT LẦN đã gắn với đơn chưa hủy này.</summary>
    public int? UsedByOrderId { get; set; }
    public string? UsedByOrderCode { get; set; }

    public bool IsOneTime => Type == "OneTime";
    public string TypeText => Type switch { "OneTime" => "Một lần", "LongTerm" => "Dài hạn", _ => "" };

    public override string ToString()
    {
        if (Id == 0) return Code; // dòng nhắc "-- Chọn hợp đồng --"
        var period = $"{StartDate:dd/MM/yyyy} – {(EndDate is { } e ? e.ToString("dd/MM/yyyy") : "không thời hạn")}";
        var price = string.IsNullOrWhiteSpace(PriceListName) ? "" : $" · bảng giá {PriceListName}";
        if (Inactive) return $"{Code} (không còn hiệu lực)";
        var type = TypeText.Length > 0 ? $" · {TypeText}" : "";
        var discount = DiscountPercent is > 0 ? $" · ưu đãi {DiscountPercent:0.##}%" : "";
        return $"{Code}{type}{discount} ({period}){price}";
    }
}

public class TransportOrderDetailItem
{
    public int Id { get; set; }
    public string Code { get; set; } = "";
    public string CustomerName { get; set; } = "";
    public int CustomerId { get; set; }
    public string Status { get; set; } = "";
    public decimal ProgressPercent { get; set; }
    public string PickupPoint { get; set; } = "";
    public string DropoffPoint { get; set; } = "";
    public DateTime? CommittedDeliveryDate { get; set; }
    public string PublicTrackingToken { get; set; } = "";
    public List<CargoLineItem> CargoLines { get; set; } = new();
    public List<TripSummaryItem> Trips { get; set; } = new();
    public double? PickupLat { get; set; }
    public double? PickupLng { get; set; }
    public double? DropoffLat { get; set; }
    public double? DropoffLng { get; set; }
    public double? WarehouseLat { get; set; }
    public double? WarehouseLng { get; set; }
    public string PickupMethod { get; set; } = "CompanyPickup";
    public int WarehouseId { get; set; }
    public string WarehouseName { get; set; } = "";
    public int? PickupLocationId { get; set; }
    public int? DropoffLocationId { get; set; }
    public int? ContractId { get; set; }
    public string? ContractCode { get; set; }
    /// <summary>Đơn chờ Chia xe nhưng CHƯA chia được — lý do (null = chia được / API cũ).</summary>
    public string? AllocateBlockReason { get; set; }
    /// <summary>Chuyến giao đã KHÔNG KỊP hạn giao — câu giải thích (null = ổn).</summary>
    public string? LateWarning { get; set; }
    /// <summary>Ngày giao sớm nhất nếu xuất phát ngay — gợi ý khi dời hạn.</summary>
    public DateTime? SuggestedDeadline { get; set; }
    /// <summary>Hàng nằm kho sau hạn tính trễ (null = không).</summary>
    public OverdueStorageItem? OverdueStorage { get; set; }
    /// <summary>HẠN TÍNH TRỄ — khác hạn giao khi công ty đã dời hạn.</summary>
    public DateTime? LateBasisDate { get; set; }
    /// <summary>Lối tắt sửa cho AllocateBlockReason (mở sơ đồ kho / nhận hàng khách mang tới).</summary>
    public List<TruckingSystem.Desktop.Common.FixHintItem>? AllocateFixes { get; set; }

    public bool HasLateBasis => LateBasisDate is DateTime b && CommittedDeliveryDate is DateTime d && b.Date != d.Date;
}

/// <summary>Hàng nằm kho sau hạn tính trễ — Decision null = chưa chọn (chưa tính), "Customer" / "Company".</summary>
public class OverdueStorageItem
{
    public int Days { get; set; }
    public decimal VolumeM3 { get; set; }
    public decimal RatePerM3PerDay { get; set; }
    public decimal EstimatedFee { get; set; }
    public string? Decision { get; set; }
    public DateTime? LateBasisDate { get; set; }
}

/// <summary>Kết quả thao tác Xác nhận/Hủy đơn.</summary>
public class OrderActionResultItem
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    /// <summary>Cách sửa nhanh khi bị chặn — xem QuickFix.</summary>
    public List<TruckingSystem.Desktop.Common.FixHintItem>? Fixes { get; set; }
    /// <summary>HỦY ĐƠN: đã làm những gì (chuyến Trả hàng, chuyến bị hủy, hàng trả tại kho...) — xem CancelOrderFlow.</summary>
    public List<CancelEffectItem>? Effects { get; set; }
}

/// <summary>1 dòng "sẽ / đã xảy ra gì" khi hủy đơn. Kind: Return (chuyến Trả hàng — TripId = chuyến mới), CancelTrip,
/// OnRoad (chuyến lấy hàng đang chạy), ReturnAtWarehouse, Fee, Info.</summary>
public class CancelEffectItem
{
    public string Kind { get; set; } = "";
    public string Text { get; set; } = "";
    public int? TripId { get; set; }
}

/// <summary>Xem trước khi hủy đơn (GET cancel-preview).</summary>
public class CancelPreviewItem
{
    public bool CanCancel { get; set; }
    public string OrderCode { get; set; } = "";
    public List<CancelEffectItem> Effects { get; set; } = new();
    public string? BlockMessage { get; set; }
    public List<TruckingSystem.Desktop.Common.FixHintItem>? Fixes { get; set; }
}

public class AllocationResultItemDto
{
    public int TripId { get; set; }
    public string VehiclePlate { get; set; } = "";
    public decimal QtyAssigned { get; set; }
}

public class AllocateVehiclesResult
{
    public bool Success { get; set; }
    public decimal TotalQtyToShip { get; set; }
    public decimal TotalQtyAssigned { get; set; }
    public List<AllocationResultItemDto> Trips { get; set; } = new();
    public string? Message { get; set; }
    public int EstimatedTripDays { get; set; } = 1;
    /// <summary>Chưa chia vì không kịp hạn giao — hỏi người dùng rồi gọi lại với acceptLate.</summary>
    public bool NeedsLateConfirmation { get; set; }
    /// <summary>Cách sửa nhanh khi chưa chia được — xem QuickFix.</summary>
    public List<TruckingSystem.Desktop.Common.FixHintItem>? Fixes { get; set; }
}

/// <summary>1 ngày hàng của đơn nằm kho. Kind: "Free" / "Charged" / "AfterBasis" (sau hạn tính trễ).</summary>
public class StorageFeeDayItem
{
    public DateTime Day { get; set; }
    public decimal VolumeM3 { get; set; }
    public decimal Fee { get; set; }
    public string Kind { get; set; } = "";
    /// <summary>Nhập / xuất trong ngày, mốc hạn tính trễ — VD "xuất 6 m³".</summary>
    public string? Event { get; set; }
}

/// <summary>1 lần đổi hạn giao + hạn tính trễ sau lần đổi đó.</summary>
public class DeadlineChangeItem
{
    public DateTime ChangedAt { get; set; }
    public DateTime? OldDate { get; set; }
    public DateTime NewDate { get; set; }
    public string RequestedBy { get; set; } = "";
    public string Reason { get; set; } = "";
    public string? ChangedBy { get; set; }
    public DateTime? LateBasisAfter { get; set; }
}

/// <summary>Phí lưu kho của đơn: miễn phí N ngày từ ngày nhập kho, sau đó mỗi ngày m³ × đơn giá kho.</summary>
public class StorageFeeItem
{
    public int TransportOrderId { get; set; }
    public decimal TotalFee { get; set; }
    public bool IsFinal { get; set; }
    public string Note { get; set; } = "";
    public string WarehouseName { get; set; } = "";
    public decimal RatePerM3PerDay { get; set; }
    public int FreeDays { get; set; }
    public DateTime? ReceivedDate { get; set; }
    public DateTime? FreeUntil { get; set; }
    public DateTime? LateBasisDate { get; set; }
    public DateTime? CommittedDeliveryDate { get; set; }
    public DateTime? LastIssueDate { get; set; }
    public bool StillInStock { get; set; }
    public decimal VolumeNowM3 { get; set; }
    public decimal CompanyBorneFee { get; set; }
    public decimal PendingFee { get; set; }
    public int AfterBasisDays { get; set; }
    /// <summary>Bên chịu các ngày sau hạn tính trễ: null = chưa chọn, "Customer" / "Company".</summary>
    public string? AfterBasisDecision { get; set; }
    public string? AfterBasisReason { get; set; }
    public List<StorageFeeDayItem> Days { get; set; } = new();
    public List<DeadlineChangeItem> History { get; set; } = new();
}
