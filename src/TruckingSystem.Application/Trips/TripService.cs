using Microsoft.EntityFrameworkCore;
using TruckingSystem.Application.Calendar;
using TruckingSystem.Application.Common;
using TruckingSystem.Application.Common.Interfaces;
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
///   - Chặng Pickup hoàn tất -> tự động ReceiveGoodsAsync (tạo GoodsReceipt +
///     chạy Best-Fit chọn vị trí + cộng WarehouseStock)
///   - Chặng Delivery bắt đầu chạy -> tự động IssueGoodsAsync (tạo GoodsIssue
///     + trừ dần StorageAssignment theo FIFO + trừ WarehouseStock)
///   - Chặng Return (MỚI — chuyến trả hàng về đối tác khi đơn bị hủy sau khi
///     đã nhập kho) hoàn tất -> tự động IssueGoodsAsync (xuất kho thật, trả
///     hàng), trả xe rảnh lại — xem TransportOrderService.CancelAsync là nơi
///     TẠO Trip loại này.
///   - Đơn SelfDelivery (không có chặng Pickup) -> nhập kho NGAY lúc Chia xe,
///     xem VehicleAllocationService.
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

            return new TripListItemDto(
                t.Id, t.TransportOrder.Code, t.Vehicle.PlateNumber, t.DriverEmployee.FullName,
                t.Status.ToString(), t.LegType.ToString(), t.TripLines.Sum(tl => tl.QtyPlanned),
                fromLat, fromLng, toLat, toLng);
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
        // (Vận hành đã duyệt -> hàng đã thật sự nhập kho) — áp dụng CHUNG cho
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

        // Chặng Delivery bắt đầu chạy = hàng THẬT SỰ rời khỏi kho — xuất kho
        // ngay tại đây (không đợi tới lúc giao xong, vì giao xong đã sang xe khác chở đi rồi).
        if (trip.LegType == TripLegType.Delivery && trip.WarehouseId.HasValue)
        {
            foreach (var line in trip.TripLines)
            {
                // Chỉ xuất PHẦN CÒN THIẾU của chuyến này: nếu trước đó đã có phiếu
                // xuất gắn đúng chuyến này (VD lần bắt đầu trước bị gián đoạn, dữ
                // liệu từ phiên bản cũ) thì không xuất lại lần 2 — tránh báo "không
                // đủ tồn kho" dù hàng thật ra đã được xuất cho chính chuyến này.
                var alreadyIssued = await _db.GoodsIssues
                    .Where(g => g.TripId == trip.Id && g.CargoLineId == line.CargoLineId)
                    .SumAsync(g => (decimal?)g.QtyIssued) ?? 0m;
                var qtyToIssue = line.QtyPlanned - alreadyIssued;
                if (qtyToIssue <= 0) continue;

                var issueResult = await _warehouseService.IssueGoodsAsync(
                    trip.WarehouseId.Value, trip.TransportOrderId, line.CargoLineId,
                    qtyToIssue, trip.DriverEmployeeId, trip.Id);

                if (!issueResult.Success)
                    return new TripActionResult(false, "Không thể bắt đầu chuyến — " + issueResult.Message);
            }
        }

        trip.Status = TripStatus.InProgress;
        trip.StartTime = DateTime.UtcNow;

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
    /// PendingConfirmation -> Completed THẬT SỰ (lúc này mới nhập/xuất kho,
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
        return new TripActionResult(true, null);
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

        decimal cost;
        string? stationName = string.IsNullOrWhiteSpace(request.StationName) ? null : request.StationName.Trim();
        if (request.AutoPrice)
        {
            // Đơn giá lấy TỰ ĐỘNG từ bảng giá nhiên liệu (màn Nhiên liệu) — chỉ
            // nhập số lít, không nhập tiền tay (tránh gõ sai / khai khống).
            var quote = await FindFuelPriceAsync(trip.Vehicle.FuelType, request.FuelProviderId);
            if (!quote.Found) return new TripActionResult(false, quote.Message);
            cost = Math.Round(request.Liters * quote.UnitPrice, 0);

            var label = quote.SourceText ?? "";
            stationName = stationName is null ? label : $"{label} — {stationName}";
        }
        else
        {
            if (request.Cost <= 0) return new TripActionResult(false, "Chi phí phải lớn hơn 0.");
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
            .Select(t => new { t.Status, t.StartTime, t.Vehicle.FuelType, t.Vehicle.PlateNumber })
            .FirstOrDefaultAsync();
        if (trip is null)
            return new FuelPriceQuoteDto(false, false, null, 0, null, null, "Không tìm thấy chuyến.");

        var canRecord = HasRun(trip.Status, trip.StartTime);
        var quote = await FindFuelPriceAsync(trip.FuelType, fuelProviderId);
        var message = !canRecord
            ? (trip.Status == TripStatus.Cancelled
                ? "Chuyến đã hủy khi chưa xuất phát — không có đổ xăng."
                : "Chuyến chưa bắt đầu chạy — chưa ghi đổ xăng được.")
            : quote.Message;
        return new FuelPriceQuoteDto(canRecord, quote.Found, trip.FuelType, quote.UnitPrice, quote.EffectiveDate, quote.SourceText, message);
    }

    public async Task<TripActionResult> DeleteFuelLogAsync(int fuelLogId)
    {
        var log = await _db.FuelLogs.Include(f => f.Trip).FirstOrDefaultAsync(f => f.Id == fuelLogId);
        if (log is null) return new TripActionResult(false, "Không tìm thấy lần đổ xăng này.");

        var tripId = log.TripId;
        _db.FuelLogs.Remove(log);
        await _db.SaveChangesAsync();
        await RecalculateActualFuelCostAsync(tripId);
        return new TripActionResult(true, null);
    }

    /// <summary>Đặt Trip.ActualFuelCost = TỔNG tiền các lần đổ xăng của chuyến, tính
    /// ngay trong database (không đọc-sửa-ghi) — luôn khớp với báo cáo.</summary>
    private Task<int> RecalculateActualFuelCostAsync(int tripId)
        => _db.Trips
            .Where(t => t.Id == tripId)
            .ExecuteUpdateAsync(s => s.SetProperty(
                t => t.ActualFuelCost,
                t => t.FuelLogs.Sum(f => (decimal?)f.Cost) ?? 0m));

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

        // Chỉ ghi GPS khi chuyến THẬT SỰ đang chạy — chuyến đã báo Hoàn tất
        // (Chờ xác nhận), đã Hoàn tất hẳn, hay đã Hủy đều KHÔNG ghi tiếp, tránh
        // phình database vô ích (app tài xế vẫn có thể lỡ gửi thêm vài điểm
        // cuối trước khi nhận ra, không coi là lỗi nghiêm trọng).
        if (trip.Status != TripStatus.Planned && trip.Status != TripStatus.InProgress)
            return new ServiceResult(false, "Chuyến này không còn ghi nhận GPS nữa.");

        // GPS ĐẦU TIÊN của chuyến còn "Đã lên kế hoạch" = tài xế bắt đầu chạy.
        // Trước đây chỉ đổi trạng thái sang Đang chạy -> chặng Giao có thể chạy
        // khi chặng Lấy hàng chưa xong, và BỎ QUA bước xuất kho (tồn kho sai).
        // Giờ đi qua ĐÚNG quy trình StartAsync: kiểm tra điều kiện + xuất kho.
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
            $"Tài xế {trip.DriverEmployee.FullName} báo đã {legText} xong cho đơn {trip.TransportOrder.Code} (chuyến #{trip.Id}). " +
            $"Vào màn Quản lý Chuyến để kiểm tra và Duyệt.");

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
    /// Vận hành chưa duyệt thì hàng chưa được nhập kho. Đơn khách tự mang hàng
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
        if (pendingPickups.Count == 0) return await GetStockBlockReasonAsync(trip);

        var list = string.Join(", ", pendingPickups.Select(p => $"#{p.Id} ({PickupStatusText(p.Status)})"));
        return $"Chưa thể bắt đầu chuyến giao hàng — đơn còn chuyến lấy hàng chưa hoàn tất: {list}. " +
               "Hàng phải được lấy về và nhập kho (Vận hành duyệt) xong mới được xuất kho đi giao.";
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
                .Where(g => g.TripId == trip.Id && g.CargoLineId == line.CargoLineId)
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

        var vehicle = await _db.Vehicles
            .Where(v => v.Id == trip.VehicleId)
            .Select(v => new { v.PlateNumber, v.Status })
            .FirstOrDefaultAsync();
        if (vehicle is null) return "Không tìm thấy xe của chuyến này.";
        if (vehicle.Status == VehicleStatus.UnderMaintenance)
            return $"Xe {vehicle.PlateNumber} đang Bảo trì — chưa thể xuất phát.";
        if (vehicle.Status == VehicleStatus.OutOfService)
            return $"Xe {vehicle.PlateNumber} đang Ngưng hoạt động — chưa thể xuất phát.";

        var docs = await VehicleDocumentRules.GetLatestAsync(_db, trip.VehicleId);
        var docReason = VehicleDocumentRules.BlockReason(docs, vehicle.PlateNumber, today);
        if (docReason is not null) return docReason;

        var driver = await _db.DriverProfiles
            .Where(dp => dp.EmployeeId == trip.DriverEmployeeId)
            .Select(dp => new { dp.LicenseExpiryDate, EmpStatus = dp.Employee.Status, dp.Employee.FullName })
            .FirstOrDefaultAsync();
        if (driver is null) return "Tài xế của chuyến này chưa có hồ sơ tài xế.";
        if (driver.EmpStatus != EmployeeStatus.Active)
            return $"Tài xế {driver.FullName} hiện không ở trạng thái Đang làm việc — chưa thể xuất phát.";
        if (driver.LicenseExpiryDate.Date < today)
            return $"Bằng lái của tài xế {driver.FullName} đã hết hạn ({driver.LicenseExpiryDate:dd/MM/yyyy}) — chưa thể xuất phát.";

        // Tài xế phải có ĐĂNG KÝ NGÀY LÀM hôm nay (giống điều kiện Chia xe).
        var tomorrow = today.AddDays(1);
        var hasWorkDayToday = await _db.DriverSchedules.AnyAsync(s =>
            s.DriverEmployeeId == trip.DriverEmployeeId && s.ShiftDate >= today && s.ShiftDate < tomorrow);
        if (!hasWorkDayToday)
            return $"Tài xế {driver.FullName} chưa đăng ký ngày làm hôm nay ({today:dd/MM/yyyy}) — đăng ký trong màn Tài xế trước khi xuất phát.";

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

    public async Task<TripRouteEstimateDto> CalculateAndSaveRouteEstimateAsync(int tripId)
    {
        var trip = await _db.Trips
            .Include(t => t.TransportOrder).ThenInclude(o => o.PickupLocation)
            .Include(t => t.TransportOrder).ThenInclude(o => o.DropoffLocation)
            .Include(t => t.Warehouse)
            .FirstOrDefaultAsync(t => t.Id == tripId);

        if (trip is null)
            return new TripRouteEstimateDto(0, 0, "Không tìm thấy chuyến.");

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
            return new TripRouteEstimateDto(0, 0, "VietMap không tính được lộ trình (lỗi mạng hoặc hết quota) — thử lại sau.");

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

        // Bước 2: Chi phí THỰC TẾ — tổng các lần đổ xăng thực tế đã ghi (FuelLog)
        var actualCost = trip.ActualFuelCost;

        if (waypoints.Count < 2)
            return new FuelEstimateDto(actualDistanceKm, null, null, actualCost, null,
                "Chưa đủ dữ liệu GPS để tính khoảng cách (cần ít nhất 2 điểm ping) — chưa ước tính được.");

        if (trip.Vehicle.FuelConsumptionPer100Km is null || string.IsNullOrEmpty(trip.Vehicle.FuelType))
            return new FuelEstimateDto(actualDistanceKm, null, null, actualCost, null,
                "Xe này CHƯA khai báo định mức tiêu hao (L/100km) hoặc loại nhiên liệu — vào Sửa xe để khai báo trước khi ước tính được.");

        // Bước 3: Lấy giá nhiên liệu MỚI NHẤT (tính tới thời điểm chuyến kết
        // thúc) khớp đúng loại nhiên liệu xe này dùng — ưu tiên giá thị trường
        // chung (không giới hạn hãng) vì đa số xe không cố định 1 hãng đổ xăng.
        var referenceDate = trip.EndTime ?? DateTime.UtcNow;
        // Cùng 1 quy tắc tìm giá với ghi đổ xăng / báo cáo (không phân biệt hoa thường,
        // khoảng trắng; tính theo ngày VN; ưu tiên giá thị trường chung).
        var priceLookup = await FindFuelPriceAsync(trip.Vehicle.FuelType, null, referenceDate);
        var latestPrice = priceLookup.Found ? new { Price = priceLookup.UnitPrice } : null;

        if (latestPrice is null)
            return new FuelEstimateDto(actualDistanceKm, null, null, actualCost, null,
                $"Chưa có giá nhiên liệu '{trip.Vehicle.FuelType}' nào được nhập — vào màn Nhiên liệu để thêm giá trước khi ước tính được.");

        // Bước 4: Tính ước tính = (khoảng cách / 100) x định mức x đơn giá
        var estimatedLiters = (actualDistanceKm / 100m) * trip.Vehicle.FuelConsumptionPer100Km.Value;
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
        var blockReason = await GetTripCostBlockReasonAsync(tripId, "sự cố");
        if (blockReason is not null) throw new InvalidOperationException(blockReason);

        if (request.Cost.HasValue && request.Cost.Value < 0)
            throw new InvalidOperationException("Chi phí phát sinh không thể là số âm.");

        var type = Enum.TryParse<IncidentType>(request.Type, out var parsed) ? parsed : IncidentType.Other;

        var incident = new IncidentReport
        {
            TripId = tripId,
            Type = type,
            Description = request.Description,
            Cost = request.Cost,
            Timestamp = DateTime.UtcNow,
        };
        _db.IncidentReports.Add(incident);
        await _db.SaveChangesAsync();

        return new IncidentReportDto(incident.Id, incident.Type.ToString(), incident.Description, incident.Timestamp, incident.Cost);
    }

    public async Task<TripActionResult> DeleteIncidentAsync(int incidentId)
    {
        var incident = await _db.IncidentReports.FindAsync(incidentId);
        if (incident is null) return new TripActionResult(false, "Không tìm thấy sự cố.");

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
        var startUtc = trip.StartTime ?? committedUtc ?? DateTime.UtcNow.AddHours(1);
        var endUtc = trip.EndTime ?? startUtc.AddHours(2);

        var trackingLink = $"{publicTrackingBaseUrl.TrimEnd('/')}/track.html?token={order.PublicTrackingToken}";
        var description = $"Đơn hàng: {order.Code}\nChặng: {legText}\nĐịa điểm: {location}\n\n🔗 Theo dõi trực tiếp: {trackingLink}";
        var title = $"🚚 {legText} — Đơn {order.Code}";

        var driverCalendarId = trip.DriverEmployee.DriverProfile?.GoogleCalendarId;
        var (driverSynced, driverMessage) = string.IsNullOrWhiteSpace(driverCalendarId)
            ? (false, "Tài xế này chưa khai báo Google Calendar ID.")
            : await SyncOneCalendarAsync(tripId, driverCalendarId!, title, description, startUtc, endUtc, location);

        var customerCalendarId = order.Customer.GoogleCalendarId;
        var (customerSynced, customerMessage) = string.IsNullOrWhiteSpace(customerCalendarId)
            ? (false, "Khách hàng này chưa khai báo Google Calendar ID.")
            : await SyncOneCalendarAsync(tripId, customerCalendarId!, title, description, startUtc, endUtc, location);

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
        int tripId, string calendarId, string title, string description, DateTime startUtc, DateTime endUtc, string? location)
    {
        var existing = await _db.TripCalendarEvents.FirstOrDefaultAsync(e => e.TripId == tripId && e.CalendarId == calendarId);

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

        var type = Enum.TryParse<TripExpenseType>(request.Type, out var parsed) ? parsed : TripExpenseType.Other;

        var expense = new TripExpense
        {
            TripId = tripId,
            Type = type,
            Amount = request.Amount,
            Note = request.Note,
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
    private async Task<string?> GetTripCostBlockReasonAsync(int tripId, string what)
    {
        var trip = await _db.Trips
            .Where(t => t.Id == tripId)
            .Select(t => new { t.Status, t.StartTime })
            .FirstOrDefaultAsync();
        if (trip is null) return "Không tìm thấy chuyến.";
        if (!HasRun(trip.Status, trip.StartTime))
            return trip.Status == TripStatus.Cancelled
                ? $"Chuyến đã hủy khi chưa xuất phát — không ghi {what}."
                : $"Chuyến chưa bắt đầu chạy — chưa thể ghi {what}. Hãy ghi sau khi chuyến đã xuất phát.";
        return null;
    }

    /// <summary>Chuyến "đã từng chạy" = có giờ bắt đầu (kể cả đã hủy giữa đường).
    /// Dùng CHUNG cho xăng, chi phí, sự cố và báo cáo nhiên liệu.</summary>
    private static bool HasRun(TripStatus status, DateTime? startTime)
        => status != TripStatus.Planned && startTime.HasValue;

    public async Task<TripActionResult> DeleteExpenseAsync(int expenseId)
    {
        var expense = await _db.TripExpenses.FindAsync(expenseId);
        if (expense is null) return new TripActionResult(false, "Không tìm thấy chi phí.");

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
    ///     chặng Delivery ngay). Tự động ReceiveGoodsAsync (Best-Fit + nhập kho).
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
            // Xe về tới kho — hàng THẬT SỰ nhập kho tại đây: tạo GoodsReceipt,
            // tự chạy Best-Fit chọn vị trí, cộng WarehouseStock.
            await _db.SaveChangesAsync(); // lưu trạng thái Completed trước, tránh xung đột khi ReceiveGoodsAsync tự SaveChanges
            foreach (var line in trip.TripLines)
            {
                // Chỉ nhập PHẦN CÒN THIẾU của chuyến này — chống nhập kho 2 lần
                // nếu chuyến đã từng được hoàn tất/nhập kho trước đó.
                var alreadyReceived = await _db.GoodsReceipts
                    .Where(r => r.TripId == trip.Id && r.CargoLineId == line.CargoLineId)
                    .SumAsync(r => (decimal?)r.QtyReceived) ?? 0m;
                var qtyToReceive = line.QtyPlanned - alreadyReceived;
                if (qtyToReceive <= 0) continue;

                var receiveResult = await _warehouseService.ReceiveGoodsAsync(
                    trip.WarehouseId.Value, trip.TransportOrderId, line.CargoLineId,
                    qtyToReceive, trip.DriverEmployeeId, trip.Id);
                if (!receiveResult.Success)
                    return "Không thể nhập kho: " + receiveResult.Message;
            }
            // Xe vẫn Status = OnTrip (đang tiếp tục chặng Delivery), không đổi
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
                    .Where(g => g.TripId == trip.Id && g.CargoLineId == line.CargoLineId)
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

        if (order.ProgressPercent >= 100 && order.Status == TransportOrderStatus.InProgress)
            order.Status = TransportOrderStatus.Done;
    }
}
