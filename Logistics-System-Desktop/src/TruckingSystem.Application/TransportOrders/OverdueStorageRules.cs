using Microsoft.EntityFrameworkCore;
using TruckingSystem.Application.Common;
using TruckingSystem.Application.Common.Interfaces;
using TruckingSystem.Domain.Entities;
using TruckingSystem.Domain.Enums;

namespace TruckingSystem.Application.TransportOrders;

/// <summary>Hàng của đơn NẰM KHO SAU HẠN TÍNH TRỄ — tự phát hiện, CHƯA TÍNH cho khách, chờ người duyệt chọn
/// "Do khách hàng" (tính phí lưu kho các ngày đó cho khách) hoặc "Do công ty" (khách 0 đ).
///  • Cùng cách đếm ngày với StorageFeeRules (loại ngày "AfterBasis"): ngày sau hạn tính trễ, đã hết số ngày miễn phí,
///    hàng của đơn còn nằm kho (đầu ngày hoặc cuối ngày) -> lúc duyệt chuyến giao cuối (hàng đã lên xe) vẫn biết.
///  • Hạn tính trễ (DeadlineBasisRules): khách dời hạn thì mốc dời theo; công ty dời hạn thì mốc giữ nguyên.
///  • Chỉ xét đơn còn đang xử lý (Đã xác nhận / Đang chạy) hoặc đơn đã đóng mà còn hàng trong kho. Kho không tính phí thì bỏ qua.
///  • Lựa chọn lưu bằng 1 dòng DeliveryDeadlineChange.IsFeeOnly có OldDate = hạn tính trễ. Khách dời hạn về sau -> mốc mới,
///    hết phần "sau hạn", không còn nhắc.</summary>
public static class OverdueStorageRules
{
    /// <summary>Số ngày CÒN CỬA SỔ tính phí sau khi chọn (tới khi hàng rời kho thì phí tự chốt).</summary>
    public const int WindowDays = 90;

    /// <param name="Due">Hạn tính trễ.</param>
    /// <param name="M3Days">Tổng m³·ngày hàng nằm kho sau hạn tính trễ.</param>
    /// <param name="Fee">Phí của những ngày đó nếu tính cho khách (tổng phí từng ngày).</param>
    public record Item(int OrderId, DateTime Due, int Days, decimal M3Days, DeadlineChangeRequester? Decision, decimal Rate = 0, decimal Fee = 0);

    /// <param name="anyStatus">true = xét cả đơn đã đóng / hết hàng (khi người dùng chủ động chọn bên chịu ở màn Phí lưu kho).</param>
    public static async Task<Dictionary<int, Item>> LoadAsync(IApplicationDbContext db, ICollection<int>? orderIds = null,
        Dictionary<int, DateTime?>? basisByOrder = null, bool anyStatus = false)
    {
        var today = VnClock.Today;
        var q = db.TransportOrders.Where(o => o.Status != TransportOrderStatus.Draft && o.CommittedDeliveryDate != null
            && o.Warehouse.StorageFeePerM3PerDay > 0
            && db.GoodsReceipts.Any(r => r.TransportOrderId == o.Id)
            && (anyStatus || o.Status == TransportOrderStatus.Confirmed || o.Status == TransportOrderStatus.InProgress
                || db.WarehouseStocks.Any(s => s.CargoLine.TransportOrderId == o.Id && s.QtyOnHand > 0)));
        if (orderIds is not null) q = q.Where(o => orderIds.Contains(o.Id));
        var orders = await q.Select(o => new
            {
                o.Id, o.CommittedDeliveryDate,
                Rate = o.Warehouse.StorageFeePerM3PerDay, FreeDays = o.Warehouse.FreeStorageDays,
            })
            .ToListAsync();
        if (orders.Count == 0) return new();
        var ids = orders.Select(o => o.Id).ToList();
        basisByOrder ??= await DeadlineBasisRules.LoadAsync(db, ids);
        var due = orders.ToDictionary(o => o.Id, o => DeadlineBasisRules.Pick(basisByOrder, o.Id, o.CommittedDeliveryDate));
        // Chưa qua hạn tính trễ -> chưa có ngày nào "sau hạn", khỏi tải nhập / xuất.
        orders = orders.Where(o => due[o.Id] is DateTime d && d < today).ToList();
        if (orders.Count == 0) return new();
        ids = orders.Select(o => o.Id).ToList();
        var moves = await StorageFeeRules.LoadMovesAsync(db, ids);
        var decisions = await StorageFeeRules.LoadDecisionsAsync(db, ids);

        var result = new Dictionary<int, Item>();
        foreach (var o in orders)
        {
            var basis = due[o.Id]!.Value.Date;
            var after = StorageFeeRules.Build(moves[o.Id], o.FreeDays, basis, today).Days
                .Where(d => d.Kind == StorageFeeRules.DayKind.AfterBasis).ToList();
            if (after.Count == 0) continue;
            DeadlineChangeRequester? decision = decisions.TryGetValue((o.Id, basis), out var dec) ? dec.By : null;
            result[o.Id] = new Item(o.Id, basis, after.Count, Math.Round(after.Sum(d => d.VolumeM3), 4), decision,
                o.Rate, after.Sum(d => Math.Round(d.VolumeM3 * o.Rate, 0)));
        }
        return result;
    }

    /// <summary>Số đơn đang có hàng nằm kho sau hạn tính trễ mà CHƯA ai chọn do bên nào (cho số đỏ trên menu).</summary>
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
        if (await DeadlineBasisRules.ForOrderAsync(db, orderId, order.CommittedDeliveryDate) is not DateTime due)
            return "Đơn chưa có hạn giao — không có phần nằm kho sau hạn tính trễ.";
        var party = ParseParty(partyText);
        if (party is null) return "Chọn bên chịu phí lưu kho: Khách hàng hoặc Công ty.";
        var reason = (reasonText ?? "").Trim();
        if (party == DeadlineChangeRequester.Customer && reason.Length < 5)
            return "Do khách hàng: phải ghi lý do (ít nhất 5 ký tự) — VD \"Khách hẹn lại ngày nhận hàng\".";
        if (reason.Length == 0) reason = "Xác nhận do công ty — không tính phí lưu kho cho khách.";
        if (reason.Length > 500) reason = reason[..500];

        var items = await LoadAsync(db, new[] { orderId }, anyStatus: true);
        if (!items.ContainsKey(orderId)) return "Đơn không có hàng nằm kho sau hạn tính trễ (đã trừ ngày miễn phí).";
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
            Detail = $"Hàng đơn {order.Code} nằm kho sau hạn tính trễ {oldDate:dd/MM/yyyy}: {(before is null ? "" : $"đổi từ \"{before}\" ")}chọn do " +
                     $"{StorageFeeRules.RequesterText(party.Value)}. Lý do: {reason}. Người chọn: {userName ?? "(không rõ)"}.",
        });
        return null;
    }
}
