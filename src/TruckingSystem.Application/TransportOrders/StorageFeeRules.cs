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
            .Select(o => new { o.Id, o.CommittedDeliveryDate, o.Status }).FirstOrDefaultAsync();
        if (order is null) return null;

        var changes = await db.DeliveryDeadlineChanges.Where(c => c.TransportOrderId == orderId)
            .OrderBy(c => c.CreatedAt).ThenBy(c => c.Id).ToListAsync();
        var cargoLines = (await db.CargoLines.Where(c => c.TransportOrderId == orderId)
                .Select(c => new { c.Id, c.QtyTotal, c.VolumeM3 }).ToListAsync());
        var orderVolumeM3 = cargoLines.Sum(c => c.VolumeM3);
        var unitVolumeById = cargoLines
            .ToDictionary(c => c.Id, c => c.QtyTotal > 0 ? c.VolumeM3 / c.QtyTotal : 0);
        var moves = (await db.GoodsReceipts.Where(r => r.TransportOrderId == orderId)
                .Select(r => new { r.CargoLineId, Qty = r.QtyReceived, At = r.ReceivedAt }).ToListAsync())
            .Select(r => (Day: r.At.AddHours(7).Date, M3: r.Qty * unitVolumeById.GetValueOrDefault(r.CargoLineId)))
            .Concat((await db.GoodsIssues.Where(i => i.TransportOrderId == orderId && i.ReversedAt == null)
                    .Select(i => new { i.CargoLineId, Qty = i.QtyIssued, At = i.IssuedAt }).ToListAsync())
                .Select(i => (Day: i.At.AddHours(7).Date, M3: -i.Qty * unitVolumeById.GetValueOrDefault(i.CargoLineId))))
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
            var dayRows = new List<StorageFeeDayDto>();
            if (c.RequestedBy == DeadlineChangeRequester.Customer && c.OldDate is DateTime oldDate && c.NewDate.Date > oldDate.Date)
            {
                var end = c.NewDate.Date;
                // Đổi hạn: hạn về sau bị rút ngắn lại thì các ngày sau hạn hiện tại không tính.
                // Ghi nhận bổ sung (IsFeeOnly) không đổi hạn giao nên KHÔNG cắt theo hạn hiện tại.
                if (!c.IsFeeOnly && order.CommittedDeliveryDate is DateTime current && current.Date < end) end = current.Date;
                for (var d = oldDate.Date.AddDays(1); d <= c.NewDate.Date; d = d.AddDays(1))
                {
                    if (d > end)
                    {
                        if (d > today) { openWindow = true; dayRows.Add(new StorageFeeDayDto(d, 0, 0, "chưa tới ngày — sẽ tính nếu hàng còn nằm kho")); }
                        else dayRows.Add(new StorageFeeDayDto(d, 0, 0, "sau hạn giao hiện tại (hạn đã rút ngắn lại)"));
                        continue;
                    }
                    if (d > today) { openWindow = true; dayRows.Add(new StorageFeeDayDto(d, 0, 0, "chưa tới ngày — sẽ tính nếu hàng còn nằm kho")); continue; }
                    if (chargedDays.Contains(d)) { dayRows.Add(new StorageFeeDayDto(d, 0, 0, "đã tính ở lần trước (không tính trùng)")); continue; }
                    var v = Math.Round(VolumeAt(d), 4);
                    if (v <= 0) { dayRows.Add(new StorageFeeDayDto(d, 0, 0, "hàng không nằm trong kho")); continue; }
                    chargedDays.Add(d);
                    var dayFee = Math.Round(v * c.StorageFeePerM3PerDay, 0);
                    dayRows.Add(new StorageFeeDayDto(d, v, dayFee, null));
                    days++;
                    m3Days += v;
                }
                // Tổng tính 1 lần trên tổng m³·ngày (phí từng ngày ở bảng chi tiết chỉ để đối chiếu).
                fee = Math.Round(m3Days * c.StorageFeePerM3PerDay, 0);
            }
            lines.Add(new StorageFeeLineDto(c.CreatedAt, c.OldDate, c.NewDate, RequesterText(c.RequestedBy), c.Reason, c.ChangedByUserName,
                c.DelayDays, c.StorageFeePerM3PerDay, c.EstimatedFee, days, Math.Round(m3Days, 4), fee,
                c.IsFeeOnly, Formula(dayRows, c.StorageFeePerM3PerDay, fee), dayRows));
        }

        var total = lines.Sum(l => l.Fee);
        // CHỐT khi: không còn ngày nào trong khoảng khách xin dời ở phía trước; hoặc hàng của đơn đã nhập
        // kho ĐỦ rồi xuất hết; hoặc đơn đã Hoàn tất / Hủy và không còn hàng sót trong kho.
        // Trước đây: hàng CHƯA về kho mà khoảng dời còn ở tương lai vẫn hiện "Đã chốt 0 đ" — sai, vì
        // hàng về kho trong các ngày đó thì vẫn phải tính.
        var receivedM3 = moves.Where(m => m.M3 > 0).Sum(m => m.M3);
        var allGoodsLeft = !stillInStock && orderVolumeM3 > 0 && receivedM3 >= orderVolumeM3 - 0.00001m;
        var orderClosed = order.Status is TransportOrderStatus.Done or TransportOrderStatus.Cancelled;
        var isFinal = !openWindow || allGoodsLeft || (orderClosed && !stillInStock);
        var note = lines.All(l => l.RequestedBy != RequesterText(DeadlineChangeRequester.Customer))
            ? "Không có lần dời hạn nào do khách yêu cầu — không tính phí lưu kho."
            : isFinal ? "Đã chốt: tính theo số ngày hàng thực sự nằm kho trong các ngày khách xin dời."
            : stillInStock ? "Tạm tính tới hôm nay — hàng vẫn còn trong kho, số chốt khi hàng xuất kho hoặc hết các ngày khách xin dời."
            : "Tạm tính — hiện hàng của đơn không nằm trong kho nên chưa phát sinh ngày nào; nếu hàng nằm kho trong các ngày khách xin dời thì những ngày đó sẽ được tính.";
        return new StorageFeeDto(orderId, total, isFinal, note, lines);
    }

    /// <summary>Diễn giải ngắn của 1 lần: gom các ngày liên tiếp cùng số m³, VD
    /// "28/10–29/10: 2 ngày × 5 m³ = 10 m³·ngày → 10 m³·ngày × 500.000 đ = 5.000.000 đ".</summary>
    private static string Formula(List<StorageFeeDayDto> days, decimal rate, decimal fee)
    {
        var charged = days.Where(d => d.NotChargedReason is null).ToList();
        if (charged.Count == 0) return days.Count == 0 ? "" : "Không có ngày nào hàng nằm kho trong khoảng này → 0 đ";
        var groups = new List<(DateTime From, DateTime To, int Count, decimal M3)>();
        foreach (var d in charged)
        {
            if (groups.Count > 0 && groups[^1].To.AddDays(1) == d.Day && groups[^1].M3 == d.VolumeM3)
                groups[^1] = (groups[^1].From, d.Day, groups[^1].Count + 1, d.VolumeM3);
            else groups.Add((d.Day, d.Day, 1, d.VolumeM3));
        }
        var parts = groups.Select(g =>
            (g.Count == 1 ? $"{g.From:dd/MM}" : $"{g.From:dd/MM}–{g.To:dd/MM}") +
            $": {g.Count} ngày × {g.M3:0.##} m³ = {g.Count * g.M3:0.##} m³·ngày");
        var m3Days = charged.Sum(d => d.VolumeM3);
        return string.Join("; ", parts) + $" → {m3Days:0.##} m³·ngày × {rate:N0} đ = {fee:N0} đ";
    }
}
