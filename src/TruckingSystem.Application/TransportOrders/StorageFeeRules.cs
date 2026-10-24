using Microsoft.EntityFrameworkCore;
using TruckingSystem.Application.Common;
using TruckingSystem.Application.Common.Interfaces;
using TruckingSystem.Domain.Enums;

namespace TruckingSystem.Application.TransportOrders;

/// <summary>PHÍ LƯU KHO do KHÁCH dời hạn giao — quy tắc DUY NHẤT (ước tính lúc đổi hạn + số chính thức):
///  • Chỉ tính lần đổi do KHÁCH HÀNG yêu cầu và dời TRỄ hơn hạn cũ. Công ty yêu cầu (công ty trễ) = 0 đ.
///  • Ngày tính phí: từ ngày SAU hạn cũ tới hạn mới (tối đa = số ngày khách xin dời), và chỉ những ngày
///    hàng của đơn THỰC SỰ còn trong kho (xuất lên xe sớm hơn thì thôi tính).
///  • Mỗi ngày: phí = số m³ hàng của đơn còn trong kho cuối ngày đó × đơn giá (đ/m³/ngày) của kho, đơn giá
///    CHỐT lúc đổi hạn. Một ngày chỉ tính 1 lần (khách dời nhiều lần chồng ngày cũng không tính trùng).
///  • Hạn giao về sau bị rút ngắn lại thì những ngày sau hạn hiện tại không tính.</summary>
public static class StorageFeeRules
{
    /// <summary>Số m³ hàng của đơn ĐANG nằm trong kho (tồn kho × thể tích 1 đơn vị của từng dòng hàng).</summary>
    public static async Task<decimal> CurrentVolumeM3Async(IApplicationDbContext db, int orderId, int warehouseId)
    {
        var rows = await db.WarehouseStocks
            .Where(s => s.WarehouseId == warehouseId && s.CargoLine.TransportOrderId == orderId && s.QtyOnHand > 0)
            .Select(s => new { s.QtyOnHand, s.CargoLine.QtyTotal, s.CargoLine.VolumeM3 })
            .ToListAsync();
        return Math.Round(rows.Sum(r => r.QtyTotal > 0 ? r.QtyOnHand * r.VolumeM3 / r.QtyTotal : 0), 4);
    }

    public static string RequesterText(DeadlineChangeRequester r) => r == DeadlineChangeRequester.Customer ? "Khách hàng" : "Công ty";

    public static async Task<StorageFeeDto?> ComputeAsync(IApplicationDbContext db, int orderId)
    {
        var order = await db.TransportOrders.Where(o => o.Id == orderId)
            .Select(o => new { o.Id, o.CommittedDeliveryDate }).FirstOrDefaultAsync();
        if (order is null) return null;

        var changes = await db.DeliveryDeadlineChanges.Where(c => c.TransportOrderId == orderId)
            .OrderBy(c => c.CreatedAt).ThenBy(c => c.Id).ToListAsync();
        var unitVolume = (await db.CargoLines.Where(c => c.TransportOrderId == orderId)
                .Select(c => new { c.Id, c.QtyTotal, c.VolumeM3 }).ToListAsync())
            .ToDictionary(c => c.Id, c => c.QtyTotal > 0 ? c.VolumeM3 / c.QtyTotal : 0);
        var moves = (await db.GoodsReceipts.Where(r => r.TransportOrderId == orderId)
                .Select(r => new { r.CargoLineId, Qty = r.QtyReceived, At = r.ReceivedAt }).ToListAsync())
            .Select(r => (Day: r.At.AddHours(7).Date, M3: r.Qty * unitVolume.GetValueOrDefault(r.CargoLineId)))
            .Concat((await db.GoodsIssues.Where(i => i.TransportOrderId == orderId && i.ReversedAt == null)
                    .Select(i => new { i.CargoLineId, Qty = i.QtyIssued, At = i.IssuedAt }).ToListAsync())
                .Select(i => (Day: i.At.AddHours(7).Date, M3: -i.Qty * unitVolume.GetValueOrDefault(i.CargoLineId))))
            .ToList();

        // Số m³ của đơn còn trong kho vào CUỐI ngày d.
        decimal VolumeAt(DateTime d) => Math.Max(0, moves.Where(m => m.Day <= d).Sum(m => m.M3));

        var today = VnClock.Today;
        var stillInStock = VolumeAt(today) > 0.00001m;
        var chargedDays = new HashSet<DateTime>();
        var lines = new List<StorageFeeLineDto>();
        var openWindow = false;
        foreach (var c in changes)
        {
            int days = 0; decimal m3Days = 0, fee = 0;
            if (c.RequestedBy == DeadlineChangeRequester.Customer && c.OldDate is DateTime oldDate && c.NewDate.Date > oldDate.Date)
            {
                var end = c.NewDate.Date;
                if (order.CommittedDeliveryDate is DateTime current && current.Date < end) end = current.Date;
                if (today < end) { end = today; openWindow = true; }
                for (var d = oldDate.Date.AddDays(1); d <= end; d = d.AddDays(1))
                {
                    if (chargedDays.Contains(d)) continue;
                    var v = VolumeAt(d);
                    if (v <= 0) continue;
                    chargedDays.Add(d);
                    days++;
                    m3Days += v;
                }
                fee = Math.Round(m3Days * c.StorageFeePerM3PerDay, 0);
            }
            lines.Add(new StorageFeeLineDto(c.CreatedAt, c.OldDate, c.NewDate, RequesterText(c.RequestedBy), c.Reason, c.ChangedByUserName,
                c.DelayDays, c.StorageFeePerM3PerDay, c.EstimatedFee, days, Math.Round(m3Days, 4), fee));
        }

        var total = lines.Sum(l => l.Fee);
        var isFinal = !(stillInStock && openWindow);
        var note = lines.All(l => l.RequestedBy != RequesterText(DeadlineChangeRequester.Customer))
            ? "Không có lần dời hạn nào do khách yêu cầu — không tính phí lưu kho."
            : isFinal ? "Đã chốt: tính theo số ngày hàng thực sự nằm kho trong các ngày khách xin dời."
            : "Tạm tính tới hôm nay — hàng vẫn còn trong kho, số chốt khi hàng xuất kho.";
        return new StorageFeeDto(orderId, total, isFinal, note, lines);
    }
}
