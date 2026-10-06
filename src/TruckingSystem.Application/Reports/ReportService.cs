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

        var costs = await LoadCostsAsync(orderIds);
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
            breakdown, monthly, rows);
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
        var lines = (await LoadCostsAsync(new List<int> { orderId }))
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
    private async Task<List<RawCost>> LoadCostsAsync(List<int> orderIds)
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
        return result;
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
