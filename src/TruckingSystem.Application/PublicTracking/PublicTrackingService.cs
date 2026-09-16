using Microsoft.EntityFrameworkCore;
using TruckingSystem.Application.Common.Interfaces;

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
        var order = await _db.TransportOrders.FirstOrDefaultAsync(o => o.PublicTrackingToken == token);
        if (order is null) return null;

        var lastWaypoint = await _db.Waypoints
            .Where(w => w.Trip.TransportOrderId == order.Id)
            .OrderByDescending(w => w.Timestamp)
            .FirstOrDefaultAsync();

        return new PublicTrackingResponse(
            order.Code, order.Status.ToString(), order.ProgressPercent,
            order.PickupPoint, order.DropoffPoint,
            lastWaypoint?.Lat, lastWaypoint?.Lng, lastWaypoint?.Timestamp
        );
    }
}
