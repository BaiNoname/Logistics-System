using Microsoft.EntityFrameworkCore;
using TruckingSystem.Application.Calendar;
using TruckingSystem.Application.Common.Interfaces;
using TruckingSystem.Application.Routing;
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
///   - Đơn SelfDelivery (không có chặng Pickup) -> nhập kho NGAY lúc Chia xe,
///     xem VehicleAllocationService.
/// </summary>
public class TripService : ITripService
{
    private readonly IApplicationDbContext _db;
    private readonly IWarehouseSlottingService _warehouseService;

    private readonly IGoogleCalendarService _calendarService;
    private readonly IRouteProvider _routeProvider;

    public TripService(IApplicationDbContext db, IWarehouseSlottingService warehouseService, IGoogleCalendarService calendarService, IRouteProvider routeProvider)
    {
        _db = db;
        _warehouseService = warehouseService;
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

        // Chặng Delivery bắt đầu chạy = hàng THẬT SỰ rời khỏi kho — xuất kho
        // ngay tại đây (không đợi tới lúc giao xong, vì giao xong đã sang xe khác chở đi rồi).
        if (trip.LegType == TripLegType.Delivery && trip.WarehouseId.HasValue)
        {
            foreach (var line in trip.TripLines)
            {
                var issueResult = await _warehouseService.IssueGoodsAsync(
                    trip.WarehouseId.Value, trip.TransportOrderId, line.CargoLineId,
                    line.QtyPlanned, trip.DriverEmployeeId, trip.Id);

                if (!issueResult.Success)
                    return new TripActionResult(false, "Không thể bắt đầu chuyến — " + issueResult.Message);
            }
        }

        trip.Status = TripStatus.InProgress;
        trip.StartTime = DateTime.UtcNow;

        if (trip.TransportOrder.Status == TransportOrderStatus.Confirmed)
            trip.TransportOrder.Status = TransportOrderStatus.InProgress;

        await _db.SaveChangesAsync();
        return new TripActionResult(true, null);
    }

    public async Task<TripActionResult> CompleteAsync(int id)
    {
        var trip = await LoadTripForArrivalAsync(id);
        if (trip is null) return new TripActionResult(false, "Không tìm thấy chuyến.");
        if (trip.Status != TripStatus.InProgress)
            return new TripActionResult(false, "Chỉ có thể hoàn tất chuyến đang ở trạng thái Đang chạy (InProgress).");

        await HandleArrivalAsync(trip);
        await _db.SaveChangesAsync();
        return new TripActionResult(true, null);
    }

    public async Task<TripActionResult> RecordFuelAsync(int id, RecordFuelRequest request)
    {
        var trip = await _db.Trips.FirstOrDefaultAsync(t => t.Id == id);
        if (trip is null) return new TripActionResult(false, "Không tìm thấy chuyến.");
        if (request.Liters <= 0 || request.Cost <= 0)
            return new TripActionResult(false, "Số lít và chi phí phải lớn hơn 0.");

        // CHẶN ghi xăng cho chuyến CHƯA từng chạy (Planned) hoặc đã hủy —
        // trước đây cho ghi ở BẤT KỲ trạng thái nào, kể cả chuyến chưa bắt đầu
        // hoặc đã hủy — không hợp lý (xe chưa chạy sao tốn xăng được).
        if (trip.Status is TripStatus.Planned or TripStatus.Cancelled)
            return new TripActionResult(false, "Chỉ ghi nhận đổ xăng cho chuyến ĐANG CHẠY hoặc ĐÃ HOÀN TẤT — chuyến này chưa từng chạy hoặc đã hủy.");

        _db.FuelLogs.Add(new FuelLog
        {
            TripId = id,
            Liters = request.Liters,
            Cost = request.Cost,
            StationName = request.StationName,
        });

        trip.ActualFuelCost += request.Cost;

        await _db.SaveChangesAsync();
        return new TripActionResult(true, null);
    }

    // ================= App tài xế (đã có từ trước) =================

    public async Task<ServiceResult> PingAsync(int tripId, GpsPingRequest request)
    {
        var trip = await _db.Trips.FirstOrDefaultAsync(t => t.Id == tripId);
        if (trip is null) return new ServiceResult(false, "Không tìm thấy chuyến.");

        _db.Waypoints.Add(new Waypoint
        {
            TripId = tripId,
            Lat = request.Lat,
            Lng = request.Lng,
            SpeedKmh = request.SpeedKmh,
            Accuracy = request.Accuracy,
            Timestamp = request.Timestamp ?? DateTime.UtcNow,
        });

        // Trip tự động chuyển InProgress ngay khi nhận điểm GPS đầu tiên (nếu còn Planned)
        if (trip.Status == TripStatus.Planned)
        {
            trip.Status = TripStatus.InProgress;
            trip.StartTime ??= DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
        return new ServiceResult(true, null);
    }

    public async Task<ServiceResult> ConfirmArrivalAsync(int tripId, ConfirmArrivalRequest request)
    {
        var trip = await LoadTripForArrivalAsync(tripId);
        if (trip is null) return new ServiceResult(false, "Không tìm thấy chuyến.");
        if (trip.Status != TripStatus.InProgress)
            return new ServiceResult(false, "Chuyến này không ở trạng thái đang chạy.");

        await HandleArrivalAsync(trip);
        await _db.SaveChangesAsync();
        return new ServiceResult(true, null);
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
    /// đúng kho); chặng Giao hàng chỉ 2 điểm (1 chiều, xong việc cho đơn này).</summary>
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
        var latestPrice = await _db.FuelPriceHistories
            .Where(p => p.FuelType == trip.Vehicle.FuelType && p.EffectiveDate <= referenceDate)
            .OrderByDescending(p => p.EffectiveDate)
            .FirstOrDefaultAsync();

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
        var legText = trip.LegType == TripLegType.Pickup ? "Lấy hàng" : "Giao hàng";
        var location = trip.LegType == TripLegType.Pickup ? order.PickupPoint : order.DropoffPoint;

        // Ưu tiên giờ THẬT nếu chuyến đã/đang chạy; chưa chạy thì dùng hẹn giao
        // của đơn làm mốc tạm; cuối cùng mới rơi về "1 giờ nữa" cho có giá trị
        // hiển thị hợp lý trên Calendar (không để trống ngày giờ).
        var startUtc = trip.StartTime ?? order.CommittedDeliveryDate ?? DateTime.UtcNow.AddHours(1);
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
    ///     xe được rảnh lại, có thể tự động chuyển đơn sang Done.</summary>
    private async Task HandleArrivalAsync(Trip trip)
    {
        trip.Status = TripStatus.Completed;
        trip.EndTime = DateTime.UtcNow;

        foreach (var line in trip.TripLines)
        {
            line.QtyDelivered = line.QtyPlanned;
            line.Status = TripLineStatus.Delivered;
        }

        if (trip.LegType == TripLegType.Delivery)
        {
            trip.Vehicle.Status = VehicleStatus.Available;
            RecalculateOrderProgress(trip.TransportOrder);
        }
        else if (trip.LegType == TripLegType.Pickup && trip.WarehouseId.HasValue)
        {
            // Xe về tới kho — hàng THẬT SỰ nhập kho tại đây: tạo GoodsReceipt,
            // tự chạy Best-Fit chọn vị trí, cộng WarehouseStock.
            await _db.SaveChangesAsync(); // lưu trạng thái Completed trước, tránh xung đột khi ReceiveGoodsAsync tự SaveChanges
            foreach (var line in trip.TripLines)
            {
                await _warehouseService.ReceiveGoodsAsync(
                    trip.WarehouseId.Value, trip.TransportOrderId, line.CargoLineId,
                    line.QtyPlanned, trip.DriverEmployeeId, trip.Id);
            }
            // Xe vẫn Status = OnTrip (đang tiếp tục chặng Delivery), không đổi
        }
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