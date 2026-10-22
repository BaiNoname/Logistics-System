using Microsoft.EntityFrameworkCore;
using TruckingSystem.Application.Calendar;
using TruckingSystem.Application.Common;
using TruckingSystem.Application.Common.Interfaces;
using TruckingSystem.Application.Drivers;
using TruckingSystem.Application.Routing;
using TruckingSystem.Application.Notifications;
using TruckingSystem.Application.Vehicles;
using TruckingSystem.Application.Warehouses;
using TruckingSystem.Domain.Entities;
using TruckingSystem.Domain.Enums;

namespace TruckingSystem.Application.Trips;

/// <summary>
/// Gộp 2 nhóm chức năng dùng chung 1 Trip:
///   1. Quản lý Trip cho Dispatcher (Desktop) — xem danh sách, đổi trạng thái
///      thủ công (chưa có GPS thật để tự động).
///   2. App tài xế gọi trực tiếp — ghi nhận GPS ping, xác nhận đến nơi.
/// CẢ 2 nhóm dùng chung 1 hàm lõi HandleArrivalAsync() để đảm bảo xử lý ĐÚNG
/// theo LegType dù được gọi từ Desktop hay từ App tài xế — tránh lặp lại lỗi
/// cũ: coi MỌI lần "đến nơi" là giao hàng xong toàn bộ đơn.
///
/// ĐÃ NỐI ĐẦY ĐỦ với Kho trung chuyển (IWarehouseSlottingService):
///   - Chặng Pickup hoàn tất -> hàng về tới kho, chuyển sang "Chờ nhập kho";
///     Vận hành nhập kho riêng trên Desktop (thủ công / Best-Fit) — xem
///     PendingReceiptService. Chặng Giao chỉ chạy khi hàng đã nhập kho đủ.
///   - Chặng Delivery: Vận hành XÁC NHẬN XUẤT KHO theo chuyến (màn Kho → 📤 Xuất kho theo
///     chuyến — TripIssueService: soạn hàng, bốc lên xe, trừ tồn theo FIFO); chỉ khi đã xuất
///     ĐỦ thì chặng Giao mới bắt đầu được (Desktop hay app tài xế).
///   - Chặng Return (MỚI — chuyến trả hàng về đối tác khi đơn bị hủy sau khi
///     đã nhập kho) hoàn tất -> tự động IssueGoodsAsync (xuất kho thật, trả
///     hàng), trả xe rảnh lại — xem TransportOrderService.CancelAsync là nơi
///     TẠO Trip loại này.
///   - Đơn SelfDelivery (không có chặng Pickup) -> thủ kho nhập tay khi khách
///     mang hàng tới; nhập ĐỦ rồi mới Chia xe được (VehicleAllocationService).
/// </summary>
public class TripService : ITripService
{
    private readonly IApplicationDbContext _db;
    private readonly IWarehouseSlottingService _warehouseService;
    private readonly INotificationService _notificationService;

    private readonly IGoogleCalendarService _calendarService;
    private readonly IRouteProvider _routeProvider;

    public TripService(IApplicationDbContext db, IWarehouseSlottingService warehouseService, INotificationService notificationService, IGoogleCalendarService calendarService, IRouteProvider routeProvider)
    {
        _db = db;
        _warehouseService = warehouseService;
        _notificationService = notificationService;
        _calendarService = calendarService;
        _routeProvider = routeProvider;
    }

    // ================= Quản lý Trip (Desktop) =================

    public async Task<List<TripListItemDto>> GetAllAsync()
    {
        var trips = await _db.Trips
            .Include(t => t.TransportOrder).ThenInclude(o => o.PickupLocation)
            .Include(t => t.TransportOrder).ThenInclude(o => o.DropoffLocation)
            .Include(t => t.Vehicle)
            .Include(t => t.DriverEmployee)
            .Include(t => t.TripLines)
            .Include(t => t.Warehouse)
            .OrderByDescending(t => t.Id)
            .ToListAsync();

        // Cờ "chờ nhập kho" cho chặng Giao chưa chạy — cùng quy tắc GetStartBlockReasonAsync:
        // đơn còn hàng chờ nhập VÀ tồn kho chưa đủ cho chính chuyến giao đó.
        var orderByTrip = trips.ToDictionary(t => t.Id, t => t.TransportOrderId);
        var ordersAwaiting = (await PendingReceiptRules.LoadAsync(_db, l => true))
            .Select(p => orderByTrip.TryGetValue(p.TripId, out var o) ? o : 0)
            .Where(o => o != 0).ToHashSet();
        var stockByKey = ordersAwaiting.Count == 0
            ? new Dictionary<(int, int), decimal>()
            : (await _db.WarehouseStocks.Select(s => new { s.WarehouseId, s.CargoLineId, s.QtyOnHand }).ToListAsync())
                .GroupBy(s => (s.WarehouseId, s.CargoLineId)).ToDictionary(g => g.Key, g => g.Sum(x => x.QtyOnHand));
        bool AwaitingReceipt(Trip t) =>
            t.LegType == TripLegType.Delivery && t.Status == TripStatus.Planned && t.WarehouseId.HasValue
            && ordersAwaiting.Contains(t.TransportOrderId)
            && t.TripLines.Any(l => (stockByKey.TryGetValue((t.WarehouseId.Value, l.CargoLineId), out var q) ? q : 0) < l.QtyPlanned);

        // Tình trạng HẠN GIAO của từng chuyến (cùng quy tắc Thông báo / Chia xe).
        var todayVn = VnClock.Today;
        var tripsByOrder = trips.GroupBy(t => t.TransportOrderId).ToDictionary(g => g.Key, g => g.ToList());

        // Tính ĐÚNG tọa độ đầu-cuối theo từng chặng — dùng tọa độ KHO làm điểm
        // giữa thay vì vẽ thẳng Khách gửi → Khách nhận (sai với đơn đi qua kho):
        //   Chặng Pickup:   Khách gửi (PickupLocation) → Kho
        //   Chặng Delivery: Kho → Khách nhận (DropoffLocation)
        //   Chặng Return:   Kho → Khách gửi (PickupLocation) — trả hàng ngược
        //                   lại đúng đối tác đã lấy hàng ban đầu
        return trips.Select(t =>
        {
            double? fromLat = null, fromLng = null, toLat = null, toLng = null;
            if (t.LegType == TripLegType.Pickup)
            {
                fromLat = t.TransportOrder.PickupLocation?.Lat;
                fromLng = t.TransportOrder.PickupLocation?.Lng;
                toLat = t.Warehouse?.Lat;
                toLng = t.Warehouse?.Lng;
            }
            else if (t.LegType == TripLegType.Return)
            {
                fromLat = t.Warehouse?.Lat;
                fromLng = t.Warehouse?.Lng;
                toLat = t.TransportOrder.PickupLocation?.Lat;
                toLng = t.TransportOrder.PickupLocation?.Lng;
            }
            else // Delivery
            {
                fromLat = t.Warehouse?.Lat;
                fromLng = t.Warehouse?.Lng;
                toLat = t.TransportOrder.DropoffLocation?.Lat;
                toLng = t.TransportOrder.DropoffLocation?.Lng;
            }

            var deadline = DeliveryDeadlineRules.Evaluate(t, tripsByOrder[t.TransportOrderId], t.TransportOrder.CommittedDeliveryDate, todayVn);
            var route = TripRouteText.For(t.LegType, t.Warehouse?.Name,
                t.TransportOrder.PickupLocation?.Name, t.TransportOrder.PickupPoint,
                t.TransportOrder.DropoffLocation?.Name, t.TransportOrder.DropoffPoint);

            return new TripListItemDto(
                t.Id, t.TransportOrder.Code, t.Vehicle.PlateNumber, t.DriverEmployee.FullName,
                t.Status.ToString(), t.LegType.ToString(), t.TripLines.Sum(tl => tl.QtyPlanned),
                fromLat, fromLng, toLat, toLng,
                HasRun(t.Status, t.StartTime),
                t.EstimatedDistanceKm.HasValue && t.EstimatedDurationMinutes.HasValue,
                AwaitingReceipt(t),
                deadline.Deadline, deadline.LatestStart, deadline.Text, deadline.Level.ToString(),
                t.PlannedStartDate, t.Warehouse?.Name,
                route.From, route.To);
        }).ToList();
    }

    public async Task<TripActionResult> StartAsync(int id)
    {
        var trip = await _db.Trips
            .Include(t => t.TransportOrder)
            .Include(t => t.TripLines)
            .FirstOrDefaultAsync(t => t.Id == id);
        if (trip is null) return new TripActionResult(false, "Không tìm thấy chuyến.");
        if (trip.Status != TripStatus.Planned)
            return new TripActionResult(false, "Chỉ có thể bắt đầu chuyến đang ở trạng thái Đã lên kế hoạch (Planned).");

        // Chặng GIAO chỉ được chạy khi TẤT CẢ chặng LẤY HÀNG của đơn đã Hoàn tất
        // VÀ hàng đã được NHẬP KHO đủ (màn Hàng chờ nhập) — áp dụng CHUNG cho
        // Desktop bấm Bắt đầu lẫn app tài xế tự bắt đầu qua GPS (xem PingAsync).
        var blockReason = await GetStartBlockReasonAsync(trip);
        if (blockReason is not null) return new TripActionResult(false, blockReason);

        // Bọc transaction: chuyến có NHIỀU dòng hàng, nếu dòng 1 xuất kho xong mà
        // dòng 2 lỗi (thiếu tồn) thì trước đây dòng 1 ĐÃ BỊ TRỪ KHO vĩnh viễn dù
        // chuyến vẫn chưa bắt đầu. Giờ return khi chưa Commit -> tự Rollback hết.
        await using var transaction = await _db.Database.BeginTransactionAsync();

        // "GIÀNH" chuyển trạng thái Planned -> InProgress bằng 1 câu UPDATE có điều
        // kiện: nếu 2 yêu cầu tới cùng lúc (app gửi 2 ping liền nhau, hoặc ping +
        // Dispatcher bấm Bắt đầu) thì chỉ 1 yêu cầu thắng — yêu cầu còn lại bị DB
        // chặn chờ rồi thấy 0 dòng -> dừng, KHÔNG xuất kho lần 2.
        if (!await TryClaimStatusAsync(trip.Id, TripStatus.Planned, TripStatus.InProgress))
            return new TripActionResult(false, "Chuyến này vừa được bắt đầu bởi thao tác khác.");

        // Đúng lúc đó Vận hành vừa ĐỔI XE / TÀI XẾ (đổi xong trước khi giành được dòng)
        // -> các kiểm tra ở trên đã làm với xe/tài xế CŨ: dừng (chưa Commit -> Rollback).
        var assigned = await _db.Trips.Where(t => t.Id == trip.Id)
            .Select(t => new { t.VehicleId, t.DriverEmployeeId }).FirstAsync();
        if (assigned.VehicleId != trip.VehicleId || assigned.DriverEmployeeId != trip.DriverEmployeeId)
            return new TripActionResult(false, "Chuyến vừa được đổi xe / tài xế — hãy tải lại và thử lại.");

        // Chặng Delivery: hàng đã được KHO XÁC NHẬN XUẤT trước đó (TripIssueService) — không tự xuất
        // nữa (xuất kho là việc của nhân viên kho). Kiểm tra LẠI sau khi đã giành dòng chuyến: đúng lúc
        // đó kho vừa bấm "Hoàn tác xuất" (cũng khóa dòng chuyến) thì dừng, không chạy khi xe rỗng.
        if (trip.LegType == TripLegType.Delivery && trip.WarehouseId.HasValue && await NotIssuedTextAsync(trip.Id) is { } notIssued)
            return new TripActionResult(false, $"Kho chưa xác nhận xuất đủ hàng cho chuyến #{trip.Id} ({notIssued}) — chưa thể bắt đầu.");

        trip.Status = TripStatus.InProgress;
        trip.StartTime = DateTime.UtcNow;

        // CHỐT số liệu nhiên liệu lúc xuất phát (loại nhiên liệu, định mức của xe, đơn giá tham
        // chiếu ngày chạy) — báo cáo xăng / lợi nhuận của chuyến dùng số này, sửa xe hay bảng giá
        // về sau không làm đổi số của chuyến đã chạy.
        var fuelInfo = await _db.Vehicles.Where(v => v.Id == trip.VehicleId)
            .Select(v => new { v.FuelType, v.FuelConsumptionPer100Km }).FirstAsync();
        trip.FuelTypeAtStart = string.IsNullOrWhiteSpace(fuelInfo.FuelType) ? null : fuelInfo.FuelType.Trim();
        trip.FuelConsumptionPer100KmAtStart = fuelInfo.FuelConsumptionPer100Km;
        if (trip.FuelTypeAtStart is not null)
        {
            var refPrice = await FindFuelPriceAsync(trip.FuelTypeAtStart, null, trip.StartTime);
            trip.FuelUnitPriceAtStart = refPrice.Found ? refPrice.UnitPrice : null;
        }

        if (trip.TransportOrder.Status == TransportOrderStatus.Confirmed)
            trip.TransportOrder.Status = TransportOrderStatus.InProgress;

        await _db.SaveChangesAsync();
        await transaction.CommitAsync();
        return new TripActionResult(true, null);
    }

    /// <summary>Dispatcher tự bấm Hoàn tất trên Desktop = tự họ đã xác nhận/
    /// duyệt luôn (họ đang trực tiếp thao tác), nên hoàn tất NGAY — KHÔNG qua
    /// bước "Chờ xác nhận" (bước đó chỉ áp dụng khi TÀI XẾ tự báo qua app, xem
    /// ConfirmArrivalAsync + ApproveCompletionAsync).</summary>
    public async Task<TripActionResult> CompleteAsync(int id)
    {
        var trip = await LoadTripForArrivalAsync(id);
        if (trip is null) return new TripActionResult(false, "Không tìm thấy chuyến.");
        if (trip.Status != TripStatus.InProgress)
            return new TripActionResult(false, "Chỉ có thể hoàn tất chuyến đang ở trạng thái Đang chạy (InProgress).");

        return await FinalizeTripAsync(trip, TripStatus.InProgress);
    }

    /// <summary>Vận hành (Dispatcher) DUYỆT xác nhận "đã xong" của tài xế —
    /// PendingConfirmation -> Completed THẬT SỰ (lúc này mới xuất kho chuyến Trả hàng / hàng
    /// Lấy về chuyển sang "Chờ nhập kho",
    /// trả xe rảnh, tính tiến độ đơn — xem HandleArrivalAsync).</summary>
    public async Task<TripActionResult> ApproveCompletionAsync(int id)
    {
        var trip = await LoadTripForArrivalAsync(id);
        if (trip is null) return new TripActionResult(false, "Không tìm thấy chuyến.");
        if (trip.Status != TripStatus.PendingConfirmation)
            return new TripActionResult(false, "Chuyến này không ở trạng thái Chờ xác nhận.");

        return await FinalizeTripAsync(trip, TripStatus.PendingConfirmation);
    }

    /// <summary>Hoàn tất THẬT SỰ 1 chuyến (dùng chung cho Dispatcher tự Hoàn tất
    /// và Duyệt xác nhận của tài xế) — bọc trọn trong 1 transaction cùng các thao
    /// tác kho. Nếu nhập/xuất kho thất bại (VD kho hết chỗ, thiếu khu vực phù
    /// hợp), Rollback toàn bộ: chuyến GIỮ NGUYÊN trạng thái cũ, báo lỗi rõ ràng
    /// cho Dispatcher xử lý (thêm vị trí kho...) rồi bấm lại — thay vì như trước:
    /// chuyến thành "Hoàn tất" nhưng hàng không hề vào kho.</summary>
    private async Task<TripActionResult> FinalizeTripAsync(Trip trip, TripStatus expectedStatus)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync();

        // Chặn 2 thao tác hoàn tất chồng nhau (Dispatcher bấm Hoàn tất đúng lúc tài
        // xế bấm Đã xong, hoặc bấm Duyệt 2 lần) -> trước đây có thể nhập kho 2 lần.
        if (!await TryClaimStatusAsync(trip.Id, expectedStatus, TripStatus.Completed))
            return new TripActionResult(false, "Trạng thái chuyến vừa bị thay đổi bởi thao tác khác — hãy tải lại danh sách.");

        var error = await HandleArrivalAsync(trip);
        if (error is not null)
            return new TripActionResult(false, error); // chưa Commit -> tự Rollback khi dispose

        await _db.SaveChangesAsync();
        await transaction.CommitAsync();

        // Chuyến đã xử lý xong -> thông báo "chờ duyệt" của chuyến này tự đánh dấu đã đọc.
        await _notificationService.ResolveTripPendingAsync(trip.Id);

        // Báo CHUNG cho bộ phận kho: hàng đã về, cần nhập kho — trước đây chỉ người bấm Duyệt
        // thấy lời nhắc; quên nhập thì chặng Giao + xe + tài xế bị giữ mà không ai biết.
        if (trip.LegType == TripLegType.Pickup)
        {
            try
            {
                var waiting = await PendingReceiptRules.LoadAsync(_db, l => l.TripId == trip.Id);
                if (waiting.Count > 0)
                    await _notificationService.CreateBroadcastAsync(
                        $"📦 Chuyến #{trip.Id} đã về kho — chờ nhập kho",
                        $"Hàng của chuyến lấy #{trip.Id} (đơn {trip.TransportOrder.Code}) đã về kho, còn chờ nhập {waiting.Sum(w => w.QtyPending):0.##}. " +
                        "Vào màn Kho → 📦 Nhập hàng từ chuyến để nhập (thủ công hoặc Best-Fit); nhập xong thì xuất kho ở 📤 Xuất kho theo chuyến, chặng Giao mới bắt đầu được.");
            }
            catch { /* thông báo không phải dữ liệu chính — chuyến đã hoàn tất xong */ }
        }
        return new TripActionResult(true, trip.LegType == TripLegType.Pickup
            ? $"Chuyến lấy hàng #{trip.Id} đã hoàn tất — hàng đã về kho, đang CHỜ NHẬP KHO. " +
              "Vào màn Kho → 📦 Nhập hàng từ chuyến để nhập (thủ công hoặc Best-Fit); nhập xong thì xuất kho ở 📤 Xuất kho theo chuyến, chặng Giao mới bắt đầu được."
            : null);
    }

    /// <summary>Vận hành TỪ CHỐI xác nhận của tài xế (VD bấm nhầm) — đẩy
    /// chuyến quay lại Đang chạy (InProgress) để tài xế tiếp tục/báo lại cho
    /// đúng. KHÔNG đụng gì tới kho/xe (chưa có gì xảy ra ở bước này cả).</summary>
    public async Task<TripActionResult> RejectCompletionAsync(int id)
    {
        var trip = await _db.Trips.FirstOrDefaultAsync(t => t.Id == id);
        if (trip is null) return new TripActionResult(false, "Không tìm thấy chuyến.");
        if (trip.Status != TripStatus.PendingConfirmation)
            return new TripActionResult(false, "Chuyến này không ở trạng thái Chờ xác nhận.");

        // Chuyến chạy tiếp -> bỏ giờ kết thúc tài xế đã báo (ghi lại khi báo xong lần sau),
        // trong CÙNG câu UPDATE giành trạng thái. Không gán trip.Status trên bản đã tải
        // (tránh SaveChanges sau ghi đè trạng thái mới hơn).
        if (!await TryClaimStatusAsync(trip.Id, TripStatus.PendingConfirmation, TripStatus.InProgress, clearEndTime: true))
            return new TripActionResult(false, "Trạng thái chuyến vừa bị thay đổi bởi thao tác khác — hãy tải lại danh sách.");
        // Ảnh hàng lần trước không còn tính — lần xác nhận sau tài xế phải chụp ảnh MỚI.
        var rejectedAt = DateTime.UtcNow;
        await _db.Trips.Where(t => t.Id == trip.Id)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.ConfirmRejectedAt, rejectedAt));

        // Đã xử lý (từ chối) -> thông báo "chờ duyệt" cũ không còn việc gì để làm.
        // Nếu tài xế báo xong lại, hệ thống sẽ tạo thông báo MỚI.
        await _notificationService.ResolveTripPendingAsync(trip.Id);
        return new TripActionResult(true, null);
    }

    public async Task<TripActionResult> RecordFuelAsync(int id, RecordFuelRequest request)
    {
        var trip = await _db.Trips.Include(t => t.Vehicle).FirstOrDefaultAsync(t => t.Id == id);
        if (trip is null) return new TripActionResult(false, "Không tìm thấy chuyến.");
        if (request.Liters <= 0)
            return new TripActionResult(false, "Số lít phải lớn hơn 0.");
        if (request.Liters > 2000)
            return new TripActionResult(false, "Số lít quá lớn — kiểm tra lại (tối đa 2.000 lít/lần đổ).");

        // QUY TẮC CHUNG cho xăng / chi phí / sự cố: chỉ ghi cho chuyến ĐÃ TỪNG CHẠY
        // (có giờ bắt đầu) — kể cả chuyến bị hủy giữa đường (xe đã chạy, đã tốn xăng).
        // Chuyến chưa từng chạy (Planned, hoặc hủy khi chưa xuất phát) thì không.
        if (!HasRun(trip.Status, trip.StartTime))
            return new TripActionResult(false, "Chuyến này chưa từng chạy — không ghi nhận đổ xăng.");
        var fuelLock = ClosedCostLockReason(trip.Status, trip.EndTime, "đổ xăng");
        if (fuelLock is not null) return new TripActionResult(false, fuelLock);

        // Không đổ quá chỗ trống của bình (khi xe đã khai dung tích + mức đo) — chặn khai khống /
        // gõ nhầm số lít. Cho lệch 5% vì mức hiện có là ƯỚC TÍNH theo định mức + GPS.
        // Ghi BÙ cho chuyến đã kết thúc TRƯỚC lần đo mức nhiên liệu gần nhất: bình hiện tại không
        // liên quan tới lần đổ đó (đã đổ và đã tiêu trước lúc đo) -> không so với chỗ trống.
        var level = (await VehicleFuelLevelRules.ComputeAsync(_db, trip.VehicleId))[trip.VehicleId];
        var endedBeforeMeasure = trip.EndTime is DateTime ended && level.MeasuredAtUtc is DateTime measuredAt && ended <= measuredAt;
        if (!endedBeforeMeasure && level.TankCapacityLiters is decimal cap && level.FreeLiters is decimal free
            && request.Liters > free + cap * 0.05m)
            return new TripActionResult(false,
                $"Đổ {request.Liters:0.#} lít nhưng bình xe {trip.Vehicle.PlateNumber} ước tính chỉ còn trống ~{free:0.#} lít " +
                $"(dung tích {cap:0.#} L, đang có ~{level.CurrentLiters:0.#} L). Kiểm tra lại số lít, hoặc cập nhật mức nhiên liệu thực tế của xe (màn Đội xe → ⛽ Nhiên liệu).");

        decimal cost;
        string? stationName = string.IsNullOrWhiteSpace(request.StationName) ? null : request.StationName.Trim();
        if (request.AutoPrice)
        {
            // Đơn giá lấy TỰ ĐỘNG từ bảng giá nhiên liệu (màn Nhiên liệu) — chỉ
            // nhập số lít, không nhập tiền tay (tránh gõ sai / khai khống).
            // Giá theo NGÀY BẮT ĐẦU chuyến — CÙNG mốc với nút So sánh xăng và báo cáo. Trước
            // đây lấy theo giờ kết thúc (đang chạy thì hôm nay): chuyến nhiều ngày, giá đổi giữa
            // chừng thì các lần đổ và định mức lấy giá 2 ngày khác nhau -> lệch giả; Từ chối xác
            // nhận còn xóa giờ kết thúc làm mốc giá nhảy lại. Giờ bắt đầu thì cố định.
            // Không chọn hãng + đã CHỐT giá lúc xuất phát -> dùng đúng giá đó (cùng giá báo cáo /
            // So sánh xăng dùng); giá nhập thêm trong ngày hay nhập lùi không làm lệch.
            var quote = request.FuelProviderId is null && trip.FuelUnitPriceAtStart is decimal pinned
                ? new FuelPriceLookup(true, pinned, null, "Giá thị trường (chốt lúc xuất phát)", null)
                : await FindFuelPriceAsync(trip.FuelTypeAtStart ?? trip.Vehicle.FuelType, request.FuelProviderId, trip.StartTime);
            if (!quote.Found) return new TripActionResult(false, quote.Message);
            cost = Math.Round(request.Liters * quote.UnitPrice, 0);

            var label = quote.SourceText ?? "";
            stationName = stationName is null ? label : $"{label} — {stationName}";
        }
        else
        {
            if (request.Cost <= 0) return new TripActionResult(false, "Chi phí phải lớn hơn 0.");
            if (request.Cost > MaxMoney) return new TripActionResult(false, $"Chi phí quá lớn (tối đa {MaxMoney:N0} đ).");
            cost = request.Cost;
        }

        _db.FuelLogs.Add(new FuelLog
        {
            TripId = id,
            Liters = request.Liters,
            Cost = cost,
            StationName = stationName,
        });

        await _db.SaveChangesAsync();
        // Tổng tiền xăng của chuyến = TÍNH LẠI từ các lần đổ (1 câu UPDATE) — trước đây
        // cộng dồn "+=" trên bản đã đọc: 2 lần ghi cùng lúc thì mất 1 lần, lệch với báo cáo.
        await RecalculateActualFuelCostAsync(id);
        return new TripActionResult(true, null);
    }

    public async Task<List<FuelLogDto>> GetFuelLogsAsync(int tripId)
    {
        var rows = await _db.FuelLogs
            .Where(f => f.TripId == tripId)
            .OrderBy(f => f.Timestamp)
            .Select(f => new { f.Id, f.Timestamp, f.Liters, f.Cost, f.StationName })
            .ToListAsync();

        return rows.Select(f => new FuelLogDto(
                f.Id, DateTime.SpecifyKind(f.Timestamp, DateTimeKind.Utc), f.Liters,
                f.Liters > 0 ? Math.Round(f.Cost / f.Liters, 0) : 0, f.Cost, f.StationName))
            .ToList();
    }

    public async Task<FuelPriceQuoteDto> GetFuelPriceQuoteAsync(int tripId, int? fuelProviderId)
    {
        var trip = await _db.Trips
            .Where(t => t.Id == tripId)
            .Select(t => new { t.Status, t.StartTime, t.EndTime, FuelType = t.FuelTypeAtStart ?? t.Vehicle.FuelType, t.Vehicle.PlateNumber, t.FuelUnitPriceAtStart, t.VehicleId })
            .FirstOrDefaultAsync();
        if (trip is null)
            return new FuelPriceQuoteDto(false, false, null, 0, null, null, "Không tìm thấy chuyến.");

        var canRecord = HasRun(trip.Status, trip.StartTime);
        var quote = fuelProviderId is null && trip.FuelUnitPriceAtStart is decimal pinned
            ? new FuelPriceLookup(true, pinned, null, "Giá thị trường (chốt lúc xuất phát)", null)
            : await FindFuelPriceAsync(trip.FuelType, fuelProviderId, trip.StartTime); // cùng mốc với lúc ghi (ngày bắt đầu)
        var message = !canRecord
            ? (trip.Status == TripStatus.Cancelled
                ? "Chuyến đã hủy khi chưa xuất phát — không có đổ xăng."
                : "Chuyến chưa bắt đầu chạy — chưa ghi đổ xăng được.")
            : quote.Message;
        var level = (await VehicleFuelLevelRules.ComputeAsync(_db, trip.VehicleId))[trip.VehicleId];
        return new FuelPriceQuoteDto(canRecord, quote.Found, trip.FuelType, quote.UnitPrice, quote.EffectiveDate, quote.SourceText, message,
            level.TankCapacityLiters, level.CurrentLiters);
    }

    public async Task<TripActionResult> DeleteFuelLogAsync(int fuelLogId)
    {
        var log = await _db.FuelLogs.Include(f => f.Trip).FirstOrDefaultAsync(f => f.Id == fuelLogId);
        if (log is null) return new TripActionResult(false, "Không tìm thấy lần đổ xăng này.");
        var fuelLock = ClosedCostLockReason(log.Trip.Status, log.Trip.EndTime, "đổ xăng");
        if (fuelLock is not null) return new TripActionResult(false, fuelLock);

        var tripId = log.TripId;
        _db.FuelLogs.Remove(log);
        await _db.SaveChangesAsync();
        await RecalculateActualFuelCostAsync(tripId);
        return new TripActionResult(true, null);
    }

    /// <summary>Đặt Trip.ActualFuelCost = TỔNG tiền các lần đổ xăng của chuyến, tính
    /// ngay trong database bằng 1 câu UPDATE (không đọc-sửa-ghi) — luôn khớp với báo cáo.
    /// Viết SQL trực tiếp (có tham số, không nối chuỗi): EF Core KHÔNG dịch được tổng qua
    /// navigation (t.FuelLogs.Sum) bên trong ExecuteUpdate/SetProperty -> trước đây lỗi 500
    /// "could not be translated" NGAY SAU khi đã lưu lần đổ xăng (lần đổ vẫn được lưu, nhưng
    /// tổng tiền xăng của chuyến không cập nhật và Desktop báo lỗi).</summary>
    private Task<int> RecalculateActualFuelCostAsync(int tripId)
        => _db.Database.ExecuteSqlInterpolatedAsync($@"
UPDATE ""Trips""
SET ""ActualFuelCost"" = COALESCE((SELECT SUM(f.""Cost"") FROM ""FuelLogs"" f WHERE f.""TripId"" = {tripId}), 0)
WHERE ""Id"" = {tripId}");

    private sealed record FuelPriceLookup(bool Found, decimal UnitPrice, DateTime? EffectiveDate, string? SourceText, string? Message);

    /// <summary>Đơn giá MỚI NHẤT (có hiệu lực tới hôm nay, giờ VN) của đúng loại
    /// nhiên liệu xe dùng. Chọn hãng -> giá của hãng đó; hãng chưa có giá cho loại
    /// nhiên liệu này -> dùng giá thị trường chung (FuelProviderId = null).</summary>
    private async Task<FuelPriceLookup> FindFuelPriceAsync(string? vehicleFuelType, int? fuelProviderId, DateTime? atUtc = null)
    {
        if (string.IsNullOrWhiteSpace(vehicleFuelType))
            return new FuelPriceLookup(false, 0, null, null,
                "Xe của chuyến này CHƯA khai báo loại nhiên liệu — vào Sửa xe để chọn loại nhiên liệu trước.");

        var fuelType = vehicleFuelType.Trim().ToLower();
        // Giá có hiệu lực tới HẾT NGÀY (giờ VN) của thời điểm cần tính — mặc định hôm nay.
        // Dùng CHUNG cho: ghi đổ xăng, nút "Ước tính nhiên liệu", báo cáo nhiên liệu.
        var todayVn = (atUtc ?? DateTime.UtcNow).AddHours(7).Date.AddDays(1).AddTicks(-1);

        string? providerName = null;
        if (fuelProviderId.HasValue)
        {
            providerName = await _db.FuelProviders.Where(p => p.Id == fuelProviderId.Value).Select(p => p.Name).FirstOrDefaultAsync();
            if (providerName is null)
                return new FuelPriceLookup(false, 0, null, null, "Hãng nhiên liệu đã chọn không tồn tại.");

            var providerPrice = await _db.FuelPriceHistories
                .Where(p => p.FuelProviderId == fuelProviderId && p.FuelType.Trim().ToLower() == fuelType && p.EffectiveDate <= todayVn)
                .OrderByDescending(p => p.EffectiveDate).ThenByDescending(p => p.Id)
                .Select(p => new { p.Price, p.EffectiveDate })
                .FirstOrDefaultAsync();
            if (providerPrice is not null)
                return new FuelPriceLookup(true, providerPrice.Price, providerPrice.EffectiveDate, providerName, null);
        }

        var marketPrice = await _db.FuelPriceHistories
            .Where(p => p.FuelProviderId == null && p.FuelType.Trim().ToLower() == fuelType && p.EffectiveDate <= todayVn)
            .OrderByDescending(p => p.EffectiveDate).ThenByDescending(p => p.Id)
            .Select(p => new { p.Price, p.EffectiveDate })
            .FirstOrDefaultAsync();
        if (marketPrice is not null)
        {
            var note = providerName is null ? null
                : $"{providerName} chưa có giá '{vehicleFuelType}' — đang dùng giá thị trường chung.";
            return new FuelPriceLookup(true, marketPrice.Price, marketPrice.EffectiveDate,
                providerName is null ? "Giá thị trường" : $"{providerName} (giá thị trường)", note);
        }

        // Không chọn hãng và chưa có giá thị trường chung -> lấy giá mới nhất của bất kỳ hãng nào.
        if (providerName is null)
        {
            var anyPrice = await _db.FuelPriceHistories
                .Where(p => p.FuelType.Trim().ToLower() == fuelType && p.EffectiveDate <= todayVn)
                .OrderByDescending(p => p.EffectiveDate).ThenByDescending(p => p.Id)
                .Select(p => new { p.Price, p.EffectiveDate, ProviderName = p.FuelProvider != null ? p.FuelProvider.Name : null })
                .FirstOrDefaultAsync();
            if (anyPrice is not null)
                return new FuelPriceLookup(true, anyPrice.Price, anyPrice.EffectiveDate,
                    anyPrice.ProviderName is null ? "Giá thị trường" : $"{anyPrice.ProviderName} (giá mới nhất)", null);
        }

        return new FuelPriceLookup(false, 0, null, null,
            providerName is null
                ? $"Chưa có giá cho '{vehicleFuelType}' — vào màn Nhiên liệu để thêm giá."
                : $"Chưa có giá '{vehicleFuelType}' của {providerName} (và chưa có giá thị trường chung) — vào màn Nhiên liệu để thêm giá.");
    }

    // ================= App tài xế (đã có từ trước) =================

    public async Task<ServiceResult> PingAsync(int tripId, GpsPingRequest request)
    {
        var trip = await _db.Trips.FirstOrDefaultAsync(t => t.Id == tripId);
        if (trip is null) return new ServiceResult(false, "Không tìm thấy chuyến.");

        // Điểm GPS sai (ngoài phạm vi, hoặc 0,0 khi máy chưa bắt được vệ tinh) -> bỏ qua:
        // 1 điểm sai làm "km GPS" trong báo cáo nhảy vọt, kéo theo cờ lệch nhiên liệu.
        if (double.IsNaN(request.Lat) || double.IsNaN(request.Lng)
            || request.Lat < -90 || request.Lat > 90 || request.Lng < -180 || request.Lng > 180
            || (request.Lat == 0 && request.Lng == 0))
            return new ServiceResult(false, "Tọa độ GPS không hợp lệ — bỏ qua điểm này.");

        // Chỉ ghi GPS khi chuyến THẬT SỰ đang chạy — chuyến đã báo Hoàn tất
        // (Chờ xác nhận), đã Hoàn tất hẳn, hay đã Hủy đều KHÔNG ghi tiếp, tránh
        // phình database vô ích (app tài xế vẫn có thể lỡ gửi thêm vài điểm
        // cuối trước khi nhận ra, không coi là lỗi nghiêm trọng).
        if (trip.Status != TripStatus.Planned && trip.Status != TripStatus.InProgress)
            return new ServiceResult(false, "Chuyến này không còn ghi nhận GPS nữa.");

        // GPS ĐẦU TIÊN của chuyến còn "Đã lên kế hoạch" = tài xế bắt đầu chạy.
        // Trước đây chỉ đổi trạng thái sang Đang chạy -> chặng Giao có thể chạy
        // khi chặng Lấy hàng chưa xong, và BỎ QUA bước xuất kho (tồn kho sai).
        // Giờ đi qua ĐÚNG quy trình StartAsync: kiểm tra điều kiện (chặng Giao: kho phải đã xuất đủ).
        if (trip.Status == TripStatus.Planned)
        {
            var start = await StartAsync(tripId);
            if (!start.Success)
            {
                // Ping khác (gửi cùng lúc) đã bắt đầu chuyến rồi -> vẫn ghi điểm GPS này.
                var nowStatus = await _db.Trips.Where(t => t.Id == tripId).Select(t => t.Status).FirstAsync();
                if (nowStatus != TripStatus.InProgress) return new ServiceResult(false, start.Message);
            }
        }

        // Giờ ping do điện thoại gửi: chuẩn hóa về UTC (DB lưu UTC). Giờ lệch xa
        // (máy chỉnh sai giờ, gửi giờ địa phương không kèm múi giờ) -> dùng giờ
        // server, tránh làm sai thứ tự điểm GPS (vẽ lộ trình + tính quãng đường).
        var pingTime = DateTime.UtcNow;
        if (request.Timestamp is { } ts)
        {
            var utc = ts.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(ts, DateTimeKind.Utc) : ts.ToUniversalTime();
            if (utc <= DateTime.UtcNow.AddMinutes(5) && utc >= DateTime.UtcNow.AddHours(-24))
                pingTime = utc;
        }

        _db.Waypoints.Add(new Waypoint
        {
            TripId = tripId,
            Lat = request.Lat,
            Lng = request.Lng,
            SpeedKmh = request.SpeedKmh,
            Accuracy = request.Accuracy,
            Timestamp = pingTime,
        });

        await _db.SaveChangesAsync();
        return new ServiceResult(true, null);
    }

    /// <summary>Tài xế báo "đã xong" qua app — KHÔNG hoàn tất ngay, chỉ
    /// chuyển sang PendingConfirmation (Chờ xác nhận) và tạo thông báo CHUNG
    /// cho Vận hành biết để vào duyệt (ApproveCompletionAsync/RejectCompletionAsync
    /// trên Desktop) — lúc đó mới THẬT SỰ tính là xong (nhập/xuất kho, trả xe
    /// rảnh...). GPS cũng ngừng ghi ngay từ đây (xem PingAsync).</summary>
    public async Task<ServiceResult> ConfirmArrivalAsync(int tripId, ConfirmArrivalRequest request)
    {
        var trip = await _db.Trips
            .Include(t => t.TransportOrder)
            .Include(t => t.DriverEmployee)
            .FirstOrDefaultAsync(t => t.Id == tripId);
        if (trip is null) return new ServiceResult(false, "Không tìm thấy chuyến.");
        if (trip.Status != TripStatus.InProgress)
            return new ServiceResult(false, "Chuyến này không ở trạng thái đang chạy.");

        // KIỂM HÀNG: tài xế phải tick ĐỦ mọi dòng hàng (tên + số lượng hiện sẵn trên app)
        // và chụp ít nhất 1 ảnh hàng. Đối tác bắt buộc giao đủ — thiếu thì không xác nhận
        // được, tài xế gọi Điều phối (hệ thống không xử lý giao thiếu).
        var tripCargoIds = await _db.TripLines.Where(l => l.TripId == tripId).Select(l => l.CargoLineId).Distinct().ToListAsync();
        var confirmed = request.ConfirmedCargoLineIds ?? new List<int>();
        if (request.ConfirmedCargoLineIds is null)
            return new ServiceResult(false, "App tài xế đang là bản cũ (chưa có bước kiểm hàng + chụp ảnh) — hãy cập nhật app.");
        var missing = tripCargoIds.Except(confirmed).ToList();
        if (missing.Count > 0)
            return new ServiceResult(false, $"Chưa xác nhận đủ {missing.Count} dòng hàng — kiểm đếm và tick đủ từng dòng. Hàng thiếu thì KHÔNG xác nhận, liên hệ Điều phối.");
        var photoSince = trip.ConfirmRejectedAt ?? DateTime.MinValue; // chỉ ảnh chụp sau lần bị từ chối gần nhất
        var photoCount = await _db.TripCargoPhotos.CountAsync(p => p.TripId == tripId && p.CreatedAt > photoSince);
        if (photoCount < TruckingSystem.Application.TripPhotos.TripPhotoService.MinPhotos)
            return new ServiceResult(false, "Cần chụp ít nhất 1 ảnh hàng hóa trước khi xác nhận.");

        // Atomic: nếu Dispatcher vừa bấm Hoàn tất cùng lúc thì KHÔNG ghi đè
        // Completed thành Chờ xác nhận (trước đây gây nhập kho 2 lần khi Duyệt).
        // Giờ kết thúc = lúc TÀI XẾ BÁO ĐẾN NƠI (không phải lúc Vận hành duyệt, có thể
        // trễ cả ngày) — dùng cho thời lượng chuyến, ngày lấy giá xăng, khung giờ video.
        // Ghi CÙNG câu UPDATE giành trạng thái (không tách 2 lệnh).
        if (!await TryClaimStatusAsync(trip.Id, TripStatus.InProgress, TripStatus.PendingConfirmation, endTime: DateTime.UtcNow))
            return new ServiceResult(false, "Chuyến này vừa được Vận hành xử lý — không cần báo xong nữa.");
        // KHÔNG gán trip.Status trên bản đã tải: lần SaveChanges sau (tạo thông báo) sẽ
        // ghi đè lại "Chờ xác nhận" nếu Vận hành vừa Duyệt xong trong tích tắc đó.

        var legText = trip.LegType == TripLegType.Pickup ? "lấy hàng" : trip.LegType == TripLegType.Return ? "trả hàng" : "giao hàng";
        await _notificationService.CreateBroadcastAsync(
            $"🟢 Tài xế báo xong chuyến #{trip.Id} — chờ duyệt",
            $"Tài xế {trip.DriverEmployee.FullName} báo đã {legText} xong cho đơn {trip.TransportOrder.Code} (chuyến #{trip.Id}), " +
            $"đã xác nhận đủ {tripCargoIds.Count} dòng hàng kèm {photoCount} ảnh. " +
            $"Vào màn Quản lý Chuyến để xem ảnh và Duyệt." +
            (string.IsNullOrWhiteSpace(request.Note) ? "" : $" Ghi chú: {request.Note.Trim()}"));

        return new ServiceResult(true, null);
    }

    /// <summary>Trạng thái chuyến dành cho app tài xế — app hỏi TRƯỚC khi bật
    /// theo dõi GPS: chuyến đã báo xong / đã hoàn thành / đã hủy thì không cho
    /// bật nữa; chặng Giao mà chặng Lấy hàng chưa xong thì báo phải chờ.</summary>
    public async Task<DriverTripStatusDto?> GetDriverStatusAsync(int tripId)
    {
        var trip = await _db.Trips.FirstOrDefaultAsync(t => t.Id == tripId);
        if (trip is null) return null;

        var legText = trip.LegType switch
        {
            TripLegType.Pickup => "lấy hàng",
            TripLegType.Return => "trả hàng",
            _ => "giao hàng",
        };

        bool canTrack = true;
        string? message = null;
        switch (trip.Status)
        {
            case TripStatus.Completed:
                canTrack = false;
                message = $"Chuyến #{trip.Id} đã hoàn thành ({legText} xong).";
                break;
            case TripStatus.PendingConfirmation:
                canTrack = false;
                message = $"Chuyến #{trip.Id} đã báo {legText} xong, đang chờ Vận hành duyệt — không cần theo dõi nữa.";
                break;
            case TripStatus.Cancelled:
                canTrack = false;
                message = $"Chuyến #{trip.Id} đã bị hủy.";
                break;
            case TripStatus.Planned:
                message = await GetStartBlockReasonAsync(trip);
                canTrack = message is null;
                break;
        }

        return new DriverTripStatusDto(trip.Id, trip.Status.ToString(), trip.LegType.ToString(), canTrack, message);
    }

    /// <summary>Lý do KHÔNG được bắt đầu chuyến (null = được phép). Chặng Giao
    /// phụ thuộc hàng đã THẬT SỰ nằm trong kho: mọi chặng Lấy hàng (không tính
    /// chặng đã hủy) của đơn phải Hoàn tất — "Chờ xác nhận" chưa tính là xong vì
    /// Vận hành chưa duyệt thì hàng chưa về kho; đã duyệt thì hàng còn phải được NHẬP
    /// KHO đủ (màn Hàng chờ nhập) mới xuất đi giao được. Đơn khách tự mang hàng
    /// tới kho (không có chặng Lấy hàng) thì không bị chặn.</summary>
    private async Task<string?> GetStartBlockReasonAsync(Trip trip)
    {
        // Kiểm tra LẠI xe + tài xế lúc xuất phát — Chia xe có thể đã từ nhiều
        // ngày trước, trong thời gian chờ: xe bị chuyển Bảo trì/Ngưng hoạt động,
        // Đăng kiểm hoặc bằng lái hết hạn, tài xế nghỉ việc... (trước đây chỉ
        // kiểm tra 1 lần lúc Chia xe, chuyến vẫn bắt đầu được).
        var resourceReason = await GetVehicleDriverBlockReasonAsync(trip);
        if (resourceReason is not null) return resourceReason;

        if (trip.LegType != TripLegType.Delivery) return null;

        var pendingPickups = await _db.Trips
            .Where(t => t.TransportOrderId == trip.TransportOrderId
                        && t.LegType == TripLegType.Pickup
                        && t.Status != TripStatus.Cancelled
                        && t.Status != TripStatus.Completed)
            .OrderBy(t => t.Id)
            .Select(t => new { t.Id, t.Status })
            .ToListAsync();
        if (pendingPickups.Count > 0)
        {
            var list = string.Join(", ", pendingPickups.Select(p => $"#{p.Id} ({PickupStatusText(p.Status)})"));
            return $"Chưa thể bắt đầu chuyến giao hàng — đơn còn chuyến lấy hàng chưa hoàn tất: {list}. " +
                   "Hàng phải được lấy về (Vận hành duyệt), nhập kho và kho xác nhận xuất xong mới được đi giao.";
        }

        // Chỉ chặn khi tồn kho THẬT SỰ chưa đủ cho chính chuyến giao này — trước đây chặn cả đơn,
        // xe + tài xế bị giữ vô ích. Hàng cùng dòng hàng của 1 đơn là hàng GIỐNG HỆT nhau (tồn kho
        // tính theo dòng hàng, không theo xe): chuyến giao nào xuất phát trước dùng hàng đã nhập
        // trước; chuyến còn lại chờ phần hàng còn chờ nhập — tổng hàng giao đi vẫn đúng.
        var stockReason = await GetStockBlockReasonAsync(trip);
        if (stockReason is null)
        {
            // Chuyến dữ liệu cũ không gắn kho: không có gì để xuất — không chặn.
            if (!trip.WarehouseId.HasValue) return null;
            // Đủ hàng trong kho nhưng KHO CHƯA XÁC NHẬN XUẤT cho chuyến này (soạn hàng + bốc lên xe).
            var notIssued = await NotIssuedTextAsync(trip.Id);
            return notIssued is null ? null
                : $"Kho chưa xác nhận XUẤT KHO cho chuyến giao #{trip.Id} ({notIssued}). Vận hành vào màn Kho → 📤 Xuất kho theo chuyến, " +
                  "soạn hàng lên xe rồi bấm Xác nhận xuất — xong mới bắt đầu chuyến được.";
        }

        // Thiếu hàng vì xe lấy hàng đã về kho nhưng hàng CHƯA lên kệ (nhập kho là bước riêng của
        // Vận hành) -> báo rõ thay vì câu chung "kho không đủ hàng".
        var orderId = trip.TransportOrderId;
        var waitingReceipt = await PendingReceiptRules.LoadAsync(_db, l => l.Trip.TransportOrderId == orderId);
        if (waitingReceipt.Count > 0)
            return "Chưa thể bắt đầu chuyến giao hàng — hàng của chuyến lấy " + PendingReceiptRules.Describe(waitingReceipt) +
                   " đã về kho nhưng CHƯA NHẬP KHO. Vận hành nhập kho trước (màn Kho → 📦 Nhập hàng từ chuyến: thủ công hoặc Best-Fit) rồi mới xuất kho đi giao.";
        return stockReason;
    }

    /// <summary>Các dòng hàng của chuyến giao CHƯA được kho xuất đủ ("Hàng A: còn 5; Hàng B: còn 2"), null = đã xuất đủ.</summary>
    private async Task<string?> NotIssuedTextAsync(int tripId)
    {
        var lines = await _db.TripLines.Where(l => l.TripId == tripId)
            .Select(l => new { l.CargoLineId, l.QtyPlanned, l.CargoLine.CargoType }).ToListAsync();
        var issued = await _db.GoodsIssues.Where(g => g.TripId == tripId && g.ReversedAt == null)
            .GroupBy(g => g.CargoLineId).Select(g => new { CargoLineId = g.Key, Qty = g.Sum(x => x.QtyIssued) })
            .ToDictionaryAsync(x => x.CargoLineId, x => x.Qty);
        var missing = lines
            .Select(l => (l.CargoType, Need: l.QtyPlanned - (issued.TryGetValue(l.CargoLineId, out var q) ? q : 0)))
            .Where(x => x.Need > 0).ToList();
        return missing.Count == 0 ? null : string.Join("; ", missing.Select(m => $"\"{m.CargoType}\" còn {m.Need:0.##}"));
    }

    /// <summary>Chặng Giao: kho phải còn ĐỦ hàng để xuất cho chuyến — kiểm tra NGAY ở
    /// bước app hỏi trạng thái (trước khi bật GPS). Trước đây chỉ phát hiện lúc ping
    /// đầu tiên: bắt đầu thất bại âm thầm, mọi điểm GPS bị bỏ, video kẹt hàng đợi.</summary>
    private async Task<string?> GetStockBlockReasonAsync(Trip trip)
    {
        if (trip.LegType != TripLegType.Delivery || !trip.WarehouseId.HasValue) return null;
        var warehouseId = trip.WarehouseId.Value;

        var lines = await _db.TripLines
            .Where(l => l.TripId == trip.Id)
            .Select(l => new { l.CargoLineId, l.QtyPlanned, l.CargoLine.CargoType })
            .ToListAsync();
        foreach (var line in lines)
        {
            var issued = await _db.GoodsIssues
                .Where(g => g.TripId == trip.Id && g.CargoLineId == line.CargoLineId && g.ReversedAt == null)
                .SumAsync(g => (decimal?)g.QtyIssued) ?? 0m;
            var need = line.QtyPlanned - issued;
            if (need <= 0) continue;
            var onHand = await _db.WarehouseStocks
                .Where(s => s.WarehouseId == warehouseId && s.CargoLineId == line.CargoLineId)
                .SumAsync(s => (decimal?)s.QtyOnHand) ?? 0m;
            if (onHand < need)
                return $"Kho không đủ hàng cho chuyến #{trip.Id} — \"{line.CargoType}\": cần xuất {need:0.##}, kho còn {onHand:0.##}. Báo Vận hành kiểm tra tồn kho trước khi xuất phát.";
        }
        return null;
    }

    private async Task<string?> GetVehicleDriverBlockReasonAsync(Trip trip)
    {
        // So theo NGÀY Việt Nam, CÙNG quy tắc với Chia xe (VehicleDocumentRules) — trước
        // đây lúc xuất phát chỉ xét Đăng kiểm, bỏ sót Bảo hiểm / Giấy phép vận tải và
        // ngày làm của tài xế (chặng Giao thường chạy sau Chia xe vài ngày).
        var today = VnClock.Today;
        // Xét ĐỦ mọi ngày CHẶNG NÀY dự kiến chạy (giống Chia xe / Đổi xe): số ngày = thời lượng
        // ước tính / 8 giờ lái, chưa có ước tính tính 1 ngày. Chặng Giao giờ hay chạy SAU Chia xe
        // vài ngày (chờ nhập kho) -> lần kiểm tra lúc Chia xe đã không còn đúng ngày.
        var tripDays = Math.Max(1, (int)Math.Ceiling((trip.EstimatedDurationMinutes ?? 0) / 480.0));
        var lastDay = today.AddDays(tripDays - 1);
        var daysText = tripDays > 1 ? $" tới hết ngày {lastDay:dd/MM/yyyy} (chặng dự kiến {tripDays} ngày)" : "";

        var vehicle = await _db.Vehicles
            .Where(v => v.Id == trip.VehicleId)
            .Select(v => new { v.PlateNumber, v.Status, v.CapacityKg, v.VehicleType })
            .FirstOrDefaultAsync();
        if (vehicle is null) return "Không tìm thấy xe của chuyến này.";
        if (vehicle.Status == VehicleStatus.UnderMaintenance)
            return $"Xe {vehicle.PlateNumber} đang Bảo trì — chưa thể xuất phát.";
        if (vehicle.Status == VehicleStatus.OutOfService)
            return $"Xe {vehicle.PlateNumber} đang Ngưng hoạt động — chưa thể xuất phát.";

        var docs = await VehicleDocumentRules.GetLatestAsync(_db, trip.VehicleId);
        var docReason = VehicleDocumentRules.BlockReason(docs, vehicle.PlateNumber, lastDay);
        if (docReason is not null)
            return VehicleDocumentRules.BlockReason(docs, vehicle.PlateNumber, today) // đã hết hạn ngay hôm nay
                   ?? $"Giấy tờ xe {vehicle.PlateNumber} hết hạn TRƯỚC KHI chặng này chạy xong{daysText}: {docReason}";

        var driver = await _db.DriverProfiles
            .Where(dp => dp.EmployeeId == trip.DriverEmployeeId)
            .Select(dp => new {
                dp.LicenseExpiryDate,
                dp.LicenseClass,
                dp.HealthCertExpiryDate,
                EmpStatus = dp.Employee.Status,
                dp.Employee.FullName,
                PositionName = dp.Employee.Position != null ? dp.Employee.Position.Name : null
            })
            .FirstOrDefaultAsync();
        if (driver is null) return "Tài xế của chuyến này chưa có hồ sơ tài xế.";
        if (driver.EmpStatus != EmployeeStatus.Active)
            return $"Tài xế {driver.FullName} hiện không ở trạng thái Đang làm việc — chưa thể xuất phát.";
        if (!DriverPositionRules.IsDriverPosition(driver.PositionName))
            return $"Tài xế {driver.FullName} hiện có chức danh \"{driver.PositionName ?? "(trống)"}\" — không phải Tài xế, chưa thể xuất phát. Đổi tài xế khác cho chuyến (nút Đổi xe/tài xế).";
        var licenseBlock = DriverLicenseRules.BlockReason(driver.LicenseClass, vehicle.CapacityKg, vehicle.VehicleType);
        if (licenseBlock is not null)
            return $"Tài xế {driver.FullName} không lái được xe {vehicle.PlateNumber}: {licenseBlock} — đổi tài xế / xe trước khi xuất phát.";
        if (driver.LicenseExpiryDate.Date < lastDay)
            return $"Bằng lái của tài xế {driver.FullName} hết hạn ({driver.LicenseExpiryDate:dd/MM/yyyy}) trước khi chặng này xong{daysText} — chưa thể xuất phát.";
        if (driver.HealthCertExpiryDate is DateTime healthCert && healthCert.Date < lastDay)
            return $"Giấy khám sức khỏe của tài xế {driver.FullName} hết hạn ({healthCert:dd/MM/yyyy}) trước khi chặng này xong{daysText} — chưa thể xuất phát.";

        // Tài xế phải ĐĂNG KÝ NGÀY LÀM đủ mọi ngày chặng chạy (giống điều kiện Chia xe).
        var afterLastDay = lastDay.AddDays(1);
        var workDays = (await _db.DriverSchedules
                .Where(s => s.DriverEmployeeId == trip.DriverEmployeeId && s.ShiftDate >= today && s.ShiftDate < afterLastDay)
                .Select(s => s.ShiftDate)
                .ToListAsync())
            .Select(d => d.Date).ToHashSet();
        var missingDays = Enumerable.Range(0, tripDays).Select(i => today.AddDays(i)).Where(d => !workDays.Contains(d)).ToList();
        if (missingDays.Count > 0)
            return $"Tài xế {driver.FullName} chưa đăng ký ngày làm {string.Join(", ", missingDays.Select(d => d.ToString("dd/MM/yyyy")))}" +
                   $"{(tripDays > 1 ? $" (chặng dự kiến {tripDays} ngày)" : "")} — đăng ký trong màn Tài xế trước khi xuất phát.";

        return null;
    }

    private static string PickupStatusText(TripStatus s) => s switch
    {
        TripStatus.Planned => "chưa chạy",
        TripStatus.InProgress => "đang chạy",
        TripStatus.PendingConfirmation => "chờ Vận hành duyệt",
        _ => s.ToString(),
    };

    /// <summary>Mỗi chuyến đã nhập (Lấy hàng) / xuất (Giao, Trả hàng) kho được
    /// bao nhiêu so với kế hoạch — để màn Chuyến hiện cột "Kho" nhìn là biết.</summary>
    public async Task<List<TripWarehouseProgressDto>> GetWarehouseProgressAsync()
    {
        var trips = await _db.Trips
            .Select(t => new { t.Id, t.LegType, Planned = t.TripLines.Sum(tl => tl.QtyPlanned) })
            .ToListAsync();
        var received = await _db.GoodsReceipts
            .Where(r => r.TripId != null)
            .GroupBy(r => r.TripId!.Value)
            .Select(g => new { TripId = g.Key, Qty = g.Sum(x => x.QtyReceived) })
            .ToDictionaryAsync(x => x.TripId, x => x.Qty);
        var issued = await _db.GoodsIssues
            .Where(i => i.TripId != null && i.ReversedAt == null)
            .GroupBy(i => i.TripId!.Value)
            .Select(g => new { TripId = g.Key, Qty = g.Sum(x => x.QtyIssued) })
            .ToDictionaryAsync(x => x.TripId, x => x.Qty);

        return trips.Select(t =>
        {
            var source = t.LegType == TripLegType.Pickup ? received : issued;
            return new TripWarehouseProgressDto(t.Id, t.LegType.ToString(), t.Planned, source.TryGetValue(t.Id, out var q) ? q : 0);
        }).ToList();
    }

    public async Task<List<WaypointDto>> GetWaypointsAsync(int tripId)
    {
        return await _db.Waypoints
            .Where(w => w.TripId == tripId)
            .OrderBy(w => w.Timestamp)
            .Select(w => new WaypointDto(w.Lat, w.Lng, w.Timestamp, w.SpeedKmh))
            .ToListAsync();
    }

    public async Task<TripRouteDto?> GetTripRouteAsync(int tripId)
    {
        var trip = await _db.Trips
            .Include(t => t.TransportOrder).ThenInclude(o => o.PickupLocation)
            .Include(t => t.TransportOrder).ThenInclude(o => o.DropoffLocation)
            .Include(t => t.Warehouse)
            .FirstOrDefaultAsync(t => t.Id == tripId);

        if (trip is null) return null;

        var stops = BuildRouteStops(trip);
        return new TripRouteDto(trip.TransportOrder.Code, trip.LegType.ToString(), stops);
    }

    /// <summary>Dựng danh sách điểm dừng ĐÚNG theo chặng — dùng CHUNG cho cả
    /// "Xem lộ trình" (GetTripRouteAsync) lẫn tính+lưu ước tính
    /// (CalculateAndSaveRouteEstimateAsync), tránh viết trùng lặp logic.
    /// Chặng Lấy hàng có 3 điểm (round-trip Kho→Đối tác→Kho, xe phải quay về
    /// đúng kho); chặng Giao hàng chỉ 2 điểm (1 chiều, xong việc cho đơn này);
    /// chặng Trả hàng (Return) cũng 2 điểm nhưng ĐẢO HƯỚNG — từ Kho trả NGƯỢC
    /// lại đúng đối tác đã lấy hàng ban đầu (PickupLocation, không phải
    /// DropoffLocation).</summary>
    private static List<RouteStopDto> BuildRouteStops(Trip trip)
    {
        var warehouseLat = trip.Warehouse?.Lat;
        var warehouseLng = trip.Warehouse?.Lng;

        if (trip.LegType == TripLegType.Pickup)
        {
            return new List<RouteStopDto>
            {
                new(warehouseLat, warehouseLng, "🏭 Xuất phát từ Kho (xe chạy rỗng)"),
                new(trip.TransportOrder.PickupLocation?.Lat, trip.TransportOrder.PickupLocation?.Lng, "📦 Lấy hàng tại đối tác"),
                new(warehouseLat, warehouseLng, "🏭 Về Kho nộp hàng"),
            };
        }
        if (trip.LegType == TripLegType.Return)
        {
            return new List<RouteStopDto>
            {
                new(warehouseLat, warehouseLng, "🏭 Xuất phát từ Kho (trả hàng)"),
                new(trip.TransportOrder.PickupLocation?.Lat, trip.TransportOrder.PickupLocation?.Lng, "🔙 Trả hàng về đối tác (đơn đã hủy)"),
            };
        }
        return new List<RouteStopDto>
        {
            new(warehouseLat, warehouseLng, "🏭 Xuất phát từ Kho"),
            new(trip.TransportOrder.DropoffLocation?.Lat, trip.TransportOrder.DropoffLocation?.Lng, "🏁 Giao hàng tại điểm đến"),
        };
    }

    public async Task<(int Updated, int Failed)> RecalculatePlannedEstimatesForWarehouseAsync(int warehouseId)
    {
        var tripIds = await _db.Trips
            .Where(t => t.WarehouseId == warehouseId && t.Status == TripStatus.Planned)
            .Select(t => t.Id).ToListAsync();
        // Tối đa 30 chuyến trong 1 lần lưu kho (mỗi chuyến gọi VietMap) — phần còn lại bấm Ước tính
        // trên màn Chuyến. Lỗi lưu 1 chuyến thì DỪNG (dữ liệu dở trong cùng phiên sẽ làm các lần lưu
        // sau lỗi dây chuyền), các chuyến còn lại tính là chưa tính lại được.
        int updated = 0, failed = 0;
        foreach (var id in tripIds)
        {
            if (updated + failed >= 30) { failed++; continue; }
            try
            {
                var r = await CalculateAndSaveRouteEstimateAsync(id);
                if (r.Message is null) updated++; else failed++;
            }
            catch
            {
                failed += tripIds.Count - updated - failed;
                break;
            }
        }
        return (updated, failed);
    }

    public async Task<TripRouteEstimateDto> CalculateAndSaveRouteEstimateAsync(int tripId)
    {
        var trip = await _db.Trips
            .Include(t => t.TransportOrder).ThenInclude(o => o.PickupLocation)
            .Include(t => t.TransportOrder).ThenInclude(o => o.DropoffLocation)
            .Include(t => t.Warehouse)
            .FirstOrDefaultAsync(t => t.Id == tripId);

        if (trip is null)
            return new TripRouteEstimateDto(0, 0, "Không tìm thấy chuyến.");

        // Ước tính lộ trình là SỐ KẾ HOẠCH (báo cáo so km GPS thật với số này). Chuyến đã
        // bắt đầu mà đã có ước tính thì GIỮ NGUYÊN — tính lại sau khi sửa địa điểm sẽ làm
        // thay đổi số kế hoạch của chuyến cũ (và cờ lệch km trong báo cáo).
        if (trip.Status != TripStatus.Planned && trip.EstimatedDistanceKm.HasValue && trip.EstimatedDurationMinutes.HasValue)
            return new TripRouteEstimateDto(trip.EstimatedDistanceKm.Value, trip.EstimatedDurationMinutes.Value,
                "Chuyến đã bắt đầu — giữ nguyên ước tính kế hoạch đã có (không tính lại).");

        var stops = BuildRouteStops(trip).Where(s => s.Lat.HasValue && s.Lng.HasValue).ToList();
        if (stops.Count < 2)
            return new TripRouteEstimateDto(0, 0, "Thiếu tọa độ ở 1 hoặc nhiều điểm dừng — không ước tính được.");

        decimal totalDistanceKm = 0;
        int totalDurationMinutes = 0;
        bool gotAnyRealRoute = false;

        for (int i = 0; i < stops.Count - 1; i++)
        {
            var route = await _routeProvider.GetRouteAsync(new RouteRequest(
                stops[i].Lat!.Value, stops[i].Lng!.Value, stops[i + 1].Lat!.Value, stops[i + 1].Lng!.Value));
            if (route.Success)
            {
                totalDistanceKm += (decimal)route.DistanceKm;
                totalDurationMinutes += route.DurationMinutes;
                gotAnyRealRoute = true;
            }
        }

        if (!gotAnyRealRoute)
        {
            // Vẫn lưu THỜI GIAN ước lượng (đường chim bay) nếu chuyến chưa có — để kiểm tra giấy tờ /
            // bằng lái / ngày làm lúc Bắt đầu chuyến phủ đúng số ngày (thiếu số này thì chặng 8 ngày
            // chỉ bị xét như 1 ngày). KHÔNG lưu km: km kế hoạch của báo cáo phải là lộ trình thật.
            if (trip.EstimatedDurationMinutes is null)
            {
                var fallback = 0;
                for (int i = 0; i < stops.Count - 1; i++)
                    fallback += TruckingSystem.Application.Routing.RouteFallback.DriveMinutes(
                        stops[i].Lat!.Value, stops[i].Lng!.Value, stops[i + 1].Lat!.Value, stops[i + 1].Lng!.Value);
                trip.EstimatedDurationMinutes = fallback;
                await _db.SaveChangesAsync();
            }
            return new TripRouteEstimateDto(0, 0, "VietMap không tính được lộ trình (lỗi mạng hoặc hết quota) — thử lại sau.");
        }

        // LƯU LẠI vào Trip — không chỉ hiện tạm thời rồi bỏ đi, để SAU NÀY có
        // thể làm bảng thống kê so sánh với số liệu THỰC TẾ (StartTime/
        // EndTime, khoảng cách GPS thật) của chính chuyến này.
        trip.EstimatedDistanceKm = totalDistanceKm;
        trip.EstimatedDurationMinutes = totalDurationMinutes;
        await _db.SaveChangesAsync();

        return new TripRouteEstimateDto(totalDistanceKm, totalDurationMinutes, null);
    }

    public async Task<FuelEstimateDto> GetFuelEstimateAsync(int tripId)
    {
        var trip = await _db.Trips.Include(t => t.Vehicle).FirstOrDefaultAsync(t => t.Id == tripId);
        if (trip is null)
            return new FuelEstimateDto(0, null, null, 0, null, "Không tìm thấy chuyến.");

        // Bước 1: Tính khoảng cách THẬT đã đi — cộng dồn khoảng cách giữa các
        // điểm GPS liên tiếp (Waypoints app tài xế gửi lên mỗi 30 giây) bằng
        // công thức Haversine (khoảng cách đường chim bay giữa 2 tọa độ trên
        // mặt cầu Trái Đất) — CHÍNH XÁC hơn nhiều so với khoảng cách lý thuyết
        // VietMap Route API tính sẵn, vì đây là đường đi THẬT xe đã chạy.
        var waypoints = await _db.Waypoints
            .Where(w => w.TripId == tripId)
            .OrderBy(w => w.Timestamp)
            .Select(w => new { w.Lat, w.Lng })
            .ToListAsync();

        decimal actualDistanceKm = 0;
        for (int i = 1; i < waypoints.Count; i++)
        {
            actualDistanceKm += (decimal)HaversineDistanceKm(
                waypoints[i - 1].Lat, waypoints[i - 1].Lng,
                waypoints[i].Lat, waypoints[i].Lng);
        }

        // Bước 2: Chi phí THỰC TẾ — cộng THẲNG từ các lần đổ xăng đã ghi (FuelLog), cùng nguồn với
        // báo cáo — không đọc cột tổng Trip.ActualFuelCost (có thể lệch nếu lần cập nhật tổng lỗi).
        var actualCost = await _db.FuelLogs.Where(f => f.TripId == tripId).SumAsync(f => (decimal?)f.Cost) ?? 0m;

        if (waypoints.Count < 2)
            return new FuelEstimateDto(actualDistanceKm, null, null, actualCost, null,
                "Chưa đủ dữ liệu GPS để tính khoảng cách (cần ít nhất 2 điểm ping) — chưa ước tính được.");

        // Số liệu CHỐT lúc xuất phát (chuyến cũ chưa có thì lấy theo xe hiện tại).
        var fuelType = trip.FuelTypeAtStart ?? trip.Vehicle.FuelType;
        var consumption = trip.FuelConsumptionPer100KmAtStart ?? trip.Vehicle.FuelConsumptionPer100Km;
        if (consumption is null || string.IsNullOrEmpty(fuelType))
            return new FuelEstimateDto(actualDistanceKm, null, null, actualCost, null,
                "Xe này CHƯA khai báo định mức tiêu hao (L/100km) hoặc loại nhiên liệu — vào Sửa xe để khai báo trước khi ước tính được.");

        // Bước 3: Lấy giá nhiên liệu MỚI NHẤT (tính tới thời điểm chuyến kết
        // thúc) khớp đúng loại nhiên liệu xe này dùng — ưu tiên giá thị trường
        // chung (không giới hạn hãng) vì đa số xe không cố định 1 hãng đổ xăng.
        // Mốc giá = NGÀY BẮT ĐẦU chuyến (cùng mốc ghi đổ xăng / báo cáo); đã chốt giá lúc xuất
        // phát thì dùng luôn giá đó.
        var referenceDate = trip.StartTime ?? DateTime.UtcNow;
        var priceLookup = trip.FuelUnitPriceAtStart is null
            ? await FindFuelPriceAsync(fuelType, null, referenceDate)
            : new FuelPriceLookup(true, trip.FuelUnitPriceAtStart.Value, null, null, null);
        var latestPrice = priceLookup.Found ? new { Price = priceLookup.UnitPrice } : null;

        if (latestPrice is null)
            return new FuelEstimateDto(actualDistanceKm, null, null, actualCost, null,
                $"Chưa có giá nhiên liệu '{fuelType}' nào được nhập — vào màn Nhiên liệu để thêm giá trước khi ước tính được.");

        // Bước 4: Tính ước tính = (khoảng cách / 100) x định mức x đơn giá
        var estimatedLiters = (actualDistanceKm / 100m) * consumption.Value;
        var estimatedCost = estimatedLiters * latestPrice.Price;
        var variance = actualCost - estimatedCost; // dương = tốn thực tế NHIỀU hơn ước tính, âm = tiết kiệm hơn

        // Lưu lại vào Trip.PlannedFuelCost để dùng cho báo cáo sau này (dù tên
        // field là "Planned", ở đây dùng làm nơi lưu giá trị ƯỚC TÍNH tính
        // ngược từ GPS thật sau khi chuyến đã chạy xong, không phải dự trù
        // trước chuyến — tái dùng field có sẵn, tránh phải thêm migration mới).
        trip.PlannedFuelCost = estimatedCost;
        await _db.SaveChangesAsync();

        return new FuelEstimateDto(actualDistanceKm, estimatedLiters, estimatedCost, actualCost, variance, null);
    }

    public async Task<List<IncidentReportDto>> GetIncidentsAsync(int tripId)
    {
        return await _db.IncidentReports
            .Where(i => i.TripId == tripId)
            .OrderByDescending(i => i.Timestamp)
            .Select(i => new IncidentReportDto(i.Id, i.Type.ToString(), i.Description, i.Timestamp, i.Cost))
            .ToListAsync();
    }

    public async Task<IncidentReportDto> CreateIncidentAsync(int tripId, CreateIncidentReportRequest request)
    {
        // Sự cố CÓ chi phí được tính vào chi phí chuyến (báo cáo) -> cùng khóa 7 ngày sau khi đóng;
        // sự cố không có chi phí thì ghi bổ sung lúc nào cũng được.
        var blockReason = await GetTripCostBlockReasonAsync(tripId, "sự cố có chi phí", lockClosed: request.Cost is > 0);
        if (blockReason is not null) throw new InvalidOperationException(blockReason);

        // Mô tả BẮT BUỘC — biên bản sự cố phải ghi rõ chuyện gì đã xảy ra (đối chiếu chi phí, bảo hiểm).
        if (string.IsNullOrWhiteSpace(request.Description) || request.Description.Trim().Length < 5)
            throw new InvalidOperationException("Vui lòng mô tả sự cố (ít nhất 5 ký tự) — VD \"Thủng lốp sau trên QL1A, Km 1520\".");
        if (request.Cost.HasValue && request.Cost.Value < 0)
            throw new InvalidOperationException("Chi phí phát sinh không thể là số âm.");
        if (request.Cost.HasValue && request.Cost.Value > MaxMoney)
            throw new InvalidOperationException($"Chi phí phát sinh quá lớn (tối đa {MaxMoney:N0} đ).");

        if (!Enum.TryParse<IncidentType>(request.Type, out var type) || !Enum.IsDefined(type))
            throw new InvalidOperationException("Vui lòng chọn loại sự cố.");

        var incident = new IncidentReport
        {
            TripId = tripId,
            Type = type,
            Description = request.Description.Trim(),
            Cost = request.Cost,
            Timestamp = DateTime.UtcNow,
        };
        _db.IncidentReports.Add(incident);
        await _db.SaveChangesAsync();

        return new IncidentReportDto(incident.Id, incident.Type.ToString(), incident.Description, incident.Timestamp, incident.Cost);
    }

    public async Task<TripActionResult> DeleteIncidentAsync(int incidentId)
    {
        var incident = await _db.IncidentReports.Include(i => i.Trip).FirstOrDefaultAsync(i => i.Id == incidentId);
        if (incident is null) return new TripActionResult(false, "Không tìm thấy sự cố.");
        if (incident.Cost is > 0)
        {
            var costLock = ClosedCostLockReason(incident.Trip.Status, incident.Trip.EndTime, "sự cố có chi phí");
            if (costLock is not null) return new TripActionResult(false, costLock);
        }

        _db.IncidentReports.Remove(incident);
        await _db.SaveChangesAsync();
        return new TripActionResult(true, null);
    }

    public async Task<CalendarSyncResultDto> SyncCalendarAsync(int tripId, string publicTrackingBaseUrl)
    {
        var trip = await _db.Trips
            .Include(t => t.TransportOrder).ThenInclude(o => o.Customer)
            .Include(t => t.DriverEmployee).ThenInclude(e => e.DriverProfile)
            .FirstOrDefaultAsync(t => t.Id == tripId);

        if (trip is null)
            return new CalendarSyncResultDto(false, "Không tìm thấy chuyến.", false, "Không tìm thấy chuyến.");
        // Chuyến đã xong / đã hủy: không tạo lại sự kiện (Hủy đơn, Trả khách tại kho đã cố ý gỡ lịch).
        if (trip.Status is TripStatus.Completed or TripStatus.Cancelled)
        {
            const string closed = "Chuyến đã hoàn tất hoặc đã hủy — không đồng bộ lại lịch (lịch tự cập nhật giờ thật khi chuyến bắt đầu / hoàn tất).";
            return new CalendarSyncResultDto(false, closed, false, closed);
        }
        return await SyncCalendarCoreAsync(trip, publicTrackingBaseUrl, onlyExisting: false);
    }

    /// <summary>Chuyến ĐÃ TỪNG đồng bộ lịch -> tự cập nhật lại giờ sự kiện khi chuyến bắt đầu /
    /// hoàn tất (trước đây lịch giữ mãi giờ dự kiến cũ, VD chặng Giao bị dời vài ngày vì chờ
    /// nhập kho mà lịch khách vẫn hiện ngày cũ). Chỉ cập nhật sự kiện đã có, không tạo mới;
    /// lỗi Calendar không ảnh hưởng nghiệp vụ.</summary>
    public async Task ResyncCalendarIfSyncedAsync(int tripId, string publicTrackingBaseUrl)
    {
        try
        {
            if (!await _db.TripCalendarEvents.AnyAsync(e => e.TripId == tripId)) return;
            // AsNoTracking: trạng thái / giờ đến nơi vừa ghi bằng ExecuteUpdate (TryClaimStatusAsync)
            // -> bản chuyến đang được theo dõi trong cùng request còn giờ CŨ; phải đọc lại từ DB.
            var trip = await _db.Trips.AsNoTracking()
                .Include(t => t.TransportOrder).ThenInclude(o => o.Customer)
                .Include(t => t.DriverEmployee).ThenInclude(e => e.DriverProfile)
                .FirstOrDefaultAsync(t => t.Id == tripId);
            if (trip is null || trip.Status == TripStatus.Cancelled) return;
            await SyncCalendarCoreAsync(trip, publicTrackingBaseUrl, onlyExisting: true);
        }
        catch { /* Calendar không phải nguồn dữ liệu chính */ }
    }

    private async Task<CalendarSyncResultDto> SyncCalendarCoreAsync(Trip trip, string publicTrackingBaseUrl, bool onlyExisting)
    {
        var tripId = trip.Id;
        var order = trip.TransportOrder;
        var legText = trip.LegType switch
        {
            TripLegType.Pickup => "Lấy hàng",
            TripLegType.Return => "Trả hàng về đối tác",
            _ => "Giao hàng",
        };
        var location = trip.LegType switch
        {
            TripLegType.Pickup => order.PickupPoint,
            TripLegType.Return => order.PickupPoint, // trả về đúng điểm đã lấy hàng ban đầu
            _ => order.DropoffPoint,
        };

        // Ưu tiên giờ THẬT nếu chuyến đã/đang chạy; chưa chạy thì dùng hẹn giao
        // của đơn làm mốc tạm; cuối cùng mới rơi về "1 giờ nữa" cho có giá trị
        // hiển thị hợp lý trên Calendar (không để trống ngày giờ).
        // Hạn giao cam kết lưu theo GIỜ VIỆT NAM (người dùng nhập) -> đổi sang UTC.
        var committedUtc = order.CommittedDeliveryDate is { } committed ? VnClock.VnToUtc(committed) : (DateTime?)null;
        // Hạn giao chỉ là mốc hợp lý cho chặng GIAO; chặng Lấy hàng / Trả hàng chưa chạy
        // không lấy hạn giao làm giờ (trước đây chặng lấy hàng hiện đúng ngày hạn giao).
        // Hạn giao thường chỉ có NGÀY (0h) -> đặt 8h sáng ngày đó (giờ VN), không phải nửa đêm;
        // mốc đã qua (chuyến bị dời, VD chờ nhập kho) thì lấy 1 giờ nữa — không hiện lịch ở quá khứ.
        if (committedUtc is not null && order.CommittedDeliveryDate is { } committedVn && committedVn.TimeOfDay == TimeSpan.Zero)
            committedUtc = VnClock.VnToUtc(committedVn.Date.AddHours(8));
        var anchorUtc = trip.LegType == TripLegType.Delivery ? committedUtc : null;
        if (anchorUtc is not null && anchorUtc < DateTime.UtcNow) anchorUtc = null;
        var startUtc = trip.StartTime ?? anchorUtc ?? DateTime.UtcNow.AddHours(1);
        var endUtc = trip.EndTime ?? startUtc.AddHours(2);

        var trackingLink = $"{publicTrackingBaseUrl.TrimEnd('/')}/track.html?token={order.PublicTrackingToken}";
        var description = $"Đơn hàng: {order.Code}\nChặng: {legText}\nĐịa điểm: {location}\n\n🔗 Theo dõi trực tiếp: {trackingLink}";
        var title = $"🚚 {legText} — Đơn {order.Code}";

        var driverCalendarId = trip.DriverEmployee.DriverProfile?.GoogleCalendarId;
        var (driverSynced, driverMessage) = string.IsNullOrWhiteSpace(driverCalendarId)
            ? (false, "Tài xế này chưa khai báo Google Calendar ID.")
            : await SyncOneCalendarAsync(tripId, driverCalendarId!, title, description, startUtc, endUtc, location, onlyExisting);

        var customerCalendarId = order.Customer.GoogleCalendarId;
        var (customerSynced, customerMessage) = string.IsNullOrWhiteSpace(customerCalendarId)
            ? (false, "Khách hàng này chưa khai báo Google Calendar ID.")
            : await SyncOneCalendarAsync(tripId, customerCalendarId!, title, description, startUtc, endUtc, location, onlyExisting);

        return new CalendarSyncResultDto(driverSynced, driverMessage, customerSynced, customerMessage);
    }

    /// <summary>Đồng bộ 1 sự kiện lên ĐÚNG 1 Calendar cụ thể — tra cứu lại
    /// bảng TripCalendarEvent (dùng CHUNG cho cả tài xế lẫn khách hàng, không
    /// phân biệt) để biết ĐÃ TỪNG đồng bộ Calendar này cho Trip này chưa (nếu
    /// có -> CẬP NHẬT đúng sự kiện cũ, tránh tạo trùng lặp mỗi lần đồng bộ
    /// lại). Trước đây lưu Event ID trực tiếp vào 2 field cố định trên Trip
    /// (GoogleCalendarEventIdDriver/Customer) — kém linh hoạt hơn bảng riêng
    /// này (vốn đã được thiết kế sẵn từ đầu đúng cho việc này, hỗ trợ mở rộng
    /// thêm loại lịch khác sau này mà không cần đổi schema Trip nữa).</summary>
    private async Task<(bool Success, string? Message)> SyncOneCalendarAsync(
        int tripId, string calendarId, string title, string description, DateTime startUtc, DateTime endUtc, string? location,
        bool onlyExisting = false)
    {
        var existing = await _db.TripCalendarEvents.FirstOrDefaultAsync(e => e.TripId == tripId && e.CalendarId == calendarId);
        if (existing is null && onlyExisting) return (false, "Chưa đồng bộ lịch này trước đó — bỏ qua.");

        var result = await _calendarService.CreateOrUpdateEventAsync(new CalendarEventRequest(
            calendarId, existing?.GoogleEventId, title, description, startUtc, endUtc, location));

        if (!result.Success) return (false, result.Message);

        if (existing is null)
        {
            _db.TripCalendarEvents.Add(new TripCalendarEvent
            {
                TripId = tripId,
                CalendarId = calendarId,
                GoogleEventId = result.EventId!,
                SyncedAt = DateTime.UtcNow,
            });
        }
        else
        {
            existing.GoogleEventId = result.EventId!;
            existing.SyncedAt = DateTime.UtcNow;
        }
        await _db.SaveChangesAsync();

        return (true, null);
    }

    public async Task<List<TripExpenseDto>> GetExpensesAsync(int tripId)
    {
        return await _db.TripExpenses
            .Where(e => e.TripId == tripId)
            .Select(e => new TripExpenseDto(e.Id, e.Type.ToString(), e.Amount, e.Note))
            .ToListAsync();
    }

    public async Task<TripExpenseDto> CreateExpenseAsync(int tripId, CreateTripExpenseRequest request)
    {
        var blockReason = await GetTripCostBlockReasonAsync(tripId, "chi phí");
        if (blockReason is not null) throw new InvalidOperationException(blockReason);

        if (request.Amount <= 0)
            throw new InvalidOperationException("Số tiền phải lớn hơn 0.");
        if (request.Amount > MaxMoney)
            throw new InvalidOperationException($"Số tiền quá lớn (tối đa {MaxMoney:N0} đ).");

        // Loại chi phí phải đúng danh mục (trước đây gõ sai thì âm thầm thành "Khác"). Loại "Khác"
        // BẮT BUỘC ghi chú là chi gì — không thì kế toán không đối chiếu được.
        if (!Enum.TryParse<TripExpenseType>(request.Type, out var type) || !Enum.IsDefined(type))
            throw new InvalidOperationException("Vui lòng chọn loại chi phí.");
        var note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim();
        if (type == TripExpenseType.Other && note is null)
            throw new InvalidOperationException("Chi phí loại \"Khác\" phải ghi chú là chi cho việc gì.");

        var expense = new TripExpense
        {
            TripId = tripId,
            Type = type,
            Amount = request.Amount,
            Note = note,
        };
        _db.TripExpenses.Add(expense);
        await _db.SaveChangesAsync();

        return new TripExpenseDto(expense.Id, expense.Type.ToString(), expense.Amount, expense.Note);
    }

    /// <summary>Chặn ghi chi phí / sự cố cho chuyến CHƯA từng chạy (Planned) —
    /// khớp với nút trên Desktop (chỉ bật khi chuyến đã chạy hoặc đã hủy).
    /// Trước đây backend cho ghi ở MỌI trạng thái, gọi thẳng API vẫn ghi được
    /// chi phí cho chuyến chưa xuất phát, làm sai báo cáo chi phí đơn hàng.
    /// Trả về null nếu được phép ghi.</summary>
    private async Task<string?> GetTripCostBlockReasonAsync(int tripId, string what, bool lockClosed = true)
    {
        var trip = await _db.Trips
            .Where(t => t.Id == tripId)
            .Select(t => new { t.Status, t.StartTime, t.EndTime })
            .FirstOrDefaultAsync();
        if (trip is null) return "Không tìm thấy chuyến.";
        if (!HasRun(trip.Status, trip.StartTime))
            return trip.Status == TripStatus.Cancelled
                ? $"Chuyến đã hủy khi chưa xuất phát — không ghi {what}."
                : $"Chuyến chưa bắt đầu chạy — chưa thể ghi {what}. Hãy ghi sau khi chuyến đã xuất phát.";
        return lockClosed ? ClosedCostLockReason(trip.Status, trip.EndTime, what) : null;
    }

    /// <summary>Số ngày sau khi chuyến ĐÓNG (hoàn tất / hủy) vẫn được bổ sung / xóa chi phí, đổ
    /// xăng (hóa đơn về trễ). Quá hạn thì KHÓA: trước đây sửa được bất cứ lúc nào -> báo cáo chi
    /// phí / lợi nhuận các tháng đã chốt tự thay đổi.</summary>
    private const int CostEditDaysAfterClose = 7;

    private static string? ClosedCostLockReason(TripStatus status, DateTime? endTime, string what)
    {
        if (status is not (TripStatus.Completed or TripStatus.Cancelled) || endTime is null) return null;
        if (endTime.Value >= DateTime.UtcNow.AddDays(-CostEditDaysAfterClose)) return null;
        return $"Chuyến đã đóng từ {DateTime.SpecifyKind(endTime.Value, DateTimeKind.Utc).AddHours(7):dd/MM/yyyy} (quá {CostEditDaysAfterClose} ngày) — số liệu đã chốt, không thêm / xóa {what} nữa.";
    }

    /// <summary>Chuyến "đã từng chạy" = có giờ bắt đầu (kể cả đã hủy giữa đường).
    /// Dùng CHUNG cho xăng, chi phí, sự cố và báo cáo nhiên liệu.</summary>
    private static bool HasRun(TripStatus status, DateTime? startTime)
        => status != TripStatus.Planned && startTime.HasValue;

    public async Task<TripActionResult> DeleteExpenseAsync(int expenseId)
    {
        var expense = await _db.TripExpenses.Include(e => e.Trip).FirstOrDefaultAsync(e => e.Id == expenseId);
        if (expense is null) return new TripActionResult(false, "Không tìm thấy chi phí.");
        var costLock = ClosedCostLockReason(expense.Trip.Status, expense.Trip.EndTime, "chi phí");
        if (costLock is not null) return new TripActionResult(false, costLock);

        _db.TripExpenses.Remove(expense);
        await _db.SaveChangesAsync();
        return new TripActionResult(true, null);
    }

    /// <summary>Công thức Haversine — tính khoảng cách (km) đường chim bay
    /// giữa 2 tọa độ (lat/lng) trên mặt cầu Trái Đất, đủ chính xác cho quãng
    /// đường ngắn giữa các điểm GPS ping liên tiếp (30 giây/lần).</summary>
    private static double HaversineDistanceKm(double lat1, double lng1, double lat2, double lng2)
    {
        const double earthRadiusKm = 6371.0;
        var dLat = (lat2 - lat1) * Math.PI / 180.0;
        var dLng = (lng2 - lng1) * Math.PI / 180.0;
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(lat1 * Math.PI / 180.0) * Math.Cos(lat2 * Math.PI / 180.0) *
                Math.Sin(dLng / 2) * Math.Sin(dLng / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return earthRadiusKm * c;
    }

    // ================= Logic lõi dùng chung =================

    private Task<Trip?> LoadTripForArrivalAsync(int id) => _db.Trips
        .Include(t => t.TransportOrder).ThenInclude(o => o.CargoLines)
        .Include(t => t.TransportOrder).ThenInclude(o => o.Trips).ThenInclude(t => t.TripLines)
        .Include(t => t.Vehicle)
        .Include(t => t.TripLines)
        .FirstOrDefaultAsync(t => t.Id == id);

    /// <summary>ĐÚNG logic phân biệt theo LegType — khác bản cũ coi MỌI lần
    /// "đến nơi" là giao hàng xong toàn bộ đơn:
    ///   - LegType.Pickup: hàng mới VỀ TỚI KHO, chưa giao cho khách — KHÔNG
    ///     tính vào tiến độ giao hàng, KHÔNG cho xe rảnh lại (thường tiếp tục
    ///     chặng Delivery ngay). KHÔNG tự nhập kho — hàng chuyển sang "Chờ nhập kho".
    ///   - LegType.Delivery: hàng đã tới tay khách thật sự — tính vào tiến độ,
    ///     xe được rảnh lại, có thể tự động chuyển đơn sang Done.
    ///   - LegType.Return: chuyến TRẢ HÀNG về đối tác khi đơn bị hủy sau khi
    ///     đã nhập kho (xem TransportOrderService.CancelAsync) — hoàn tất =
    ///     hàng THẬT SỰ rời kho trả lại đối tác, xuất kho tại đây, xe rảnh
    ///     lại NGAY (đây luôn là chặng CUỐI CÙNG, không còn gì tiếp theo).</summary>
    private async Task<string?> HandleArrivalAsync(Trip trip)
    {
        trip.Status = TripStatus.Completed;
        // Đã có giờ tài xế báo đến nơi (ConfirmArrivalAsync) thì giữ nguyên; Dispatcher
        // tự bấm Hoàn tất (không qua app) thì lấy giờ hiện tại.
        trip.EndTime ??= DateTime.UtcNow;

        foreach (var line in trip.TripLines)
        {
            line.QtyDelivered = line.QtyPlanned;
            line.Status = TripLineStatus.Delivered;
        }

        if (trip.LegType == TripLegType.Delivery)
        {
            await ReleaseVehicleIfIdleAsync(trip);
            RecalculateOrderProgress(trip.TransportOrder);
        }
        else if (trip.LegType == TripLegType.Pickup && trip.WarehouseId.HasValue)
        {
            // Xe về tới kho — hàng mới TỚI kho, CHƯA lên kệ: KHÔNG tự nhập kho nữa. Hàng chuyển
            // sang "Chờ nhập kho" (xem PendingReceiptRules) — Vận hành nhập trên Desktop (màn Kho
            // → Hàng chờ nhập): tự chọn vị trí hoặc để Best-Fit gợi ý. Chặng Giao của đơn chỉ
            // bắt đầu được khi hàng đã nhập kho đủ (GetStartBlockReasonAsync).
            // Bình thường xe vẫn "Đang chạy" (chạy tiếp chặng Giao của chính nó). Nhưng nếu
            // chặng Giao đã được ĐỔI sang xe khác (nút Đổi xe/tài xế), xe này hết việc ->
            // trả về Rảnh (hàm chỉ trả khi xe không còn chuyến nào chờ/đang chạy).
            await ReleaseVehicleIfIdleAsync(trip);
        }
        else if (trip.LegType == TripLegType.Return && trip.WarehouseId.HasValue)
        {
            // Chuyến Trả hàng hoàn tất = hàng THẬT SỰ rời kho, trả lại đối
            // tác. Xuất kho tại đây (giống Delivery), rồi trả xe về rảnh (đây
            // là chặng CUỐI CÙNG, không còn gì tiếp theo nữa).
            await _db.SaveChangesAsync();
            foreach (var line in trip.TripLines)
            {
                // Chỉ xuất PHẦN CÒN THIẾU của chuyến này: nếu trước đó đã có phiếu
                // xuất gắn đúng chuyến này (VD lần bắt đầu trước bị gián đoạn, dữ
                // liệu từ phiên bản cũ) thì không xuất lại lần 2 — tránh báo "không
                // đủ tồn kho" dù hàng thật ra đã được xuất cho chính chuyến này.
                var alreadyIssued = await _db.GoodsIssues
                    .Where(g => g.TripId == trip.Id && g.CargoLineId == line.CargoLineId && g.ReversedAt == null)
                    .SumAsync(g => (decimal?)g.QtyIssued) ?? 0m;
                var qtyToIssue = line.QtyPlanned - alreadyIssued;
                if (qtyToIssue <= 0) continue;

                var issueResult = await _warehouseService.IssueGoodsAsync(
                    trip.WarehouseId.Value, trip.TransportOrderId, line.CargoLineId,
                    qtyToIssue, trip.DriverEmployeeId, trip.Id);
                if (!issueResult.Success)
                    return "Không thể xuất kho trả hàng: " + issueResult.Message;
            }
            await ReleaseVehicleIfIdleAsync(trip);
        }

        return null; // null = thành công
    }

    /// <summary>"Giành" đổi trạng thái chuyến bằng 1 câu UPDATE có điều kiện
    /// (WHERE Status = from). Trả về false nếu trạng thái đã bị thao tác khác
    /// đổi trước — chặn 2 thao tác chồng nhau (bấm 2 lần, app + Desktop cùng lúc).</summary>
    private async Task<bool> TryClaimStatusAsync(int tripId, TripStatus from, TripStatus to,
        DateTime? endTime = null, bool clearEndTime = false)
    {
        var query = _db.Trips.Where(t => t.Id == tripId && t.Status == from);
        int affected;
        if (endTime.HasValue)
        {
            var value = endTime.Value;
            affected = await query.ExecuteUpdateAsync(s => s
                .SetProperty(t => t.Status, to)
                .SetProperty(t => t.EndTime, value));
        }
        else if (clearEndTime)
        {
            affected = await query.ExecuteUpdateAsync(s => s
                .SetProperty(t => t.Status, to)
                .SetProperty(t => t.EndTime, (DateTime?)null));
        }
        else
        {
            affected = await query.ExecuteUpdateAsync(s => s.SetProperty(t => t.Status, to));
        }
        return affected == 1;
    }

    /// <summary>Trả xe về Rảnh SAU KHI xong chuyến — chỉ khi xe đang ở trạng
    /// thái "Đang chạy" (không ghi đè Bảo trì / Ngưng hoạt động mà người dùng
    /// đặt giữa chừng) VÀ xe không còn chuyến nào khác đang chờ/đang chạy.</summary>
    private async Task ReleaseVehicleIfIdleAsync(Trip trip)
    {
        if (trip.Vehicle.Status != VehicleStatus.OnTrip) return;
        var hasOtherActiveTrip = await _db.Trips.AnyAsync(t =>
            t.VehicleId == trip.VehicleId && t.Id != trip.Id &&
            (t.Status == TripStatus.Planned || t.Status == TripStatus.InProgress || t.Status == TripStatus.PendingConfirmation));
        if (!hasOtherActiveTrip) trip.Vehicle.Status = VehicleStatus.Available;
    }

    private static void RecalculateOrderProgress(TransportOrder order)
    {
        var totalToShip = order.CargoLines.Sum(c => c.QtyTotal);
        var totalDelivered = order.Trips
            .Where(t => t.LegType == TripLegType.Delivery)
            .SelectMany(t => t.TripLines)
            .Sum(tl => tl.QtyDelivered);

        order.ProgressPercent = totalToShip == 0 ? 0 : Math.Round(totalDelivered / totalToShip * 100, 2);

        // Sai số làm tròn 0,01 (số lượng lẻ) không được làm đơn kẹt "Đang chạy" mãi.
        // ...nhưng chỉ khi KHÔNG còn chuyến giao nào chưa xong (dòng hàng rất nhỏ 0,01 vẫn đang chạy).
        var deliveryStillOpen = order.Trips.Any(t => t.LegType == TripLegType.Delivery &&
            t.Status is TripStatus.Planned or TripStatus.InProgress or TripStatus.PendingConfirmation);
        if ((order.ProgressPercent >= 100 || (totalToShip > 0 && totalDelivered >= totalToShip - 0.01m && !deliveryStillOpen))
            && order.Status == TransportOrderStatus.InProgress)
            order.Status = TransportOrderStatus.Done;
    }

    // =====================================================================
    // GỠ CHUYẾN BỊ KẸT (chưa chạy): đổi xe/tài xế, hoặc trả khách tại kho.
    // Trước đây chuyến chờ chạy có xe hỏng / giấy tờ / bằng lái hết hạn sẽ không bao
    // giờ bắt đầu được, hủy đơn thì chuyến Trả hàng lại dùng đúng xe đó -> kẹt mãi:
    // hàng bị giữ, xe + tài xế "bận" mãi, không đặt Ngưng hoạt động được.
    // =====================================================================

    /// <summary>Khóa "gán xe/tài xế" dùng chung (pg_advisory_xact_lock) giữa Chia xe và Đổi
    /// xe/tài xế — tự nhả khi transaction kết thúc.</summary>
    public const long AssignmentLockKey = 7_420_001;

    private sealed record ReassignContext(Trip Trip, List<Trip> Affected, int OrderWarehouseId,
        decimal NeedQty, decimal NeedKg, decimal NeedM3);

    private static readonly TripStatus[] ActiveTripStatuses =
        { TripStatus.Planned, TripStatus.InProgress, TripStatus.PendingConfirmation };

    /// <summary>Chuyến được chọn + các chuyến ĐI CÙNG (cùng đơn, cùng xe, cùng tài xế, đều
    /// chưa chạy — VD cặp Lấy hàng + Giao hàng) và tải trọng lớn nhất xe mới phải chở.</summary>
    private async Task<(ReassignContext? Ctx, string? Error)> LoadReassignContextAsync(int tripId)
    {
        var trip = await _db.Trips.Include(t => t.TransportOrder).FirstOrDefaultAsync(t => t.Id == tripId);
        if (trip is null) return (null, "Không tìm thấy chuyến.");
        if (trip.Status != TripStatus.Planned)
            return (null, "Chỉ đổi xe / tài xế được cho chuyến CHƯA CHẠY (Đã lên kế hoạch).");

        var affected = await _db.Trips
            .Include(t => t.TripLines).ThenInclude(l => l.CargoLine)
            .Where(t => t.TransportOrderId == trip.TransportOrderId && t.Status == TripStatus.Planned
                        && t.VehicleId == trip.VehicleId && t.DriverEmployeeId == trip.DriverEmployeeId)
            .OrderBy(t => t.Id)
            .ToListAsync();

        // Kho đã xuất hàng LÊN XE CŨ cho chặng giao -> đổi xe thì hàng vẫn nằm trên xe cũ. Hoàn tác xuất
        // (dỡ hàng về kệ) trước, đổi xe xong kho xuất lại lên xe mới.
        var affectedIds = affected.Select(t => t.Id).ToList();
        var loaded = await _db.GoodsIssues
            .Where(g => g.TripId != null && affectedIds.Contains(g.TripId.Value) && g.ReversedAt == null)
            .Select(g => g.TripId!.Value).Distinct().ToListAsync();
        if (loaded.Count > 0)
            return (null, $"Kho đã xuất hàng lên xe hiện tại cho chuyến #{string.Join(", #", loaded)} — vào màn Kho → 📤 Xuất kho theo chuyến, " +
                          "bấm Hoàn tác xuất (dỡ hàng về kệ) rồi mới đổi xe / tài xế; đổi xong kho xuất lại lên xe mới.");

        decimal needQty = 0, needKg = 0, needM3 = 0;
        foreach (var t in affected)
        {
            var qty = t.TripLines.Sum(l => l.QtyPlanned);
            var kg = t.TripLines.Sum(l => l.CargoLine.QtyTotal > 0 ? l.QtyPlanned * l.CargoLine.WeightKg / l.CargoLine.QtyTotal : 0);
            var m3 = t.TripLines.Sum(l => l.CargoLine.QtyTotal > 0 ? l.QtyPlanned * l.CargoLine.VolumeM3 / l.CargoLine.QtyTotal : 0);
            needQty = Math.Max(needQty, qty);
            needKg = Math.Max(needKg, Math.Round(kg, 3));
            needM3 = Math.Max(needM3, Math.Round(m3, 4));
        }
        return (new ReassignContext(trip, affected, trip.TransportOrder.WarehouseId, needQty, needKg, needM3), null);
    }

    /// <summary>Đánh giá MỌI xe / tài xế theo CÙNG điều kiện với Chia xe + Bắt đầu chuyến.</summary>
    private async Task<(List<ReassignOptionDto> Vehicles, List<ReassignOptionDto> Drivers)> BuildReassignOptionsAsync(ReassignContext ctx)
    {
        // Khoảng thời gian chuyến tính từ NGÀY XUẤT PHÁT DỰ KIẾN (chưa tới thì từ ngày đó, đã qua
        // thì từ hôm nay) — CÙNG cách Chia xe tính, không mặc định "từ hôm nay".
        var today = VnClock.Today;
        if (ctx.Trip.PlannedStartDate is DateTime plannedStart && plannedStart.Date > today) today = plannedStart.Date;
        var affectedIds = ctx.Affected.Select(t => t.Id).ToList();
        var currentVehicleId = ctx.Trip.VehicleId;
        var currentDriverId = ctx.Trip.DriverEmployeeId;
        // Giấy tờ xe + bằng lái phải còn hạn tới HẾT NGÀY CUỐI các chặng (giống Chia xe):
        // số ngày = tổng thời lượng ước tính / 8 giờ lái, chặng chưa có ước tính tính 1 ngày.
        var tripDays = Math.Max(1, (int)Math.Ceiling(ctx.Affected.Sum(t => t.EstimatedDurationMinutes ?? 0) / 480.0)
                                   + ctx.Affected.Count(t => t.EstimatedDurationMinutes is null));
        var lastDay = today.AddDays(tripDays - 1);

        var busy = await _db.Trips
            .Where(t => ActiveTripStatuses.Contains(t.Status) && !affectedIds.Contains(t.Id))
            .Select(t => new { t.VehicleId, t.DriverEmployeeId })
            .ToListAsync();
        var busyVehicleIds = busy.Select(b => b.VehicleId).ToHashSet();
        var busyDriverIds = busy.Select(b => b.DriverEmployeeId).ToHashSet();

        // ----- Xe -----
        var docsByVehicle = (await VehicleDocumentRules.GetLatestAsync(_db)).ToLookup(d => d.VehicleId);
        var vehicles = await _db.Vehicles
            .OrderBy(v => v.PlateNumber)
            .Select(v => new { v.Id, v.PlateNumber, v.Status, v.WarehouseId, v.CapacityQty, v.CapacityKg, v.CapacityM3 })
            .ToListAsync();
        var vehicleOptions = new List<ReassignOptionDto>();
        foreach (var v in vehicles)
        {
            string? reason = null;
            var isCurrent = v.Id == currentVehicleId;
            if (v.Status == VehicleStatus.UnderMaintenance) reason = "đang Bảo trì";
            else if (v.Status == VehicleStatus.OutOfService) reason = "đang Ngưng hoạt động";
            else if (!isCurrent && (v.Status != VehicleStatus.Available || busyVehicleIds.Contains(v.Id))) reason = "đang có chuyến khác";
            else if (v.WarehouseId.HasValue && v.WarehouseId.Value != ctx.OrderWarehouseId) reason = "đóng quân ở kho khác";
            else if (v.CapacityQty < ctx.NeedQty || v.CapacityKg < ctx.NeedKg || v.CapacityM3 < ctx.NeedM3)
                reason = $"không đủ sức chứa (cần {ctx.NeedQty:0.##} đv / {ctx.NeedKg:0.##} kg / {ctx.NeedM3:0.##} m³)";
            else reason = VehicleDocumentRules.BlockReason(docsByVehicle[v.Id], v.PlateNumber, lastDay);

            var text = v.PlateNumber + (isCurrent ? " (xe hiện tại)" : "") + $" — {v.CapacityQty:0.##} đv / {v.CapacityKg:0.##} kg / {v.CapacityM3:0.##} m³";
            vehicleOptions.Add(new ReassignOptionDto(v.Id, text, reason is null, reason));
        }

        // ----- Tài xế -----
        // Phải đăng ký ĐỦ ngày làm từ hôm nay tới hết ngày cuối chuyến — giống Chia xe
        // (trước đây chỉ cảnh báo -> tài xế "hết ca" giữa chuyến nhiều ngày).
        var neededDates = Enumerable.Range(0, tripDays).Select(i => today.AddDays(i)).ToList();
        var afterLastDay = lastDay.AddDays(1);
        var scheduledDatesByDriver = (await _db.DriverSchedules
                .Where(s => s.ShiftDate >= today && s.ShiftDate < afterLastDay)
                .Select(s => new { s.DriverEmployeeId, s.ShiftDate })
                .ToListAsync())
            .GroupBy(s => s.DriverEmployeeId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.ShiftDate.Date).ToHashSet());
        var driverPositionIds = await DriverPositionRules.DriverPositionIdsAsync(_db);
        var drivers = await _db.DriverProfiles
            .OrderBy(dp => dp.Employee.FullName)
            .Select(dp => new {
                dp.EmployeeId,
                dp.Employee.FullName,
                dp.Employee.Code,
                EmpStatus = dp.Employee.Status,
                dp.Employee.PositionId,
                dp.LicenseExpiryDate,
                dp.LicenseClass,
                dp.WarehouseId,
                dp.HealthCertExpiryDate
            })
            .ToListAsync();
        var driverOptions = new List<ReassignOptionDto>();
        foreach (var d in drivers)
        {
            string? reason = null;
            var eligible = false;
            var isCurrent = d.EmployeeId == currentDriverId;
            if (d.EmpStatus != EmployeeStatus.Active) reason = "không ở trạng thái Đang làm việc";
            else if (d.PositionId is null || !driverPositionIds.Contains(d.PositionId.Value)) reason = "chức danh không phải Tài xế";
            else if (d.LicenseExpiryDate.Date < lastDay) reason = $"bằng lái hết hạn trước khi chuyến xong ({d.LicenseExpiryDate:dd/MM/yyyy})";
            else if (d.HealthCertExpiryDate is DateTime hc && hc.Date < lastDay) reason = $"giấy khám sức khỏe hết hạn trước khi chuyến xong ({hc:dd/MM/yyyy})";
            else if (!isCurrent && busyDriverIds.Contains(d.EmployeeId)) reason = "đang có chuyến khác";
            else if (d.WarehouseId.HasValue && d.WarehouseId.Value != ctx.OrderWarehouseId) reason = "đóng quân ở kho khác";
            else if (!scheduledDatesByDriver.TryGetValue(d.EmployeeId, out var dates) || !neededDates.All(dates.Contains))
                reason = tripDays > 1
                    ? $"chưa đăng ký đủ ngày làm từ {today:dd/MM} tới {lastDay:dd/MM}"
                    : $"chưa đăng ký ngày làm ngày {today:dd/MM}";
            else eligible = true;
            // Hạng bằng hiện kèm tên — hạng bằng phải hợp với XE được chọn (kiểm tra khi bấm Lưu).
            var text = $"{d.FullName} ({d.Code}) — bằng {d.LicenseClass}" + (isCurrent ? " (tài xế hiện tại)" : "");
            driverOptions.Add(new ReassignOptionDto(d.EmployeeId, text, eligible, reason));
        }

        return (vehicleOptions, driverOptions);
    }

    public async Task<ReassignOptionsDto> GetReassignOptionsAsync(int tripId)
    {
        var (ctx, error) = await LoadReassignContextAsync(tripId);
        if (ctx is null)
            return new ReassignOptionsDto(tripId, new List<int>(), 0, 0, new List<ReassignOptionDto>(), new List<ReassignOptionDto>(), error);

        var (vehicles, drivers) = await BuildReassignOptionsAsync(ctx);
        return new ReassignOptionsDto(tripId, ctx.Affected.Select(t => t.Id).ToList(), ctx.Trip.VehicleId, ctx.Trip.DriverEmployeeId,
            vehicles, drivers, null);
    }

    public async Task<TripActionResult> ReassignAsync(int tripId, ReassignTripRequest request)
    {
        var orderId = await _db.Trips.Where(t => t.Id == tripId).Select(t => (int?)t.TransportOrderId).FirstOrDefaultAsync();
        if (orderId is null) return new TripActionResult(false, "Không tìm thấy chuyến.");

        // Khóa các chuyến của đơn (CÙNG thứ tự khóa với Hủy đơn) — không cho Bắt đầu / Hủy
        // chen vào giữa lúc đang đổi xe.
        await using var transaction = await _db.Database.BeginTransactionAsync();
        // Khóa chung "gán xe/tài xế" (cùng khóa với Chia xe): 2 người cùng lúc không thể
        // gán TRÙNG 1 tài xế (xe đã có cột xmin chặn, tài xế thì chưa).
        await _db.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(" + AssignmentLockKey + ")");
        await _db.Database.ExecuteSqlInterpolatedAsync($"SELECT 1 FROM \"Trips\" WHERE \"TransportOrderId\" = {orderId.Value} ORDER BY \"Id\" FOR UPDATE");

        var (ctx, error) = await LoadReassignContextAsync(tripId);
        if (ctx is null) return new TripActionResult(false, error);
        if (request.VehicleId == ctx.Trip.VehicleId && request.DriverEmployeeId == ctx.Trip.DriverEmployeeId)
            return new TripActionResult(false, "Xe và tài xế chọn giống hiện tại — không có gì để đổi.");

        var (vehicles, drivers) = await BuildReassignOptionsAsync(ctx);
        var vehicle = vehicles.FirstOrDefault(v => v.Id == request.VehicleId);
        if (vehicle is null) return new TripActionResult(false, "Không tìm thấy xe đã chọn.");
        if (!vehicle.Eligible) return new TripActionResult(false, $"Không dùng được xe này: {vehicle.Reason}.");
        var driver = drivers.FirstOrDefault(d => d.Id == request.DriverEmployeeId);
        if (driver is null) return new TripActionResult(false, "Không tìm thấy tài xế đã chọn.");
        if (!driver.Eligible) return new TripActionResult(false, $"Không dùng được tài xế này: {driver.Reason}.");

        // Hạng bằng lái của tài xế phải lái được XE đã chọn (tải trọng / đầu kéo).
        var pairVehicle = await _db.Vehicles.Where(v => v.Id == request.VehicleId)
            .Select(v => new { v.PlateNumber, v.CapacityKg, v.VehicleType }).FirstAsync();
        var pairLicense = await _db.DriverProfiles.Where(dp => dp.EmployeeId == request.DriverEmployeeId)
            .Select(dp => dp.LicenseClass).FirstAsync();
        var licenseReason = DriverLicenseRules.BlockReason(pairLicense, pairVehicle.CapacityKg, pairVehicle.VehicleType);
        if (licenseReason is not null)
            return new TripActionResult(false, $"Tài xế đã chọn không lái được xe {pairVehicle.PlateNumber}: {licenseReason}.");

        var oldVehicleId = ctx.Trip.VehicleId;
        var affectedIds = ctx.Affected.Select(t => t.Id).ToList();
        foreach (var t in ctx.Affected)
        {
            t.VehicleId = request.VehicleId;
            t.DriverEmployeeId = request.DriverEmployeeId;
        }

        if (request.VehicleId != oldVehicleId)
        {
            var newVehicle = await _db.Vehicles.FindAsync(request.VehicleId);
            if (newVehicle is not null && newVehicle.Status == VehicleStatus.Available) newVehicle.Status = VehicleStatus.OnTrip;

            // Xe cũ hết chuyến -> trả về Rảnh (không ghi đè Bảo trì / Ngưng hoạt động).
            var oldVehicle = await _db.Vehicles.FindAsync(oldVehicleId);
            var oldHasOther = await _db.Trips.AnyAsync(t =>
                t.VehicleId == oldVehicleId && !affectedIds.Contains(t.Id) && ActiveTripStatuses.Contains(t.Status));
            if (oldVehicle is not null && !oldHasOther && oldVehicle.Status == VehicleStatus.OnTrip)
                oldVehicle.Status = VehicleStatus.Available;
        }

        await _db.SaveChangesAsync();
        await transaction.CommitAsync();

        // Lịch Google cũ ghi xe/tài xế cũ -> gỡ (đồng bộ lại sau). Lỗi lịch không ảnh hưởng.
        await RemoveCalendarEventsAsync(affectedIds);

        return new TripActionResult(true,
            $"Đã đổi sang xe/tài xế mới cho chuyến #{string.Join(", #", affectedIds)}." +
            " Nếu đã gửi link / đồng bộ lịch cho tài xế cũ, hãy gửi lại cho tài xế mới.");
    }

    /// <summary>Gỡ sự kiện Google Calendar của các chuyến (best-effort). CHỈ xóa bản ghi
    /// của sự kiện đã gỡ được trên Google — gỡ lỗi thì giữ bản ghi để lần sau gỡ lại /
    /// đồng bộ đè, không để sự kiện "mồ côi" mà hệ thống không còn biết.</summary>
    private async Task RemoveCalendarEventsAsync(List<int> tripIds)
    {
        try
        {
            var events = await _db.TripCalendarEvents.Where(e => tripIds.Contains(e.TripId)).ToListAsync();
            var removed = new List<TripCalendarEvent>();
            foreach (var ev in events)
            {
                try
                {
                    if (await _calendarService.DeleteEventAsync(ev.CalendarId, ev.GoogleEventId)) removed.Add(ev);
                }
                catch { /* giữ bản ghi */ }
            }
            if (removed.Count > 0)
            {
                _db.TripCalendarEvents.RemoveRange(removed);
                await _db.SaveChangesAsync();
            }
        }
        catch { /* lịch không phải nguồn dữ liệu chính — không làm hỏng thao tác chính */ }
    }

    /// <summary>Giới hạn số tiền nhập tay (đ) — số lớn hơn làm tràn cột numeric(18,2) -> lỗi 500.</summary>
    private const decimal MaxMoney = 10_000_000_000m;

    public async Task<TripActionResult> ReturnAtWarehouseAsync(int tripId, string? userName)
    {
        var trip = await _db.Trips
            .Include(t => t.TripLines)
            .Include(t => t.Vehicle)
            .FirstOrDefaultAsync(t => t.Id == tripId);
        if (trip is null) return new TripActionResult(false, "Không tìm thấy chuyến.");
        if (trip.LegType != TripLegType.Return)
            return new TripActionResult(false, "Chỉ áp dụng cho chuyến TRẢ HÀNG.");
        if (trip.Status != TripStatus.Planned)
            return new TripActionResult(false, "Chỉ áp dụng cho chuyến trả hàng CHƯA CHẠY — chuyến đang chạy thì Hoàn tất như bình thường.");
        if (!trip.WarehouseId.HasValue)
            return new TripActionResult(false, "Chuyến này không gắn kho — không xuất kho được.");

        await using var transaction = await _db.Database.BeginTransactionAsync();
        var now = DateTime.UtcNow;
        if (!await TryClaimStatusAsync(trip.Id, TripStatus.Planned, TripStatus.Completed, endTime: now))
            return new TripActionResult(false, "Trạng thái chuyến vừa bị thay đổi bởi thao tác khác — hãy tải lại danh sách.");
        trip.Status = TripStatus.Completed;
        trip.EndTime = now;
        await _db.SaveChangesAsync();

        var performer = (string.IsNullOrWhiteSpace(userName) ? "Vận hành" : userName) + " — trả khách tại kho";
        foreach (var line in trip.TripLines)
        {
            var alreadyIssued = await _db.GoodsIssues
                .Where(g => g.TripId == trip.Id && g.CargoLineId == line.CargoLineId && g.ReversedAt == null)
                .SumAsync(g => (decimal?)g.QtyIssued) ?? 0m;
            var qtyToIssue = line.QtyPlanned - alreadyIssued;
            if (qtyToIssue > 0)
            {
                var issue = await _warehouseService.IssueGoodsAsync(trip.WarehouseId.Value, trip.TransportOrderId, line.CargoLineId,
                    qtyToIssue, issuedByEmployeeId: null, tripId: trip.Id, performedByUserName: performer);
                if (!issue.Success)
                    return new TripActionResult(false, "Không thể xuất kho trả hàng: " + issue.Message); // chưa Commit -> Rollback
            }
            line.QtyDelivered = line.QtyPlanned;
            line.Status = TripLineStatus.Delivered;
        }

        await ReleaseVehicleIfIdleAsync(trip);
        await _db.SaveChangesAsync();
        await transaction.CommitAsync();
        await RemoveCalendarEventsAsync(new List<int> { trip.Id }); // chuyến không chạy nữa -> gỡ lịch tài xế
        return new TripActionResult(true, "Đã trả hàng cho khách tại kho — chuyến trả hàng đóng lại, xe và tài xế được giải phóng.");
    }
}
