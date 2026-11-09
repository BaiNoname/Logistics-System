using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using TruckingSystem.Application.Common.Interfaces;
using TruckingSystem.Domain.Entities;
using TruckingSystem.Domain.Enums;

namespace TruckingSystem.Application.Warehouses;

/// <summary>HÀNG CHỜ NHẬP KHO — 1 quy tắc DUY NHẤT dùng chung cho màn Hàng chờ nhập,
/// chặn bắt đầu chuyến Giao, chặn Hủy đơn, giới hạn nhập tay.
/// Chuyến Lấy hàng HOÀN TẤT = xe đã chở hàng về tới kho, nhưng hàng CHƯA lên kệ: Vận hành
/// nhập kho riêng (thủ công / Best-Fit). Phần còn chờ nhập của 1 dòng hàng trong 1 chuyến =
/// số kế hoạch của chuyến − tổng phiếu nhập GẮN ĐÚNG chuyến đó (không cần cột mới trong DB).</summary>
public static class PendingReceiptRules
{
    public record PendingLine(int TripId, int CargoLineId, decimal QtyPlanned, decimal QtyReceived)
    {
        public decimal QtyPending => Math.Max(0, QtyPlanned - QtyReceived);
    }

    /// <summary>Các dòng còn chờ nhập (QtyPending > 0) của chuyến Lấy hàng đã Hoàn tất,
    /// lọc thêm theo <paramref name="lineFilter"/> (VD theo kho, theo đơn, theo chuyến).</summary>
    public static async Task<List<PendingLine>> LoadAsync(IApplicationDbContext db, Expression<Func<TripLine, bool>> lineFilter)
    {
        var planned = await db.TripLines
            // WarehouseId null = dữ liệu cũ không gắn kho (bản cũ cũng không nhập kho cho chuyến này)
            // -> không tính là chờ nhập, tránh khóa đơn mãi mà không màn nào nhập được.
            .Where(l => l.Trip.LegType == TripLegType.Pickup && l.Trip.Status == TripStatus.Completed && l.Trip.WarehouseId != null)
            .Where(lineFilter)
            .GroupBy(l => new { l.TripId, l.CargoLineId })
            .Select(g => new { g.Key.TripId, g.Key.CargoLineId, Qty = g.Sum(x => x.QtyPlanned) })
            .ToListAsync();
        if (planned.Count == 0) return new List<PendingLine>();

        var tripIds = planned.Select(p => p.TripId).Distinct().ToList();
        var received = await db.GoodsReceipts
            .Where(r => r.TripId != null && tripIds.Contains(r.TripId.Value))
            .GroupBy(r => new { TripId = r.TripId!.Value, r.CargoLineId })
            .Select(g => new { g.Key.TripId, g.Key.CargoLineId, Qty = g.Sum(x => x.QtyReceived) })
            .ToListAsync();
        var receivedByKey = received.ToDictionary(r => (r.TripId, r.CargoLineId), r => r.Qty);

        return planned
            .Select(p => new PendingLine(p.TripId, p.CargoLineId, p.Qty,
                receivedByKey.TryGetValue((p.TripId, p.CargoLineId), out var q) ? q : 0))
            .Where(p => p.QtyPending > 0)
            .OrderBy(p => p.TripId).ThenBy(p => p.CargoLineId)
            .ToList();
    }

    /// <summary>Câu báo chung: "chuyến #12 (chờ nhập 5), chuyến #13 (chờ nhập 2)".</summary>
    public static string Describe(IEnumerable<PendingLine> lines)
        => string.Join(", ", lines.GroupBy(l => l.TripId)
            .Select(g => $"#{g.Key} (chờ nhập {g.Sum(x => x.QtyPending):0.##})"));
}
