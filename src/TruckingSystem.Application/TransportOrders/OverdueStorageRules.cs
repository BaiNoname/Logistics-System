using Microsoft.EntityFrameworkCore;
using TruckingSystem.Application.Common;
using TruckingSystem.Application.Common.Interfaces;
using TruckingSystem.Domain.Enums;

namespace TruckingSystem.Application.TransportOrders;

/// <summary>Hàng của đơn NẰM KHO QUÁ HẠN GIAO — tự phát hiện, MẶC ĐỊNH CÔNG TY CHỊU (khách 0 đ), chờ người duyệt chọn
/// "Do khách hàng" (tính phí lưu kho cho khách, cộng dồn tới khi hàng rời kho) hoặc "Do công ty" (xác nhận 0 đ).
/// Lựa chọn lưu bằng 1 dòng "ghi nhận phí" (DeliveryDeadlineChange.IsFeeOnly) có OldDate = hạn giao hiện tại —
/// dùng lại bảng sẵn có, không cần migration. Hạn giao bị dời về sau -> hết quá hạn, không còn nhắc.</summary>
public static class OverdueStorageRules
{
    /// <summary>Số ngày CÒN CỬA SỔ tính phí sau khi chọn (tới khi hàng rời kho thì phí tự chốt).</summary>
    public const int WindowDays = 90;

    public record Item(int OrderId, int Days, DeadlineChangeRequester? Decision);

    /// <summary>Các đơn đang có hàng nằm kho sau hạn giao (đơn đã xác nhận trở đi). orderIds = null: mọi đơn.</summary>
    public static async Task<Dictionary<int, Item>> LoadAsync(IApplicationDbContext db, ICollection<int>? orderIds = null)
    {
        var today = VnClock.Today;
        var q = db.TransportOrders.Where(o => o.Status != TransportOrderStatus.Draft && o.CommittedDeliveryDate != null
            && o.CommittedDeliveryDate < today
            && db.WarehouseStocks.Any(s => s.WarehouseId == o.WarehouseId && s.CargoLine.TransportOrderId == o.Id && s.QtyOnHand > 0));
        if (orderIds is not null) q = q.Where(o => orderIds.Contains(o.Id));
        var orders = await q.Select(o => new { o.Id, Due = o.CommittedDeliveryDate!.Value }).ToListAsync();
        if (orders.Count == 0) return new();
        var ids = orders.Select(o => o.Id).ToList();
        var marks = await db.DeliveryDeadlineChanges.Where(c => ids.Contains(c.TransportOrderId) && c.IsFeeOnly && c.OldDate != null)
            .Select(c => new { c.Id, c.TransportOrderId, OldDate = c.OldDate!.Value, c.RequestedBy })
            .ToListAsync();
        return orders.ToDictionary(o => o.Id, o =>
        {
            var mark = marks.Where(m => m.TransportOrderId == o.Id && m.OldDate.Date == o.Due.Date).OrderByDescending(m => m.Id).FirstOrDefault();
            return new Item(o.Id, (today - o.Due.Date).Days, mark?.RequestedBy);
        });
    }

    /// <summary>Số đơn đang nằm kho quá hạn mà CHƯA ai chọn do bên nào (cho số đỏ trên menu).</summary>
    public static async Task<int> CountUndecidedAsync(IApplicationDbContext db)
        => (await LoadAsync(db)).Values.Count(i => i.Decision is null);
}
