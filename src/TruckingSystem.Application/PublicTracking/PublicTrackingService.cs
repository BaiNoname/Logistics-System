using Microsoft.EntityFrameworkCore;
using TruckingSystem.Application.Common.Interfaces;
using TruckingSystem.Domain.Enums;

namespace TruckingSystem.Application.PublicTracking;

public class PublicTrackingService : IPublicTrackingService
{
    private readonly IApplicationDbContext _db;

    public PublicTrackingService(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<PublicTrackingResponse?> TrackAsync(string token)
    {
        var order = await _db.TransportOrders
            .Include(o => o.Trips)
            .FirstOrDefaultAsync(o => o.PublicTrackingToken == token);
        if (order is null) return null;

        // CHỈ lấy GPS từ chặng đang chạy (Pickup hoặc Delivery, tùy giai đoạn
        // nào đang diễn ra) — tránh trộn lẫn 2 chặng gây hiểu nhầm cho khách
        // hàng (VD: khách tưởng xe đang tới chỗ mình, nhưng thật ra là xe đang
        // đi lấy hàng từ người gửi khác).
        // Đơn nhiều xe: ưu tiên xe đang GIAO (đang tới chỗ khách), rồi mới tới xe
        // đang lấy hàng — trước đây lấy đại 1 chuyến bất kỳ.
        // Không hiện xe chặng TRẢ HÀNG (đơn đã hủy, xe chở hàng về người gửi — khách nhận
        // không liên quan) và không hiện GPS gì khi đơn đã hủy.
        var activeTrip = order.Status == TransportOrderStatus.Cancelled ? null : order.Trips
            .Where(t => t.LegType != TripLegType.Return)
            .Where(t => t.Status == TripStatus.InProgress || t.Status == TripStatus.PendingConfirmation)
            .OrderBy(t => t.LegType == TripLegType.Delivery ? 0 : 1)
            .ThenBy(t => t.Status == TripStatus.InProgress ? 0 : 1)
            .ThenBy(t => t.Id)
            .FirstOrDefault();

        double? lastLat = null, lastLng = null;
        DateTime? lastUpdatedAt = null;
        if (activeTrip != null)
        {
            var lastWaypoint = await _db.Waypoints
                .Where(w => w.TripId == activeTrip.Id)
                .OrderByDescending(w => w.Timestamp)
                .FirstOrDefaultAsync();
            lastLat = lastWaypoint?.Lat;
            lastLng = lastWaypoint?.Lng;
            lastUpdatedAt = lastWaypoint?.Timestamp;
        }

        var stage = DetermineStage(order.Status, order.Trips.ToList());

        return new PublicTrackingResponse(
            order.Code, order.Status.ToString(), order.ProgressPercent,
            order.PickupPoint, order.DropoffPoint,
            lastLat, lastLng, lastUpdatedAt, stage
        );
    }

    /// <summary>Diễn giải giai đoạn hiện tại bằng ngôn ngữ dễ hiểu cho khách
    /// hàng — dựa theo trạng thái các Trip (Pickup/Delivery) của đơn, KHÔNG
    /// dùng thẳng tên kỹ thuật (Enum) như "InProgress" khó hiểu với khách.</summary>
    private static string DetermineStage(TransportOrderStatus orderStatus, List<Domain.Entities.Trip> trips)
    {
        if (orderStatus == TransportOrderStatus.Cancelled) return "Đơn hàng đã bị hủy";
        if (orderStatus == TransportOrderStatus.Done) return "✅ Đã giao hàng thành công";
        if (orderStatus is TransportOrderStatus.Draft or TransportOrderStatus.Confirmed)
            return trips.Any(t => t.Status != TripStatus.Cancelled)
                ? "🚛 Đã điều phối xe — chờ xe xuất phát"
                : "Đơn đang chờ điều phối xe";

        // Đơn có thể chia NHIỀU xe — xét TẤT CẢ chuyến (bỏ chuyến đã hủy), không
        // lấy đại 1 chuyến (trước đây có thể báo "Đã giao thành công" trong khi xe
        // thứ 2 còn đang trên đường).
        var active = trips.Where(t => t.Status != TripStatus.Cancelled).ToList();
        var pickups = active.Where(t => t.LegType == TripLegType.Pickup).ToList();
        var deliveries = active.Where(t => t.LegType == TripLegType.Delivery).ToList();

        if (deliveries.Any(t => t.Status == TripStatus.InProgress)) return "🚚 Đang trên đường giao tới bạn";
        if (deliveries.Any(t => t.Status == TripStatus.PendingConfirmation)) return "📍 Tài xế báo đã tới nơi, đang chờ xác nhận giao hàng";
        if (deliveries.Count > 0 && deliveries.All(t => t.Status == TripStatus.Completed)) return "✅ Đã giao hàng thành công";
        if (deliveries.Any(t => t.Status == TripStatus.Completed)) return "🚚 Đã giao 1 phần, phần còn lại đang được chuẩn bị giao";

        if (pickups.Any(t => t.Status == TripStatus.InProgress)) return "🔽 Đang lấy hàng từ người gửi";
        if (pickups.Any(t => t.Status == TripStatus.PendingConfirmation)) return "📦 Tài xế báo đã lấy hàng xong, đang chờ xác nhận";
        if (pickups.Count > 0 && pickups.All(t => t.Status == TripStatus.Completed)) return "📦 Hàng đã về kho, đang chuẩn bị giao";
        if (pickups.Any(t => t.Status == TripStatus.Completed)) return "📦 Đã lấy 1 phần hàng về kho, đang lấy tiếp";

        return "Đơn đang được xử lý";
    }
}