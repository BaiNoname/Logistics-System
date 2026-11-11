using Microsoft.EntityFrameworkCore;
using TruckingSystem.Application.Common.Interfaces;
using TruckingSystem.Domain.Enums;

namespace TruckingSystem.Application.Reports;

public class ReportService : IReportService
{
    // CreatedAt/Timestamp lưu theo giờ UTC; lọc kỳ và gom theo tháng phải đổi
    // sang giờ Việt Nam, nếu không đơn tạo lúc 0h–7h sáng sẽ bị tính nhầm sang hôm trước.
    private static readonly TimeSpan VnOffset = TimeSpan.FromHours(7);

    private readonly IApplicationDbContext _db;

    /// <summary>Km GPS của chuyến đã chốt (hoàn tất / hủy) — không đổi nữa nên nhớ trong bộ nhớ máy chủ (mất khi khởi động lại,
    /// lần sau tính lại 1 lần).</summary>
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<int, decimal?> GpsKmCache = new();

    public ReportService(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<ProfitReportDto> GetProfitReportAsync(DateTime from, DateTime to)
    {
        if (to < from) (from, to) = (to, from);
        var fromUtc = from.Date - VnOffset;
        var toUtcExclusive = to.Date.AddDays(1) - VnOffset;

        var orders = await _db.TransportOrders
            .Where(o => o.CreatedAt >= fromUtc && o.CreatedAt < toUtcExclusive)
            .Select(o => new { o.Id, o.Code, CustomerName = o.Customer.Name, o.Status, o.CreatedAt, o.CommittedDeliveryDate })
            .ToListAsync();
        var orderIds = orders.Select(o => o.Id).ToList();

        // GIAO ĐÚNG HẠN (cùng quy tắc DeliveryDeadlineRules): đơn hoàn tất — ngày xong chặng Giao
        // CUỐI CÙNG (giờ VN) so với hạn giao cam kết.
        var lastDeliveryEnd = (await _db.Trips
                .Where(t => orderIds.Contains(t.TransportOrderId) && t.LegType == TripLegType.Delivery
                            && t.Status == TripStatus.Completed && t.EndTime != null)
                .Select(t => new { t.TransportOrderId, End = t.EndTime!.Value })
                .ToListAsync())
            .GroupBy(t => t.TransportOrderId)
            .ToDictionary(g => g.Key, g => TruckingSystem.Application.Trips.DeliveryDeadlineRules.UtcToVnDate(g.Max(x => x.End)));
        int? LateDays(int orderId, TransportOrderStatus status, DateTime? dueVn)
            => status == TransportOrderStatus.Done && dueVn is { } due && lastDeliveryEnd.TryGetValue(orderId, out var done)
                ? (done - due.Date).Days : null;

        // Doanh thu: hóa đơn của các đơn này, BỎ QUA hóa đơn đã hủy.
        var invoices = await _db.Invoices
            .Where(i => orderIds.Contains(i.TransportOrderId) && i.Status != InvoiceStatus.Cancelled)
            .Select(i => new { i.Id, i.TransportOrderId, i.TotalAmount })
            .ToListAsync();
        var revenueByOrder = invoices.GroupBy(i => i.TransportOrderId).ToDictionary(g => g.Key, g => g.Sum(x => x.TotalAmount));
        var invoiceIds = invoices.Select(i => i.Id).ToList();
        var collected = await _db.Payments.Where(p => invoiceIds.Contains(p.InvoiceId)).SumAsync(p => (decimal?)p.Amount) ?? 0;

        var fuelStats = await ComputeFuelStatsAsync(orderIds);
        var vehicleFuel = await ComputeVehicleFuelAsync(fromUtc, toUtcExclusive);
        var costs = await LoadCostsAsync(orderIds, fuelStats);
        var costsByOrder = costs.GroupBy(c => c.OrderId).ToDictionary(g => g.Key, g => g.ToList());

        var rows = orders.Select(o =>
        {
            var lines = costsByOrder.TryGetValue(o.Id, out var l) ? l : new List<RawCost>();
            var fuel = lines.Where(x => x.Kind == CostKind.Fuel).Sum(x => x.Amount);
            var expense = lines.Where(x => x.Kind == CostKind.Expense).Sum(x => x.Amount);
            var incident = lines.Where(x => x.Kind == CostKind.Incident).Sum(x => x.Amount);
            var hasInvoice = revenueByOrder.TryGetValue(o.Id, out var revenue);
            var total = fuel + expense + incident;
            var late = LateDays(o.Id, o.Status, o.CommittedDeliveryDate);
            return new OrderProfitDto(o.Id, o.Code, o.CustomerName, o.Status.ToString(), o.CreatedAt + VnOffset,
                hasInvoice, revenue, fuel, expense, incident, total, revenue - total,
                late is null ? null : late <= 0 ? "Đúng hạn" : $"Trễ {late} ngày");
        })
        .OrderBy(r => r.Profit) // đơn lỗ nặng nhất lên đầu — cần chú ý trước
        .ToList();

        var totalRevenue = rows.Sum(r => r.Revenue);
        var totalCost = rows.Sum(r => r.TotalCost);
        var profit = totalRevenue - totalCost;

        var breakdown = costs
            .GroupBy(c => c.Category)
            .Select(g => new CostCategoryDto(g.Key, g.Sum(x => x.Amount)))
            .OrderByDescending(c => c.Amount)
            .ToList();

        var monthly = rows
            .GroupBy(r => r.CreatedAt.ToString("yyyy-MM"))
            .OrderBy(g => g.Key)
            .Select(g => new MonthlyProfitDto(g.Key, g.Sum(x => x.Revenue), g.Sum(x => x.TotalCost)))
            .ToList();

        var cancelled = rows.Where(r => r.Status == nameof(TransportOrderStatus.Cancelled)).ToList();

        return new ProfitReportDto(
            from.Date, to.Date,
            totalRevenue, collected,
            rows.Sum(r => r.FuelCost), rows.Sum(r => r.ExpenseCost), rows.Sum(r => r.IncidentCost), totalCost,
            profit, totalRevenue > 0 ? Math.Round(profit / totalRevenue * 100, 1) : null,
            rows.Count,
            rows.Count(r => r.Status == nameof(TransportOrderStatus.Done)),
            cancelled.Count,
            // Đơn đã giao xong nhưng CHƯA xuất hóa đơn: doanh thu đang = 0 nên trông
            // như lỗ — tách riêng để không hiểu nhầm, và nhắc xuất hóa đơn.
            rows.Count(r => r.Status == nameof(TransportOrderStatus.Done) && !r.HasInvoice),
            // Chi phí đã bỏ ra cho đơn bị hủy (xe đã chạy, chuyến Trả hàng...) — tiền mất trắng.
            cancelled.Sum(r => r.TotalCost),
            breakdown, monthly, rows,
            // ---- Kiểm soát nhiên liệu: 3 con số + bảng từng chuyến ----
            fuelStats.Sum(s => s.PlannedCost ?? 0),
            fuelStats.Sum(s => s.NormCost ?? 0),
            vehicleFuel.Sum(v => v.RefuelCost),   // xăng ĐÃ MUA trong kỳ (mọi xe, theo ngày đổ)
            fuelStats.Count(s => s.UsesEstimate),
            fuelStats.Select(s => s.ToDto()).OrderByDescending(r => r.Flagged).ThenBy(r => r.TripId).ToList(),
            // "Lệch dự toán" chỉ so trên CÙNG 1 tập chuyến: chuyến hoàn tất có ĐỦ cả dự toán
            // (km VietMap) lẫn định mức (km GPS) — trước đây 2 tổng lấy từ 2 tập chuyến khác nhau.
            fuelStats.Where(IsComparable).Sum(s => s.PlannedCost!.Value),
            fuelStats.Where(IsComparable).Sum(s => s.NormCost!.Value),
            fuelStats.Count(IsComparable),
            rows.Count(r => r.DeliveryResult == "Đúng hạn"),
            rows.Count(r => r.DeliveryResult is not null && r.DeliveryResult != "Đúng hạn"),
            orders.Count(o => o.Status == TransportOrderStatus.Done && o.CommittedDeliveryDate is null),
            vehicleFuel.Sum(v => v.RefuelLiters),
            // Lít tính vào đơn: theo km GPS; chuyến thiếu GPS tạm theo km dự kiến (cùng quy tắc tiền xăng của đơn).
            Math.Round(fuelStats.Sum(s => s.NormLiters
                ?? (s.UsesEstimate && s.PlannedKm is decimal pk && s.ConsumptionPer100Km is decimal c ? pk / 100m * c : 0)), 1),
            vehicleFuel);
    }

    private static bool IsComparable(TripFuelStats s)
        => s.Status == TripStatus.Completed && s.PlannedCost.HasValue && s.NormCost.HasValue;

    public async Task<OrderCostDetailDto?> GetOrderCostDetailAsync(int orderId)
    {
        var order = await _db.TransportOrders
            .Include(o => o.Customer)
            .Include(o => o.Trips)
            .FirstOrDefaultAsync(o => o.Id == orderId);
        if (order is null) return null;

        var revenue = await _db.Invoices
            .Where(i => i.TransportOrderId == orderId && i.Status != InvoiceStatus.Cancelled)
            .SumAsync(i => (decimal?)i.TotalAmount) ?? 0;

        var trips = order.Trips.ToDictionary(t => t.Id);
        var lines = (await LoadCostsAsync(new List<int> { orderId }, await ComputeFuelStatsAsync(new List<int> { orderId })))
            .OrderBy(c => c.Date)
            .Select(c => new CostLineDto(
                c.TripId,
                trips.TryGetValue(c.TripId, out var t) ? LegName(t.LegType) : "",
                trips.TryGetValue(c.TripId, out var t2) ? t2.Status.ToString() : "",
                c.Category, c.Description, c.Date + VnOffset, c.Amount))
            .ToList();

        return new OrderCostDetailDto(order.Id, order.Code, order.Customer.Name, order.Status.ToString(),
            revenue, lines.Sum(l => l.Amount), lines);
    }

    // ================= Gom chi phí từ 3 nguồn =================

    private enum CostKind { Fuel, Expense, Incident }

    private sealed record RawCost(int OrderId, int TripId, CostKind Kind, string Category, string Description, DateTime Date, decimal Amount);

    /// <summary>Gom TẤT CẢ khoản chi của các đơn: nhiên liệu thực tế đã đổ
    /// (FuelLog), chi phí phát sinh (TripExpense), chi phí sự cố (IncidentReport
    /// có ghi số tiền) — của mọi chuyến thuộc các đơn đó.</summary>
    private async Task<List<RawCost>> LoadCostsAsync(List<int> orderIds, List<TripFuelStats> fuelStats)
    {
        var trips = await _db.Trips
            .Where(t => orderIds.Contains(t.TransportOrderId))
            .Select(t => new { t.Id, t.TransportOrderId })
            .ToListAsync();
        var orderOfTrip = trips.ToDictionary(t => t.Id, t => t.TransportOrderId);
        var tripIds = trips.Select(t => t.Id).ToList();

        var expenses = await _db.TripExpenses.Where(e => tripIds.Contains(e.TripId))
            .Select(e => new { e.TripId, e.Type, e.Amount, e.Note, e.CreatedAt }).ToListAsync();
        var incidents = await _db.IncidentReports.Where(i => tripIds.Contains(i.TripId) && i.Cost != null && i.Cost > 0)
            .Select(i => new { i.TripId, i.Type, i.Cost, i.Description, i.Timestamp }).ToListAsync();

        var result = new List<RawCost>();
        result.AddRange(expenses.Select(e => new RawCost(orderOfTrip[e.TripId], e.TripId, CostKind.Expense, ExpenseName(e.Type),
            string.IsNullOrWhiteSpace(e.Note) ? ExpenseName(e.Type) : e.Note!, e.CreatedAt, e.Amount)));
        result.AddRange(incidents.Select(i => new RawCost(orderOfTrip[i.TripId], i.TripId, CostKind.Incident, "Sự cố: " + IncidentName(i.Type),
            string.IsNullOrWhiteSpace(i.Description) ? IncidentName(i.Type) : i.Description!, i.Timestamp, i.Cost!.Value)));

        // NHIÊN LIỆU CỦA CHUYẾN = xăng chuyến đó THỰC TIÊU (km GPS × định mức × giá), KHÔNG phải tiền các lần đổ ghi vào
        // chuyến: đổ đầy bình ở chuyến A (chạy 10 km) thì xăng còn lại chạy tiếp chuyến B, C… — trước đây đơn của A gánh
        // cả bình (lỗ giả), còn B "chưa nhập hóa đơn" lại bị cộng thêm tiền ước tính (tính trùng). Tiền đổ xăng là tiền
        // mua vào bình xe -> quyết toán theo xe (ComputeVehicleFuelAsync). Chuyến thiếu GPS: tạm theo km tuyến dự kiến.
        foreach (var s in fuelStats.Where(s => orderOfTrip.ContainsKey(s.TripId)))
        {
            if (s.NormCost is > 0)
                result.Add(new RawCost(orderOfTrip[s.TripId], s.TripId, CostKind.Fuel, "Nhiên liệu",
                    $"{s.GpsKm:0.#} km GPS × {s.ConsumptionPer100Km:0.#} L/100km = {s.NormLiters:0.#} L × {s.UnitPrice:N0} đ/L",
                    s.TripDate, Math.Round(s.NormCost.Value, 0)));
            else if (s.UsesEstimate && s.PlannedCost is > 0)
                result.Add(new RawCost(orderOfTrip[s.TripId], s.TripId, CostKind.Fuel, "Nhiên liệu (tạm tính)",
                    $"Thiếu dữ liệu GPS — tạm tính {s.PlannedKm:0.#} km dự kiến × {s.ConsumptionPer100Km:0.#} L/100km × {s.UnitPrice:N0} đ/L",
                    s.TripDate, Math.Round(s.PlannedCost.Value, 0)));
        }
        return result;
    }

    // ================= Kiểm soát nhiên liệu =================

    /// <summary>Số liệu nhiên liệu của 1 chuyến đã chạy:
    ///   - Dự toán (VietMap): km tuyến dự kiến × định mức × giá
    ///   - Định mức (GPS):    km xe THẬT SỰ chạy (cộng dồn điểm GPS) × định mức × giá
    ///   - Thực đổ:           tổng các lần ghi đổ xăng (hóa đơn)
    /// Giá = giá mới nhất (tính tới ngày chạy) trong bảng giá nhiên liệu, đúng loại
    /// nhiên liệu của xe; ưu tiên giá thị trường chung, không có thì giá hãng.</summary>
    private sealed record TripFuelStats(
        int TripId, string OrderCode, TripLegType LegType, string PlateNumber, TripStatus Status, DateTime TripDate,
        decimal? PlannedKm, decimal? GpsKm, decimal? ConsumptionPer100Km, decimal? UnitPrice,
        decimal? PlannedCost, decimal? NormCost, decimal ActualCost, decimal ActualLiters, string? Note)
    {
        public bool HasActual => ActualCost > 0;
        /// <summary>Thiếu km GPS -> xăng của chuyến tạm tính theo km tuyến dự kiến.</summary>
        public bool UsesEstimate => NormCost is null && PlannedCost is > 0;

        public decimal? NormLiters => GpsKm.HasValue && ConsumptionPer100Km.HasValue
            ? Math.Round(GpsKm.Value / 100m * ConsumptionPer100Km.Value, 1) : null;

        /// <summary>% km GPS lệch so với km VietMap (dương = chạy dài hơn kế hoạch).</summary>
        public decimal? KmDeviationPct => PlannedKm is > 0 && GpsKm.HasValue
            ? Math.Round((GpsKm.Value - PlannedKm.Value) / PlannedKm.Value * 100, 1) : null;

        /// <summary>% tiền thực đổ lệch so với định mức GPS (dương = đổ nhiều hơn định mức).</summary>
        public decimal? CostDeviationPct => HasActual && NormCost is > 0
            ? Math.Round((ActualCost - NormCost.Value) / NormCost.Value * 100, 1) : null;

        public FuelControlRowDto ToDto() => new(
            TripId, OrderCode, LegName(LegType), PlateNumber, Status.ToString(), TripDate + VnOffset,
            PlannedKm, GpsKm, KmDeviationPct, ConsumptionPer100Km, UnitPrice,
            PlannedCost.HasValue ? Math.Round(PlannedCost.Value, 0) : null,
            NormCost.HasValue ? Math.Round(NormCost.Value, 0) : null,
            NormLiters, ActualCost, ActualLiters, CostDeviationPct, HasActual,
            // Chỉ gắn cờ chuyến ĐÃ HOÀN TẤT — chuyến đang chạy / chờ duyệt / hủy giữa đường
            // có km GPS dở dang, so với km kế hoạch luôn "thiếu" -> cờ đỏ oan.
            // Chỉ so KM (chạy vòng / sai tuyến / mất sóng). Tiền đổ xăng KHÔNG so theo chuyến — quyết toán theo xe.
            // Quãng ngắn: lệch vài trăm mét đã thành hàng trăm % -> phải lệch QUÁ 15% VÀ từ 2 km trở lên mới gắn cờ.
            Status == TripStatus.Completed && Math.Abs(KmDeviationPct ?? 0) > FuelDeviationThresholdPct
                && PlannedKm.HasValue && GpsKm.HasValue && Math.Abs(GpsKm.Value - PlannedKm.Value) >= FuelDeviationMinKm,
            Note);
    }

    /// <summary>Ngưỡng cảnh báo lệch (%). GPS hụt ~2–5% ở khúc cua/mất sóng nên
    /// không bắt lỗi từng phần trăm — chỉ đánh dấu khi lệch quá 15%.</summary>
    private const decimal FuelDeviationThresholdPct = 15m;

    /// <summary>Lệch tối thiểu (km) mới gắn cờ chuyến — tránh cờ đỏ oan ở chuyến quãng ngắn.</summary>
    private const decimal FuelDeviationMinKm = 2m;

    /// <summary>Km GPS dưới mức này coi như CHƯA ĐỦ dữ liệu GPS (các điểm trùng một chỗ, xe chưa thật sự chạy).</summary>
    private const decimal GpsMinKm = 0.1m;

    /// <summary>Bảng xăng theo xe: định mức dưới mức này (lít) thì quá ít để so % (đổ 2 L / định mức 0,04 L = +4000%).</summary>
    private const decimal VehicleFuelMinNormLiters = 20m;

    private async Task<List<TripFuelStats>> ComputeFuelStatsAsync(List<int> orderIds)
    {
        // Chỉ chuyến ĐÃ CHẠY (có giờ bắt đầu) — chuyến chưa chạy không tốn nhiên liệu.
        var trips = await _db.Trips
            .Where(t => orderIds.Contains(t.TransportOrderId) && t.StartTime != null)
            .Select(t => new
            {
                t.Id,
                OrderCode = t.TransportOrder.Code,
                t.LegType,
                t.Status,
                t.StartTime,
                t.EndTime,
                t.EstimatedDistanceKm,
                t.Vehicle.PlateNumber,
                // Số liệu CHỐT lúc xuất phát; chuyến cũ (chưa chốt) thì theo xe hiện tại.
                FuelConsumptionPer100Km = t.FuelConsumptionPer100KmAtStart ?? t.Vehicle.FuelConsumptionPer100Km,
                FuelType = t.FuelTypeAtStart ?? t.Vehicle.FuelType,
                t.FuelUnitPriceAtStart,
            })
            .ToListAsync();
        if (trips.Count == 0) return new List<TripFuelStats>();
        var tripIds = trips.Select(t => t.Id).ToList();

        var gpsKmByTrip = await LoadGpsKmAsync(trips.Select(t => (t.Id, t.Status)).ToList());

        var fuelLogs = await _db.FuelLogs
            .Where(f => tripIds.Contains(f.TripId))
            .GroupBy(f => f.TripId)
            .Select(g => new { TripId = g.Key, Cost = g.Sum(f => f.Cost), Liters = g.Sum(f => f.Liters) })
            .ToListAsync();
        var fuelByTrip = fuelLogs.ToDictionary(f => f.TripId);

        // Bảng giá nhỏ -> tải 1 lần, chọn giá trong bộ nhớ.
        var prices = await _db.FuelPriceHistories
            .Select(p => new { p.Id, p.FuelType, p.Price, p.EffectiveDate, p.FuelProviderId })
            .ToListAsync();

        var result = new List<TripFuelStats>();
        foreach (var t in trips)
        {
            var tripDate = t.EndTime ?? t.StartTime ?? DateTime.UtcNow;
            // Mốc lấy GIÁ: NGÀY BẮT ĐẦU chuyến — CÙNG mốc với lúc ghi đổ xăng và nút So sánh
            // xăng (cố định, không đổi khi chuyến kéo dài nhiều ngày / bị từ chối xác nhận).
            var priceDate = t.StartTime ?? DateTime.UtcNow;
            gpsKmByTrip.TryGetValue(t.Id, out var gpsKm);
            fuelByTrip.TryGetValue(t.Id, out var actual);

            string? note = null;
            decimal? unitPrice = null;
            if (t.FuelConsumptionPer100Km is null or <= 0) note = "Xe chưa khai báo định mức L/100km";
            else if (string.IsNullOrWhiteSpace(t.FuelType)) note = "Xe chưa chọn loại nhiên liệu";
            else if (t.FuelUnitPriceAtStart is decimal pinned)
            {
                unitPrice = pinned; // giá đã CHỐT lúc xuất phát — bảng giá sửa về sau không ảnh hưởng
            }
            else
            {
                var fuelType = t.FuelType.Trim();
                // Giá có hiệu lực tới NGÀY chạy (giờ VN) — EffectiveDate nhập theo ngày VN.
                var refDateVn = (priceDate + VnOffset).Date.AddDays(1);
                // Cùng quy tắc với ghi đổ xăng / nút Ước tính: ưu tiên giá thị trường chung
                // mới nhất; chưa có thì giá mới nhất của bất kỳ hãng nào.
                var candidates = prices
                    .Where(p => string.Equals(p.FuelType.Trim(), fuelType, StringComparison.OrdinalIgnoreCase) && p.EffectiveDate < refDateVn)
                    .OrderByDescending(p => p.EffectiveDate)
                    .ThenByDescending(p => p.Id)
                    .ToList();
                unitPrice = (candidates.FirstOrDefault(p => p.FuelProviderId == null) ?? candidates.FirstOrDefault())?.Price;
                if (unitPrice is null) note = $"Chưa có giá \"{fuelType}\" tới ngày chạy";
            }
            if (note is null && gpsKm is null) note = t.EstimatedDistanceKm is > 0
                ? "Chưa đủ dữ liệu GPS (xe chưa di chuyển / dưới 2 điểm) — tạm tính theo km dự kiến"
                : "Chưa đủ dữ liệu GPS (xe chưa di chuyển / dưới 2 điểm)";

            decimal? Cost(decimal? km) => km.HasValue && unitPrice.HasValue && t.FuelConsumptionPer100Km is > 0
                ? km.Value / 100m * t.FuelConsumptionPer100Km.Value * unitPrice.Value : null;

            result.Add(new TripFuelStats(
                t.Id, t.OrderCode, t.LegType, t.PlateNumber, t.Status, tripDate,
                t.EstimatedDistanceKm, gpsKm, t.FuelConsumptionPer100Km, unitPrice,
                Cost(t.EstimatedDistanceKm), Cost(gpsKm),
                actual?.Cost ?? 0, actual?.Liters ?? 0, note));
        }
        return result;
    }

    /// <summary>Km GPS từng chuyến (cộng dồn khoảng cách các điểm GPS; cần ≥ 2 điểm). Chuyến đã chốt thì nhớ trong GpsKmCache.</summary>
    private async Task<Dictionary<int, decimal?>> LoadGpsKmAsync(List<(int Id, TripStatus Status)> trips)
    {
        var tripIds = trips.Select(t => t.Id).ToList();
        // Km GPS: chuyến ĐÃ HOÀN TẤT / ĐÃ HỦY thì không còn điểm GPS mới -> tính 1 lần rồi nhớ (GpsKmCache). Chỉ tải điểm
        // GPS của chuyến chưa có trong bộ nhớ — trước đây MỖI lần mở / đổi kỳ ở màn Tổng quan đều tải lại TOÀN BỘ điểm GPS
        // của mọi chuyến trong kỳ (kỳ "Năm nay" có thể vài trăm nghìn điểm), nay chỉ lần đầu.
        var gpsKmByTrip = new Dictionary<int, decimal?>();
        var closed = trips.Where(t => t.Status is TripStatus.Completed or TripStatus.Cancelled).Select(t => t.Id).ToHashSet();
        foreach (var id in closed)
            if (GpsKmCache.TryGetValue(id, out var cached)) gpsKmByTrip[id] = cached;
        var needIds = tripIds.Where(id => !gpsKmByTrip.ContainsKey(id)).ToList();
        if (needIds.Count > 0)
        {
            var waypoints = await _db.Waypoints
                .Where(w => needIds.Contains(w.TripId))
                .OrderBy(w => w.Timestamp)
                .Select(w => new { w.TripId, w.Lat, w.Lng })
                .ToListAsync();
            var byTrip = waypoints.GroupBy(w => w.TripId).ToDictionary(g => g.Key, g => g.ToList());
            foreach (var id in needIds)
            {
                decimal? km = null;
                if (byTrip.TryGetValue(id, out var pts) && pts.Count >= 2)
                {
                    double sum = 0;
                    for (var i = 1; i < pts.Count; i++) sum += HaversineKm(pts[i - 1].Lat, pts[i - 1].Lng, pts[i].Lat, pts[i].Lng);
                    km = Math.Round((decimal)sum, 1);
                    if (km < GpsMinKm) km = null; // điểm GPS trùng một chỗ -> coi như chưa đủ dữ liệu GPS
                }
                if (km is not null) gpsKmByTrip[id] = km;
                if (closed.Contains(id)) GpsKmCache[id] = km;
            }
        }

        return gpsKmByTrip;
    }

    /// <summary>XĂNG THEO XE trong kỳ [fromUtc, toUtc) — tham khảo, xem VehicleFuelRowDto.
    /// Chuyến tính theo GIỜ XUẤT PHÁT trong kỳ; lần đổ xăng theo giờ ghi đổ.</summary>
    private async Task<List<VehicleFuelRowDto>> ComputeVehicleFuelAsync(DateTime fromUtc, DateTime toUtc)
    {
        var refuels = await _db.FuelLogs
            .Where(f => f.Timestamp >= fromUtc && f.Timestamp < toUtc)
            .Select(f => new { f.Trip.VehicleId, f.Liters, f.Cost })
            .ToListAsync();
        var trips = await _db.Trips
            .Where(t => t.StartTime != null && t.StartTime >= fromUtc && t.StartTime < toUtc)
            .Select(t => new
            {
                t.Id, t.VehicleId, t.Status,
                Norm = t.FuelConsumptionPer100KmAtStart ?? t.Vehicle.FuelConsumptionPer100Km,
            })
            .ToListAsync();
        var vehicleIds = refuels.Select(r => r.VehicleId).Concat(trips.Select(t => t.VehicleId)).Distinct().ToList();
        if (vehicleIds.Count == 0) return new List<VehicleFuelRowDto>();
        var vehicles = await _db.Vehicles.Where(v => vehicleIds.Contains(v.Id))
            .Select(v => new { v.Id, v.PlateNumber, v.FuelConsumptionPer100Km }).ToListAsync();
        var gpsKm = await LoadGpsKmAsync(trips.Select(t => (t.Id, t.Status)).ToList());

        var result = new List<VehicleFuelRowDto>();
        foreach (var v in vehicles)
        {
            var vt = trips.Where(t => t.VehicleId == v.Id).ToList();
            decimal km = 0, normL = 0;
            var noGps = 0;
            foreach (var t in vt)
            {
                if (gpsKm.TryGetValue(t.Id, out var k) && k is decimal tk)
                {
                    km += tk;
                    normL += tk / 100m * (t.Norm ?? v.FuelConsumptionPer100Km ?? 0);
                }
                else noGps++;
            }
            var rf = refuels.Where(r => r.VehicleId == v.Id).ToList();
            var refuelL = rf.Sum(r => r.Liters);
            // Chỉ so % khi đủ dữ liệu: không chuyến nào thiếu GPS và định mức đủ lớn (quãng ngắn -> % vô nghĩa).
            decimal? dev = noGps == 0 && normL >= VehicleFuelMinNormLiters
                ? Math.Round((refuelL - normL) / normL * 100, 1) : null;
            string? note = v.FuelConsumptionPer100Km is null or <= 0 ? "Xe chưa khai báo định mức L/100km"
                : noGps > 0 ? $"{noGps} chuyến thiếu GPS — chưa so được"
                : normL < VehicleFuelMinNormLiters ? "Quãng đường quá ngắn để so"
                : dev is decimal d && Math.Abs(d) > FuelDeviationThresholdPct
                    ? (d > 0 ? "Đổ nhiều hơn định mức cả kỳ — có thể còn xăng trong bình, theo dõi thêm"
                             : "Đổ ít hơn định mức cả kỳ — có thể dùng xăng còn từ kỳ trước, hoặc thiếu hóa đơn đổ")
                : null;
            result.Add(new VehicleFuelRowDto(v.Id, v.PlateNumber, v.FuelConsumptionPer100Km,
                Math.Round(refuelL, 1), Math.Round(rf.Sum(r => r.Cost), 0),
                Math.Round(km, 1), Math.Round(normL, 1), vt.Count, noGps, dev, note));
        }
        return result.OrderBy(r => r.PlateNumber).ToList();
    }

    private static double HaversineKm(double lat1, double lng1, double lat2, double lng2)
    {
        const double r = 6371.0;
        var dLat = (lat2 - lat1) * Math.PI / 180.0;
        var dLng = (lng2 - lng1) * Math.PI / 180.0;
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(lat1 * Math.PI / 180.0) * Math.Cos(lat2 * Math.PI / 180.0) * Math.Sin(dLng / 2) * Math.Sin(dLng / 2);
        return r * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }

    private static string ExpenseName(TripExpenseType t) => t switch
    {
        TripExpenseType.Toll => "Phí cầu đường",
        TripExpenseType.Parking => "Phí bến bãi",
        TripExpenseType.Loading => "Phí bốc xếp",
        TripExpenseType.Fine => "Tiền phạt",
        _ => "Chi phí khác",
    };

    private static string IncidentName(IncidentType t) => t switch
    {
        IncidentType.Accident => "Tai nạn",
        IncidentType.Breakdown => "Hỏng xe",
        IncidentType.TrafficViolation => "Vi phạm giao thông",
        _ => "Khác",
    };

    private static string LegName(TripLegType t) => t switch
    {
        TripLegType.Pickup => "Lấy hàng",
        TripLegType.Return => "Trả hàng",
        _ => "Giao hàng",
    };
}
