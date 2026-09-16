using Microsoft.EntityFrameworkCore;
using TruckingSystem.Application.Common.Interfaces;
using TruckingSystem.Domain.Entities;
using TruckingSystem.Domain.Enums;

namespace TruckingSystem.Application.Trips;

/// <summary>
/// Cài đặt use case Trip — chỉ phụ thuộc IApplicationDbContext (interface),
/// KHÔNG phụ thuộc AppDbContext/Npgsql cụ thể. Nhờ vậy có thể viết Unit Test
/// cho class này bằng cách mock IApplicationDbContext, không cần dựng
/// PostgreSQL thật — đúng lợi ích lớn nhất của Clean Architecture đã bàn.
/// </summary>
public class TripService : ITripService
{
    private readonly IApplicationDbContext _db;

    public TripService(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<ServiceResult> PingAsync(int tripId, GpsPingRequest request)
    {
        var trip = await _db.Trips.FindAsync(tripId);
        if (trip is null)
            return new ServiceResult(false, "Không tìm thấy chuyến.");

        _db.Waypoints.Add(new Waypoint
        {
            TripId = tripId,
            Lat = request.Lat,
            Lng = request.Lng,
            Accuracy = request.Accuracy,
            SpeedKmh = request.SpeedKmh,
            Timestamp = request.Timestamp,
        });

        if (trip.Status == TripStatus.Planned)
        {
            trip.Status = TripStatus.InProgress;
            trip.StartTime ??= request.Timestamp;
        }

        await _db.SaveChangesAsync();

        // TODO: bắn SignalR broadcast ở đây khi làm Live Fleet Tracking thật.
        // Vì IApplicationDbContext không "biết" SignalR (đúng nguyên tắc), việc
        // này nên thông qua 1 interface riêng, ví dụ INotificationPublisher,
        // implement bên Infrastructure — làm ở bước sau khi cần.

        return new ServiceResult(true);
    }

    public async Task<ServiceResult> ConfirmArrivalAsync(int tripId, ConfirmArrivalRequest request)
    {
        var trip = await _db.Trips
            .Include(t => t.TripLines)
            .Include(t => t.TransportOrder)
                .ThenInclude(o => o.CargoLines)
            .FirstOrDefaultAsync(t => t.Id == tripId);

        if (trip is null)
            return new ServiceResult(false, "Không tìm thấy chuyến.");

        trip.Status = TripStatus.Completed;
        trip.EndTime = DateTime.UtcNow;

        foreach (var line in trip.TripLines)
        {
            line.QtyDelivered = line.QtyPicked > 0 ? line.QtyPicked : line.QtyPlanned;
            line.Status = TripLineStatus.Delivered;
        }

        // Lưu trước khi tính tổng — SumAsync dịch ra câu SQL riêng, đọc thẳng
        // DB, nên phải lưu thay đổi TripLine xuống trước, nếu không sẽ đọc
        // phải giá trị QtyDelivered cũ (bug đã gặp và sửa ở vòng lặp trước).
        await _db.SaveChangesAsync();

        var order = trip.TransportOrder;
        var totalQty = order.CargoLines.Sum(c => c.QtyTotal);
        var deliveredQty = await _db.TripLines
            .Where(tl => tl.CargoLine.TransportOrderId == order.Id)
            .SumAsync(tl => tl.QtyDelivered);

        order.ProgressPercent = totalQty == 0 ? 0 : Math.Round(deliveredQty / totalQty * 100, 2);

        if (deliveredQty >= totalQty && totalQty > 0)
        {
            order.Status = TransportOrderStatus.Done;
            _db.AuditLogs.Add(new AuditLog
            {
                Action = "AutoComplete",
                EntityName = nameof(TransportOrder),
                EntityId = order.Id,
                Detail = $"Tự động hoàn tất vì đã giao đủ {deliveredQty}/{totalQty}.",
            });
        }

        await _db.SaveChangesAsync();

        return new ServiceResult(true);
    }
}
