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
            .Select(o => new { o.Id, o.Code, CustomerName = o.Customer.Name, o.Status, o.CreatedAt })
            .ToListAsync();
        var orderIds = orders.Select(o => o.Id).ToList();

        // Doanh thu: hóa đơn của các đơn này, BỎ QUA hóa đơn đã hủy.
        var invoices = await _db.Invoices
            .Where(i => orderIds.Contains(i.TransportOrderId) && i.Status != InvoiceStatus.Cancelled)
            .Select(i => new { i.Id, i.TransportOrderId, i.TotalAmount })
            .ToListAsync();
        var revenueByOrder = invoices.GroupBy(i => i.TransportOrderId).ToDictionary(g => g.Key, g => g.Sum(x => x.TotalAmount));
        var invoiceIds = invoices.Select(i => i.Id).ToList();
        var collected = await _db.Payments.Where(p => invoiceIds.Contains(p.InvoiceId)).SumAsync(p => (decimal?)p.Amount) ?? 0;

        var fuelStats = await ComputeFuelStatsAsync(orderIds);
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
            return new OrderProfitDto(o.Id, o.Code, o.CustomerName, o.Status.ToString(), o.CreatedAt + VnOffset,
                hasInvoice, revenue, fuel, expense, incident, total, revenue - total);
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
            fuelStats.Sum(s => s.ActualCost),
            fuelStats.Count(s => s.UsesEstimate),
            fuelStats.Select(s => s.ToDto()).OrderByDescending(r => r.Flagged).ThenBy(r => r.TripId).ToList());
    }

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

        var fuel = await _db.FuelLogs.Where(f => tripIds.Contains(f.TripId))
            .Select(f => new { f.TripId, f.Cost, f.Liters, f.StationName, f.Timestamp }).ToListAsync();
        var expenses = await _db.TripExpenses.Where(e => tripIds.Contains(e.TripId))
            .Select(e => new { e.TripId, e.Type, e.Amount, e.Note, e.CreatedAt }).ToListAsync();
        var incidents = await _db.IncidentReports.Where(i => tripIds.Contains(i.TripId) && i.Cost != null && i.Cost > 0)
            .Select(i => new { i.TripId, i.Type, i.Cost, i.Description, i.Timestamp }).ToListAsync();

        var result = new List<RawCost>();
        result.AddRange(fuel.Select(f => new RawCost(orderOfTrip[f.TripId], f.TripId, CostKind.Fuel, "Nhiên liệu",
            $"Đổ {f.Liters:0.##} lít" + (string.IsNullOrWhiteSpace(f.StationName) ? "" : $" tại {f.StationName}"),
            f.Timestamp, f.Cost)));
        result.AddRange(expenses.Select(e => new RawCost(orderOfTrip[e.TripId], e.TripId, CostKind.Expense, ExpenseName(e.Type),
            string.IsNullOrWhiteSpace(e.Note) ? ExpenseName(e.Type) : e.Note!, e.CreatedAt, e.Amount)));
        result.AddRange(incidents.Select(i => new RawCost(orderOfTrip[i.TripId], i.TripId, CostKind.Incident, "Sự cố: " + IncidentName(i.Type),
            string.IsNullOrWhiteSpace(i.Description) ? IncidentName(i.Type) : i.Description!, i.Timestamp, i.Cost!.Value)));

        // Chuyến ĐÃ CHẠY nhưng CHƯA nhập hóa đơn xăng nào -> tạm tính chi phí nhiên
        // liệu theo ĐỊNH MỨC (km GPS thực tế × L/100km × giá) — tránh lãi/lỗ "đẹp giả"
        // do thiếu tiền xăng. Có hóa đơn thì chỉ dùng số thực đổ ở trên.
        result.AddRange(fuelStats
            .Where(s => s.UsesEstimate && s.NormCost is > 0 && orderOfTrip.ContainsKey(s.TripId))
            .Select(s => new RawCost(orderOfTrip[s.TripId], s.TripId, CostKind.Fuel, "Nhiên liệu (ước tính)",
                $"Chưa nhập hóa đơn xăng — tạm tính {s.GpsKm:0.#} km GPS × {s.ConsumptionPer100Km:0.#} L/100km × {s.UnitPrice:N0} đ/L",
                s.TripDate, Math.Round(s.NormCost!.Value, 0))));
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
        public bool UsesEstimate => !HasActual && NormCost is > 0;

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
            Math.Abs(KmDeviationPct ?? 0) > FuelDeviationThresholdPct || Math.Abs(CostDeviationPct ?? 0) > FuelDeviationThresholdPct,
            Note);
    }

    /// <summary>Ngưỡng cảnh báo lệch (%). GPS hụt ~2–5% ở khúc cua/mất sóng nên
    /// không bắt lỗi từng phần trăm — chỉ đánh dấu khi lệch quá 15%.</summary>
    private const decimal FuelDeviationThresholdPct = 15m;

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
                t.Vehicle.FuelConsumptionPer100Km,
                t.Vehicle.FuelType,
            })
            .ToListAsync();
        if (trips.Count == 0) return new List<TripFuelStats>();
        var tripIds = trips.Select(t => t.Id).ToList();

        var waypoints = await _db.Waypoints
            .Where(w => tripIds.Contains(w.TripId))
            .OrderBy(w => w.Timestamp)
            .Select(w => new { w.TripId, w.Lat, w.Lng })
            .ToListAsync();
        var gpsKmByTrip = waypoints.GroupBy(w => w.TripId).ToDictionary(g => g.Key, g =>
        {
            var pts = g.ToList();
            double km = 0;
            for (var i = 1; i < pts.Count; i++) km += HaversineKm(pts[i - 1].Lat, pts[i - 1].Lng, pts[i].Lat, pts[i].Lng);
            return pts.Count >= 2 ? (decimal?)Math.Round((decimal)km, 1) : null;
        });

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
            gpsKmByTrip.TryGetValue(t.Id, out var gpsKm);
            fuelByTrip.TryGetValue(t.Id, out var actual);

            string? note = null;
            decimal? unitPrice = null;
            if (t.FuelConsumptionPer100Km is null or <= 0) note = "Xe chưa khai báo định mức L/100km";
            else if (string.IsNullOrWhiteSpace(t.FuelType)) note = "Xe chưa chọn loại nhiên liệu";
            else
            {
                var fuelType = t.FuelType.Trim();
                // Giá có hiệu lực tới NGÀY chạy (giờ VN) — EffectiveDate nhập theo ngày VN.
                var refDateVn = (tripDate + VnOffset).Date.AddDays(1);
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
            if (note is null && gpsKm is null) note = "Chưa đủ dữ liệu GPS (cần ít nhất 2 điểm)";

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
