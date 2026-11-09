using TruckingSystem.Domain.Enums;

namespace TruckingSystem.Application.Common;

/// <summary>"Điểm đi → Điểm đến" của 1 chặng — CÙNG quy tắc với tọa độ / bản đồ của chuyến (hàng luôn
/// đi qua kho trung chuyển):
///   Lấy hàng:  Điểm lấy hàng (đối tác) → Kho
///   Giao hàng: Kho → Điểm giao hàng (khách nhận)
///   Trả hàng:  Kho → Điểm lấy hàng ban đầu (trả lại đối tác khi hủy đơn)
/// Tên địa điểm lấy từ danh mục Địa điểm; đơn cũ không chọn địa điểm thì dùng địa chỉ đã ghi trên đơn.</summary>
public static class TripRouteText
{
    public static (string From, string To) For(TripLegType leg, string? warehouseName,
        string? pickupName, string? pickupText, string? dropoffName, string? dropoffText)
    {
        var warehouse = string.IsNullOrWhiteSpace(warehouseName) ? "🏬 (chưa gắn kho)" : "🏬 " + warehouseName.Trim();
        var pickup = Pick(pickupName, pickupText);
        var dropoff = Pick(dropoffName, dropoffText);
        return leg switch
        {
            TripLegType.Pickup => (pickup, warehouse),
            TripLegType.Return => (warehouse, pickup),
            _ => (warehouse, dropoff),
        };
    }

    private static string Pick(string? name, string? text)
        => !string.IsNullOrWhiteSpace(name) ? name.Trim()
         : !string.IsNullOrWhiteSpace(text) ? text.Trim()
         : "—";
}
