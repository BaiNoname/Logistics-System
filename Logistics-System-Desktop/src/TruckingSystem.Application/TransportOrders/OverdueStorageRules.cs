using Microsoft.EntityFrameworkCore;
using TruckingSystem.Application.Common;
using TruckingSystem.Application.Common.Interfaces;
using TruckingSystem.Domain.Entities;
using TruckingSystem.Domain.Enums;

namespace TruckingSystem.Application.TransportOrders;

/// <summary>Hàng của đơn NẰM KHO QUÁ HẠN GIAO — tự phát hiện, MẶC ĐỊNH CÔNG TY CHỊU (khách 0 đ), chờ người duyệt chọn
/// "Do khách hàng" (tính phí lưu kho cho khách) hoặc "Do công ty" (xác nhận 0 đ).
///  • Tính theo LỊCH SỬ nhập / xuất (không chỉ hàng đang còn trong kho): những ngày SAU hạn giao mà hàng của đơn từng
///    nằm kho (đầu ngày hoặc cuối ngày — cùng cách StorageFeeRules) -> lúc duyệt chuyến giao cuối (hàng đã lên xe) vẫn biết.
///  • Chỉ xét đơn còn đang xử lý (Đã xác nhận / Đang chạy) hoặc đơn đã đóng mà còn hàng trong kho — đơn cũ đã xong thì thôi.
///  • Lựa chọn lưu bằng 1 dòng "ghi nhận phí" (DeliveryDeadlineChange.IsFeeOnly) có OldDate = hạn giao hiện tại —
///    dùng lại bảng sẵn có, không cần migration. Hạn giao bị dời về sau -> hết quá hạn, không còn nhắc.</summary>
public static class OverdueStorageRules
{
    /// <summary>Số ngày CÒN CỬA SỔ tính phí sau khi chọn (tới khi hàng rời kho thì phí tự chốt).</summary>
    public const int WindowDays = 90;

    /// <param name="M3Days">Tổng m³·ngày hàng nằm kho sau hạn giao (phí ước tính = M3Days × đơn giá kho).</param>
    public record Item(int OrderId, DateTime Due, int Days, decimal M3Days, DeadlineChangeRequester? Decision);

    public static async Task<Dictionary<int, Item>> LoadAsync(IApplicationDbContext db, ICollection<int>? orderIds = null)
    {
        var today = VnClock.Today;
        var q = db.TransportOrders.Where(o => o.Status != TransportOrderStatus.Draft && o.CommittedDeliveryDate != null
            && o.CommittedDeliveryDate < today
            && db.GoodsReceipts.Any(r => r.TransportOrderId == o.Id)
            && (o.Status == TransportOrderStatus.Confirmed || o.Status == TransportOrderStatus.InProgress
                || db.WarehouseStocks.Any(s => s.CargoLine.TransportOrderId == o.Id && s.QtyOnHand > 0)));
        if (orderIds is not null) q = q.Where(o => orderIds.Contains(o.Id));
        var orders = await q.Select(o => new { o.Id, Due = o.CommittedDeliveryDate!.Value }).ToListAsync();
        if (orders.Count == 0) return new();
        var ids = orders.Select(o => o.Id).ToList();

        var unit = await db.CargoLines.Where(c => ids.Contains(c.TransportOrderId))
            .Select(c => new { c.Id, c.QtyTotal, c.VolumeM3 })
            .ToDictionaryAsync(c => c.Id, c => c.QtyTotal > 0 ? c.VolumeM3 / c.QtyTotal : 0);
        var moves = (await db.GoodsReceipts.Where(r => ids.Contains(r.TransportOrderId))
                .Select(r => new { r.TransportOrderId, r.CargoLineId, Qty = r.QtyReceived, At = r.ReceivedAt }).ToListAsync())
            .Select(r => (r.TransportOrderId, Day: r.At.AddHours(7).Date, M3: r.Qty * unit.GetValueOrDefault(r.CargoLineId)))
            .Concat((await db.GoodsIssues.Where(i => ids.Contains(i.TransportOrderId) && i.ReversedAt == null)
                    .Select(i => new { i.TransportOrderId, i.CargoLineId, Qty = i.QtyIssued, At = i.IssuedAt }).ToListAsync())
                .Select(i => (i.TransportOrderId, Day: i.At.AddHours(7).Date, M3: -i.Qty * unit.GetValueOrDefault(i.CargoLineId))))
            .ToLookup(m => m.TransportOrderId);
        var marks = await db.DeliveryDeadlineChanges.Where(c => ids.Contains(c.TransportOrderId) && c.IsFeeOnly && c.OldDate != null)
            .Select(c => new { c.Id, c.TransportOrderId, OldDate = c.OldDate!.Value, c.RequestedBy })
            .ToListAsync();

        var result = new Dictionary<int, Item>();
        foreach (var o in orders)
        {
            var m = moves[o.Id].ToList();
            decimal VolumeAt(DateTime d) => Math.Max(0, m.Where(x => x.Day <= d).Sum(x => x.M3));
            int days = 0; decimal m3Days = 0;
            for (var d = o.Due.Date.AddDays(1); d <= today; d = d.AddDays(1))
            {
                var v = Math.Max(VolumeAt(d.AddDays(-1)), VolumeAt(d));
                if (v <= 0.00001m) continue;
                days++;
                m3Days += v;
            }
            if (days == 0) continue;
            var mark = marks.Where(x => x.TransportOrderId == o.Id && x.OldDate.Date == o.Due.Date).OrderByDescending(x => x.Id).FirstOrDefault();
            result[o.Id] = new Item(o.Id, o.Due.Date, days, Math.Round(m3Days, 4), mark?.RequestedBy);
        }
        return result;
    }

    /// <summary>Số đơn đang có hàng nằm kho quá hạn mà CHƯA ai chọn do bên nào (cho số đỏ trên menu).</summary>
    public static async Task<int> CountUndecidedAsync(IApplicationDbContext db)
        => (await LoadAsync(db)).Values.Count(i => i.Decision is null);

    public static DeadlineChangeRequester? ParseParty(string? value)
        => Enum.TryParse<DeadlineChangeRequester>(value, true, out var r) && Enum.IsDefined(r) ? r : null;

    /// <summary>Ghi lựa chọn bên chịu phí (CHƯA SaveChanges — người gọi lưu, để gói chung transaction với việc duyệt chuyến).
    /// Trả câu lỗi (null = hợp lệ). Chọn lại thì cập nhật dòng cũ; luôn ghi nhật ký.</summary>
    public static async Task<string?> ApplyDecisionAsync(IApplicationDbContext db, int orderId, string? partyText, string? reasonText,
        string? userId, string? userName)
    {
        var order = await db.TransportOrders.FirstOrDefaultAsync(o => o.Id == orderId);
        if (order is null) return "Không tìm thấy đơn.";
        if (order.CommittedDeliveryDate is not DateTime due) return "Đơn chưa có hạn giao — không có phần nằm kho quá hạn.";
        var party = ParseParty(partyText);
        if (party is null) return "Chọn bên chịu phí lưu kho: Khách hàng hoặc Công ty.";
        var reason = (reasonText ?? "").Trim();
        if (party == DeadlineChangeRequester.Customer && reason.Length < 5)
            return "Do khách hàng: phải ghi lý do (ít nhất 5 ký tự) — VD \"Khách hẹn lại ngày nhận hàng\".";
        if (reason.Length == 0) reason = "Xác nhận do công ty — không tính phí lưu kho cho khách.";
        if (reason.Length > 500) reason = reason[..500];

        var items = await LoadAsync(db, new[] { orderId });
        if (!items.ContainsKey(orderId)) return "Đơn không có hàng nằm kho quá hạn giao.";
        var rate = await db.Warehouses.Where(w => w.Id == order.WarehouseId).Select(w => w.StorageFeePerM3PerDay).FirstOrDefaultAsync();
        if (party == DeadlineChangeRequester.Customer && rate <= 0)
            return "Kho của đơn chưa khai báo phí lưu kho (Sửa kho → Phí lưu kho) — chưa tính được cho khách.";
        var volume = await StorageFeeRules.CurrentVolumeM3Async(db, order.Id, order.WarehouseId);
        var oldDate = due.Date;
        var mark = (await db.DeliveryDeadlineChanges.Where(c => c.TransportOrderId == orderId && c.IsFeeOnly && c.OldDate != null).ToListAsync())
            .Where(c => c.OldDate!.Value.Date == oldDate).OrderByDescending(c => c.Id).FirstOrDefault();
        var endDate = VnClock.Today.AddDays(WindowDays);
        if (mark is null)
        {
            mark = new DeliveryDeadlineChange { TransportOrderId = orderId, OldDate = oldDate, IsFeeOnly = true };
            db.DeliveryDeadlineChanges.Add(mark);
        }
        var before = mark.Id == 0 ? null : StorageFeeRules.RequesterText(mark.RequestedBy);
        mark.NewDate = endDate;
        mark.RequestedBy = party.Value;
        mark.Reason = reason;
        mark.ChangedByUserName = userName;
        mark.DelayDays = (endDate - oldDate).Days;
        mark.StorageFeePerM3PerDay = rate;
        mark.VolumeM3AtChange = volume;
        mark.EstimatedFee = 0;
        db.AuditLogs.Add(new AuditLog
        {
            UserId = userId,
            Action = "SetStorageResponsibility",
            EntityName = nameof(TransportOrder),
            EntityId = orderId,
            Detail = $"Hàng đơn {order.Code} nằm kho quá hạn giao {oldDate:dd/MM/yyyy}: {(before is null ? "" : $"đổi từ \"{before}\" ")}chọn do " +
                     $"{StorageFeeRules.RequesterText(party.Value)}. Lý do: {reason}. Người chọn: {userName ?? "(không rõ)"}.",
        });
        return null;
    }
}
