using Microsoft.EntityFrameworkCore;
using TruckingSystem.Application.Common.Interfaces;
using TruckingSystem.Domain.Entities;
using TruckingSystem.Domain.Enums;

namespace TruckingSystem.Application.TransportOrders;

/// <summary>HẠN TÍNH TRỄ — mốc để xét đơn giao ĐÚNG HẠN hay TRỄ (khác với "hạn giao" hiện tại để báo khách):
///  • Bắt đầu = hạn giao ban đầu (hạn cũ của lần đổi đầu tiên; chưa đổi lần nào thì = hạn giao hiện tại).
///  • KHÁCH yêu cầu dời hạn -> hạn tính trễ = hạn mới (giao trước / đúng hạn mới là đúng hạn).
///  • CÔNG TY dời hạn -> hạn tính trễ GIỮ NGUYÊN (giao sau mốc cũ vẫn là trễ); công ty dời SỚM hơn thì lấy hạn sớm hơn.
/// Dùng cho: nhãn đúng hạn / trễ (màn Chuyến, báo cáo), ngày hàng nằm kho "sau hạn" phải chọn bên chịu phí,
/// và phần bồi thường giao trễ (phần của Minh) nên đếm số ngày trễ theo mốc này.
/// Không cần cột mới: tính từ lịch sử đổi hạn (DeliveryDeadlineChange, bỏ qua dòng IsFeeOnly).</summary>
public static class DeadlineBasisRules
{
    /// <summary>1 lần đổi hạn kèm hạn tính trễ SAU lần đổi đó (để hiện ở bảng lịch sử).</summary>
    public record Step(DeliveryDeadlineChange Change, DateTime? BasisAfter);

    public static List<Step> Walk(IEnumerable<DeliveryDeadlineChange> changes)
    {
        var steps = new List<Step>();
        DateTime? basis = null;
        var first = true;
        foreach (var c in changes.Where(c => !c.IsFeeOnly).OrderBy(c => c.CreatedAt).ThenBy(c => c.Id))
        {
            if (first) { basis = c.OldDate?.Date; first = false; }
            var newDate = c.NewDate.Date;
            if (basis is null || c.RequestedBy == DeadlineChangeRequester.Customer) basis = newDate;
            else if (newDate < basis.Value) basis = newDate;
            steps.Add(new Step(c, basis));
        }
        return steps;
    }

    /// <summary>Hạn tính trễ từ hạn giao hiện tại + lịch sử đổi hạn (null = đơn không có hạn giao).</summary>
    public static DateTime? Compute(DateTime? committed, IEnumerable<DeliveryDeadlineChange> changes)
    {
        var steps = Walk(changes);
        return steps.Count == 0 ? committed?.Date : steps[^1].BasisAfter ?? committed?.Date;
    }

    /// <summary>Hạn tính trễ của nhiều đơn 1 lần (chỉ những đơn ĐÃ từng đổi hạn) — dùng <see cref="Pick"/> để lấy.</summary>
    public static async Task<Dictionary<int, DateTime?>> LoadAsync(IApplicationDbContext db, ICollection<int>? orderIds = null)
    {
        var q = db.DeliveryDeadlineChanges.Where(c => !c.IsFeeOnly);
        if (orderIds is not null) q = q.Where(c => orderIds.Contains(c.TransportOrderId));
        var rows = await q.ToListAsync();
        return rows.GroupBy(c => c.TransportOrderId).ToDictionary(g => g.Key, g => Compute(null, g));
    }

    public static DateTime? Pick(Dictionary<int, DateTime?> map, int orderId, DateTime? committed)
        => map.TryGetValue(orderId, out var b) && b is not null ? b : committed?.Date;

    public static async Task<DateTime?> ForOrderAsync(IApplicationDbContext db, int orderId, DateTime? committed)
        => Pick(await LoadAsync(db, new[] { orderId }), orderId, committed);
}
