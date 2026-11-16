using Microsoft.EntityFrameworkCore;
using TruckingSystem.Application.Common;
using TruckingSystem.Application.Common.Interfaces;
using TruckingSystem.Domain.Enums;

namespace TruckingSystem.Application.TransportOrders;

/// <summary>PHÍ LƯU KHO của đơn — quy tắc DUY NHẤT (màn Phí lưu kho, thẻ đơn, hóa đơn, nhắc nằm kho quá hạn):
///  • Miễn phí <c>Warehouse.FreeStorageDays</c> ngày kể từ ngày NHẬP KHO ĐẦU TIÊN của đơn (tính cả ngày nhập).
///  • Từ ngày kế tiếp, mỗi ngày hàng của đơn còn nằm kho: phí = số m³ nằm kho trong ngày × đơn giá kho (đ/m³/ngày).
///    m³ trong ngày = số lớn hơn giữa đầu ngày và cuối ngày (ngày XUẤT KHO vẫn tính vì hàng nằm kho qua đêm tới hôm đó).
///  • Ngày SAU "hạn tính trễ" (<see cref="DeadlineBasisRules"/>) phải chọn bên chịu: do khách -> tính cho khách,
///    do công ty -> 0 đ cho khách, chưa chọn -> chưa tính và phí CHƯA CHỐT (hóa đơn chờ).
///  • Đổi hạn giao KHÔNG tự sinh phí riêng nữa (bỏ ước tính lúc đổi hạn và phần ghi phí bổ sung).
///  • Đơn giá = đơn giá hiện tại của kho (đổi giá kho thì đơn chưa lập hóa đơn tính theo giá mới).</summary>
public static class StorageFeeRules
{
    public enum DayKind { Free, Charged, AfterBasis }

    public record DayRow(DateTime Day, decimal VolumeM3, DayKind Kind, string? Event);

    /// <param name="Received">Ngày nhập kho đầu tiên (null = hàng chưa về kho).</param>
    /// <param name="LastIssue">Ngày xuất hết (null = còn hàng trong kho / chưa xuất).</param>
    public record Timeline(DateTime? Received, DateTime? FreeUntil, DateTime? LastIssue, bool StillInStock, decimal VolumeNowM3,
        decimal ReceivedM3, List<DayRow> Days);

    private const decimal Eps = 0.00001m;

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

    /// <summary>Nhập / xuất kho (m³, ngày giờ VN) của nhiều đơn — phiếu xuất đã hoàn tác thì bỏ.</summary>
    public static async Task<ILookup<int, (DateTime Day, decimal M3)>> LoadMovesAsync(IApplicationDbContext db, ICollection<int> orderIds)
    {
        var unit = await db.CargoLines.Where(c => orderIds.Contains(c.TransportOrderId))
            .Select(c => new { c.Id, c.QtyTotal, c.VolumeM3 })
            .ToDictionaryAsync(c => c.Id, c => c.QtyTotal > 0 ? c.VolumeM3 / c.QtyTotal : 0);
        var receipts = await db.GoodsReceipts.Where(r => orderIds.Contains(r.TransportOrderId))
            .Select(r => new { r.TransportOrderId, r.CargoLineId, Qty = r.QtyReceived, At = r.ReceivedAt }).ToListAsync();
        var issues = await db.GoodsIssues.Where(i => orderIds.Contains(i.TransportOrderId) && i.ReversedAt == null)
            .Select(i => new { i.TransportOrderId, i.CargoLineId, Qty = i.QtyIssued, At = i.IssuedAt }).ToListAsync();
        return receipts.Select(r => (r.TransportOrderId, Day: r.At.AddHours(7).Date, M3: r.Qty * unit.GetValueOrDefault(r.CargoLineId)))
            .Concat(issues.Select(i => (i.TransportOrderId, Day: i.At.AddHours(7).Date, M3: -i.Qty * unit.GetValueOrDefault(i.CargoLineId))))
            .ToLookup(m => m.TransportOrderId, m => (m.Day, m.M3));
    }

    /// <summary>Dựng các ngày hàng nằm kho (từ ngày nhập đầu tiên tới hôm nay / ngày xuất hết) và loại từng ngày.</summary>
    public static Timeline Build(IEnumerable<(DateTime Day, decimal M3)> movesIn, int freeDays, DateTime? lateBasis, DateTime today)
    {
        var moves = movesIn.ToList();
        var receivedM3 = moves.Where(m => m.M3 > 0).Sum(m => m.M3);
        if (receivedM3 <= Eps) return new Timeline(null, null, null, false, 0, 0, new List<DayRow>());
        decimal VolumeAt(DateTime d) => Math.Max(0, moves.Where(m => m.Day <= d).Sum(m => m.M3));

        var received = moves.Where(m => m.M3 > 0).Min(m => m.Day);
        var freeUntil = received.AddDays(Math.Max(0, freeDays) - 1);
        var volumeNow = Math.Round(VolumeAt(today), 4);
        var stillInStock = volumeNow > Eps;
        DateTime? lastIssue = !stillInStock && moves.Any(m => m.M3 < 0) ? moves.Where(m => m.M3 < 0).Max(m => m.Day) : null;
        var end = stillInStock ? today : lastIssue ?? today;

        var days = new List<DayRow>();
        for (var d = received; d <= end; d = d.AddDays(1))
        {
            var v = Math.Round(Math.Max(VolumeAt(d.AddDays(-1)), VolumeAt(d)), 4);
            if (v <= Eps) continue;
            var kind = d <= freeUntil ? DayKind.Free
                : lateBasis is DateTime b && d > b.Date ? DayKind.AfterBasis
                : DayKind.Charged;
            var inM3 = moves.Where(m => m.Day == d && m.M3 > 0).Sum(m => m.M3);
            var outM3 = -moves.Where(m => m.Day == d && m.M3 < 0).Sum(m => m.M3);
            var ev = new List<string>();
            if (inM3 > Eps) ev.Add($"nhập {inM3:0.##} m³");
            if (outM3 > Eps) ev.Add(VolumeAt(d) <= Eps ? "xuất hết" : $"xuất {outM3:0.##} m³");
            if (lateBasis is DateTime b2 && d == b2.Date) ev.Add("hạn tính trễ");
            days.Add(new DayRow(d, v, kind, ev.Count == 0 ? null : string.Join(", ", ev)));
        }
        return new Timeline(received, freeUntil, lastIssue, stillInStock, volumeNow, receivedM3, days);
    }

    /// <summary>Lựa chọn "do khách / do công ty" cho các ngày sau hạn tính trễ (dòng IsFeeOnly có OldDate = hạn tính trễ).</summary>
    public static async Task<Dictionary<(int OrderId, DateTime Basis), (DeadlineChangeRequester By, string Reason)>> LoadDecisionsAsync(
        IApplicationDbContext db, ICollection<int> orderIds)
        => (await db.DeliveryDeadlineChanges.Where(c => orderIds.Contains(c.TransportOrderId) && c.IsFeeOnly && c.OldDate != null)
                .OrderByDescending(c => c.Id)
                .Select(c => new { c.TransportOrderId, OldDate = c.OldDate!.Value, c.RequestedBy, c.Reason }).ToListAsync())
            .GroupBy(c => (c.TransportOrderId, c.OldDate.Date))
            .ToDictionary(g => (g.Key.TransportOrderId, g.Key.Date), g => (g.First().RequestedBy, g.First().Reason));

    public static async Task<StorageFeeDto?> ComputeAsync(IApplicationDbContext db, int orderId)
    {
        var order = await db.TransportOrders.Where(o => o.Id == orderId)
            .Select(o => new
            {
                o.Id, o.Status, o.CommittedDeliveryDate, WarehouseName = o.Warehouse.Name,
                Rate = o.Warehouse.StorageFeePerM3PerDay, FreeDays = o.Warehouse.FreeStorageDays,
                OrderM3 = o.CargoLines.Sum(c => (decimal?)c.VolumeM3) ?? 0,
            })
            .FirstOrDefaultAsync();
        if (order is null) return null;

        var changes = await db.DeliveryDeadlineChanges.Where(c => c.TransportOrderId == orderId).ToListAsync();
        var steps = DeadlineBasisRules.Walk(changes);
        var basis = steps.Count == 0 ? order.CommittedDeliveryDate?.Date : steps[^1].BasisAfter ?? order.CommittedDeliveryDate?.Date;
        var history = steps.Select(s => new DeadlineChangeDto(s.Change.CreatedAt, s.Change.OldDate, s.Change.NewDate,
            RequesterText(s.Change.RequestedBy), s.Change.Reason, s.Change.ChangedByUserName, s.BasisAfter)).ToList();

        var today = VnClock.Today;
        var tl = Build((await LoadMovesAsync(db, new[] { orderId }))[orderId], order.FreeDays, basis, today);

        // Lựa chọn bên chịu cho các ngày sau hạn tính trễ.
        var mark = basis is DateTime bd
            ? changes.Where(c => c.IsFeeOnly && c.OldDate?.Date == bd).OrderByDescending(c => c.Id).FirstOrDefault()
            : null;
        DeadlineChangeRequester? decision = mark?.RequestedBy;

        var rate = order.Rate;
        decimal Fee(DayRow d) => rate > 0 ? Math.Round(d.VolumeM3 * rate, 0) : 0;
        var dayDtos = tl.Days.Select(d =>
        {
            var fee = d.Kind == DayKind.Free ? 0 : Fee(d);
            return new StorageFeeDayDto(d.Day, d.VolumeM3, fee, d.Kind.ToString(), d.Event);
        }).ToList();
        var chargedFee = dayDtos.Where(d => d.Kind == nameof(DayKind.Charged)).Sum(d => d.Fee);
        var afterFee = dayDtos.Where(d => d.Kind == nameof(DayKind.AfterBasis)).Sum(d => d.Fee);
        var afterDays = dayDtos.Count(d => d.Kind == nameof(DayKind.AfterBasis));
        var total = chargedFee + (decision == DeadlineChangeRequester.Customer ? afterFee : 0);
        var companyFee = decision == DeadlineChangeRequester.Company ? afterFee : 0;
        var pendingFee = decision is null ? afterFee : 0;

        // CHỐT khi: hàng không còn nằm kho, (đã nhập đủ hàng của đơn hoặc đơn đã Hoàn tất / Hủy), và không còn phần chờ chọn
        // bên chịu. Đơn ĐÃ ĐÓNG mà vẫn chưa chọn (đơn cũ trước khi có quy tắc này) -> mặc định công ty chịu, vẫn chốt được.
        // Kho không tính phí (đơn giá 0) thì luôn chốt 0 đ.
        var orderClosed = order.Status is TransportOrderStatus.Done or TransportOrderStatus.Cancelled;
        var receivedAll = order.OrderM3 > 0 && tl.ReceivedM3 >= order.OrderM3 - Eps;
        var isFinal = rate <= 0
            || ((pendingFee <= 0 || orderClosed) && !tl.StillInStock && (tl.Received is null ? orderClosed : receivedAll || orderClosed));

        string note;
        if (rate <= 0) note = $"Kho {order.WarehouseName} không tính phí lưu kho (đơn giá 0 đ).";
        else if (tl.Received is null) note = orderClosed ? "Hàng của đơn không nằm kho — không có phí lưu kho." : "Hàng chưa nhập kho — chưa phát sinh phí lưu kho.";
        else if (pendingFee > 0 && isFinal) note = $"Hàng nằm kho {afterDays} ngày SAU hạn tính trễ {basis:dd/MM} chưa chọn bên chịu — đơn đã đóng nên mặc định công ty chịu (khách 0 đ); vẫn chọn lại được.";
        else if (pendingFee > 0) note = $"Hàng nằm kho {afterDays} ngày SAU hạn tính trễ {basis:dd/MM} — chọn do khách hay do công ty để chốt phí.";
        else if (isFinal) note = "Đã chốt: hàng đã rời kho, phí tính theo số ngày hàng thực sự nằm kho sau thời gian miễn phí.";
        else if (tl.StillInStock) note = tl.FreeUntil is DateTime fu && today <= fu
            ? $"Hàng còn miễn phí lưu kho tới hết {fu:dd/MM}."
            : "Tạm tính tới hôm nay — hàng vẫn còn trong kho, mỗi ngày nằm thêm sẽ được cộng.";
        else note = "Tạm tính — đơn còn hàng chưa nhập kho, hàng về kho thì tiếp tục tính.";

        return new StorageFeeDto(orderId, total, isFinal, note,
            order.WarehouseName, rate, order.FreeDays, tl.Received, tl.FreeUntil, basis, order.CommittedDeliveryDate?.Date,
            tl.LastIssue, tl.StillInStock, tl.VolumeNowM3, companyFee, pendingFee, afterDays, decision?.ToString(), mark?.Reason,
            dayDtos, history);
    }

    /// <summary>Tóm tắt lưu kho cho DANH SÁCH đơn (thẻ đơn, cột Việc cần làm): ngày lưu kho thứ mấy, còn mấy ngày miễn phí,
    /// phí tới hôm nay (phần đã chắc chắn tính cho khách). Chỉ đơn đang có hàng nằm kho.</summary>
    public record Summary(int StorageDay, int FreeDaysLeft, decimal FeeToDate, decimal VolumeNowM3);

    public static async Task<Dictionary<int, Summary>> SummariesAsync(IApplicationDbContext db, Dictionary<int, DateTime?> basisByOrder)
    {
        var today = VnClock.Today;
        var orders = await db.TransportOrders
            .Where(o => db.WarehouseStocks.Any(s => s.CargoLine.TransportOrderId == o.Id && s.QtyOnHand > 0))
            .Select(o => new { o.Id, o.CommittedDeliveryDate, Rate = o.Warehouse.StorageFeePerM3PerDay, FreeDays = o.Warehouse.FreeStorageDays })
            .ToListAsync();
        if (orders.Count == 0) return new();
        var ids = orders.Select(o => o.Id).ToList();
        var moves = await LoadMovesAsync(db, ids);
        var decisions = await LoadDecisionsAsync(db, ids);
        var result = new Dictionary<int, Summary>();
        foreach (var o in orders)
        {
            var basis = DeadlineBasisRules.Pick(basisByOrder, o.Id, o.CommittedDeliveryDate);
            var tl = Build(moves[o.Id], o.FreeDays, basis, today);
            if (tl.Received is not DateTime rec || !tl.StillInStock) continue;
            var customerAfter = basis is DateTime b && decisions.TryGetValue((o.Id, b.Date), out var dec) && dec.By == DeadlineChangeRequester.Customer;
            var fee = o.Rate <= 0 ? 0 : tl.Days
                .Where(d => d.Kind == DayKind.Charged || (d.Kind == DayKind.AfterBasis && customerAfter))
                .Sum(d => Math.Round(d.VolumeM3 * o.Rate, 0));
            var freeLeft = tl.FreeUntil is DateTime fu && fu >= today ? (fu - today).Days + 1 : 0;
            result[o.Id] = new Summary((today - rec).Days + 1, o.Rate <= 0 ? -1 : freeLeft, fee, tl.VolumeNowM3);
        }
        return result;
    }
}
