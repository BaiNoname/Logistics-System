using Microsoft.EntityFrameworkCore;
using TruckingSystem.Application.Common.Interfaces;
using TruckingSystem.Domain.Enums;

namespace TruckingSystem.Application.Warehouses;

/// <summary>Kho phải luôn còn KHU (có vị trí) đúng tính chất hàng của các đơn đang xử lý —
/// 1 quy tắc dùng chung cho Tạo/Sửa/Xác nhận đơn, Chia xe và Xóa/Sửa khu, vị trí.
/// Trước đây chỉ kiểm tra lúc tạo đơn: sau đó xóa khu Hàng nguy hiểm (đang trống) hoặc đổi
/// loại khu thì hàng lấy về KHÔNG nhập kho được -> chặng Giao, Hủy đơn đều bị chặn, kẹt cả xe.</summary>
public static class WarehouseZoneRules
{
    public static ZoneType Required(bool isHazmat, bool isFragile)
        => isHazmat ? ZoneType.Hazmat : isFragile ? ZoneType.Fragile : ZoneType.Normal;

    public static string ZoneName(ZoneType z) => z switch
    {
        ZoneType.Hazmat => "Hàng nguy hiểm",
        ZoneType.Fragile => "Hàng dễ vỡ",
        _ => "Hàng thường",
    };

    /// <summary>Loại khu kho đang có (khu có ít nhất 1 vị trí), bỏ qua khu / vị trí sắp xóa,
    /// và tính khu sắp ĐỔI loại theo loại mới.</summary>
    public static async Task<HashSet<ZoneType>> AvailableAsync(IApplicationDbContext db, int warehouseId,
        int? excludeZoneId = null, int? excludeBinId = null, (int ZoneId, ZoneType NewType)? retype = null)
    {
        var zones = await db.WarehouseZones
            .Where(z => z.WarehouseId == warehouseId && z.Id != excludeZoneId
                        && db.StorageBins.Any(b => b.WarehouseZoneId == z.Id && b.Id != excludeBinId))
            .Select(z => new { z.Id, z.ZoneType })
            .ToListAsync();
        return zones.Select(z => retype is { } r && r.ZoneId == z.Id ? r.NewType : z.ZoneType).ToHashSet();
    }

    /// <summary>Lý do thiếu khu (null = đủ) cho danh sách loại khu cần.</summary>
    public static async Task<string?> MissingReasonAsync(IApplicationDbContext db, int warehouseId, IEnumerable<ZoneType> needed)
    {
        var available = await AvailableAsync(db, warehouseId);
        var missing = needed.Distinct().Where(z => !available.Contains(z)).ToList();
        if (missing.Count == 0) return null;
        return $"Kho trung chuyển chưa có khu vực (có vị trí) cho: {string.Join(", ", missing.Select(ZoneName))} — hàng của đơn sẽ không nhập kho được. " +
               "Chọn kho khác hoặc thêm khu vực / vị trí cho kho này trước (màn Kho → Quản lý khu vực).";
    }

    /// <summary>Loại khu mà các đơn ĐANG XỬ LÝ của kho còn cần để nhập hàng: đơn Đã xác nhận /
    /// Đang chạy còn dòng hàng chưa nhập đủ (gồm cả hàng đã về kho, đang chờ nhập).
    /// Trả về theo loại khu -> mã các đơn cần (để báo lỗi rõ).</summary>
    public static async Task<Dictionary<ZoneType, List<string>>> NeededByActiveOrdersAsync(IApplicationDbContext db, int warehouseId)
    {
        var lines = await db.CargoLines
            .Where(c => c.TransportOrder.WarehouseId == warehouseId
                        && (c.TransportOrder.Status == TransportOrderStatus.Confirmed || c.TransportOrder.Status == TransportOrderStatus.InProgress)
                        && (db.GoodsReceipts.Where(r => r.CargoLineId == c.Id).Sum(r => (decimal?)r.QtyReceived) ?? 0) < c.QtyTotal)
            .Select(c => new { c.IsHazmat, c.IsFragile, c.TransportOrder.Code })
            .ToListAsync();
        return lines.GroupBy(l => Required(l.IsHazmat, l.IsFragile))
            .ToDictionary(g => g.Key, g => g.Select(x => x.Code).Distinct().OrderBy(x => x).ToList());
    }

    /// <summary>Thay đổi kho (xóa khu / xóa vị trí / đổi loại khu) có làm MẤT loại khu cuối cùng
    /// mà đơn đang xử lý còn cần không — có thì trả lý do chặn.</summary>
    public static async Task<string?> BlockIfRemovesNeededAsync(IApplicationDbContext db, int warehouseId, string action,
        int? excludeZoneId = null, int? excludeBinId = null, (int ZoneId, ZoneType NewType)? retype = null)
    {
        var needed = await NeededByActiveOrdersAsync(db, warehouseId);
        if (needed.Count == 0) return null;
        var before = await AvailableAsync(db, warehouseId);
        var after = await AvailableAsync(db, warehouseId, excludeZoneId, excludeBinId, retype);
        // Chỉ chặn khi CHÍNH thay đổi này làm mất loại khu (loại khu vốn đã thiếu thì không đổ cho thao tác này).
        var lost = needed.Where(n => before.Contains(n.Key) && !after.Contains(n.Key)).ToList();
        if (lost.Count == 0) return null;
        var detail = string.Join("; ", lost.Select(l =>
            $"khu {ZoneName(l.Key)} — đơn {string.Join(", ", l.Value.Take(5))}{(l.Value.Count > 5 ? $" và {l.Value.Count - 5} đơn khác" : "")}"));
        return $"Không thể {action}: đây là chỗ CUỐI CÙNG của kho cho loại hàng mà các đơn đang xử lý còn phải nhập kho ({detail}). " +
               "Thêm khu vực / vị trí cùng loại khác trước, hoặc chờ các đơn đó nhập kho xong.";
    }
}
