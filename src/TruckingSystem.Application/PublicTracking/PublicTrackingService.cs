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
        var activeTrip = order.Trips.FirstOrDefault(t => t.Status == TripStatus.InProgress);

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
            return "Đơn đang chờ điều phối xe";

        var pickupTrip = trips.FirstOrDefault(t => t.LegType == TripLegType.Pickup);
        var deliveryTrip = trips.FirstOrDefault(t => t.LegType == TripLegType.Delivery);

        if (deliveryTrip?.Status == TripStatus.InProgress) return "🚚 Đang trên đường giao tới bạn";
        if (deliveryTrip?.Status == TripStatus.Completed) return "✅ Đã giao hàng thành công";

        if (pickupTrip != null)
        {
            if (pickupTrip.Status == TripStatus.InProgress) return "🔽 Đang lấy hàng từ người gửi";
            if (pickupTrip.Status == TripStatus.Completed) return "📦 Hàng đã về kho, đang chuẩn bị giao";
        }

        return "Đơn đang được xử lý";
    }
}