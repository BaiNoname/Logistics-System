using Microsoft.EntityFrameworkCore;
using TruckingSystem.Application.Common.Interfaces;

namespace TruckingSystem.Application.Vehicles;

/// <summary>NHIÊN LIỆU ĐANG CÓ TRONG BÌNH của xe — 1 quy tắc dùng chung cho màn Đội xe, form
/// Mức nhiên liệu và kiểm tra khi Ghi đổ xăng:
///   Hiện có = Mức đo gần nhất (Vận hành khai báo khi kiểm tra đồng hồ / đổ đầy bình)
///           + Tổng lít các lần ĐỔ XĂNG sau mốc đo (FuelLog của các chuyến của xe)
///           − TIÊU HAO ước tính sau mốc đo = km đi thật (cộng dồn điểm GPS) × định mức L/100km
///             (định mức chốt lúc xuất phát của chuyến; chưa chốt thì lấy của xe).
/// Kẹp trong [0, dung tích bình]. Tính động mỗi lần xem -> xóa / ghi thêm 1 lần đổ xăng hay GPS
/// gửi bù đều tự đúng, không cộng dồn sai theo thời gian. Định mức là ước tính, nên định kỳ Vận
/// hành đo lại (khai báo mức mới = đặt lại mốc).</summary>
public static class VehicleFuelLevelRules
{
    public record Level(
        int VehicleId,
        decimal? TankCapacityLiters,
        decimal? MeasuredLiters,
        DateTime? MeasuredAtUtc,
        decimal RefueledLiters,
        decimal ConsumedLiters,
        decimal ConsumedKm,
        decimal? CurrentLiters)
    {
        /// <summary>Còn trống trong bình (lít) — null khi chưa khai dung tích / mức đo.</summary>
        public decimal? FreeLiters => TankCapacityLiters is decimal cap && CurrentLiters is decimal cur ? Math.Max(0, cap - cur) : null;
    }

    public static async Task<Dictionary<int, Level>> ComputeAsync(IApplicationDbContext db, int? vehicleId = null)
    {
        var vehicles = await db.Vehicles
            .Where(v => vehicleId == null || v.Id == vehicleId)
            .Select(v => new { v.Id, v.FuelTankCapacityLiters, v.FuelLevelLiters, v.FuelLevelMeasuredAt, v.FuelConsumptionPer100Km })
            .ToListAsync();
        var result = vehicles.ToDictionary(v => v.Id,
            v => new Level(v.Id, v.FuelTankCapacityLiters, v.FuelLevelLiters, v.FuelLevelMeasuredAt, 0, 0, 0, null));

        var measured = vehicles.Where(v => v.FuelLevelLiters != null && v.FuelLevelMeasuredAt != null).ToList();
        if (measured.Count == 0) return result;
        var ids = measured.Select(v => v.Id).ToList();
        var minAt = measured.Min(v => v.FuelLevelMeasuredAt!.Value);

        // Đổ xăng sau mốc đo (lọc mốc riêng từng xe trong bộ nhớ).
        var refuels = await db.FuelLogs
            .Where(f => ids.Contains(f.Trip.VehicleId) && f.Timestamp > minAt)
            .Select(f => new { f.Trip.VehicleId, f.Timestamp, f.Liters })
            .ToListAsync();

        // Điểm GPS sau mốc đo của các chuyến của xe (để tính km đi thật).
        var points = await db.Waypoints
            .Where(w => ids.Contains(w.Trip.VehicleId) && w.Timestamp > minAt)
            .OrderBy(w => w.TripId).ThenBy(w => w.Timestamp)
            .Select(w => new { w.TripId, w.Trip.VehicleId, w.Timestamp, w.Lat, w.Lng, Norm = w.Trip.FuelConsumptionPer100KmAtStart })
            .ToListAsync();

        foreach (var v in measured)
        {
            var at = v.FuelLevelMeasuredAt!.Value;
            var refueled = refuels.Where(f => f.VehicleId == v.Id && f.Timestamp > at).Sum(f => f.Liters);

            decimal km = 0, consumed = 0;
            foreach (var trip in points.Where(p => p.VehicleId == v.Id && p.Timestamp > at).GroupBy(p => p.TripId))
            {
                var pts = trip.ToList();
                double tripKm = 0;
                for (var i = 1; i < pts.Count; i++) tripKm += HaversineKm(pts[i - 1].Lat, pts[i - 1].Lng, pts[i].Lat, pts[i].Lng);
                var norm = pts[0].Norm ?? v.FuelConsumptionPer100Km ?? 0;
                km += (decimal)tripKm;
                consumed += (decimal)tripKm * norm / 100m;
            }

            var current = v.FuelLevelLiters!.Value + refueled - consumed;
            current = Math.Max(0, current);
            if (v.FuelTankCapacityLiters is decimal cap && cap > 0) current = Math.Min(cap, current);
            result[v.Id] = result[v.Id] with
            {
                RefueledLiters = Math.Round(refueled, 1),
                ConsumedLiters = Math.Round(consumed, 1),
                ConsumedKm = Math.Round(km, 1),
                CurrentLiters = Math.Round(current, 1),
            };
        }
        return result;
    }

    private static double HaversineKm(double lat1, double lng1, double lat2, double lng2)
    {
        const double r = 6371;
        double Rad(double d) => d * Math.PI / 180;
        var dLat = Rad(lat2 - lat1);
        var dLng = Rad(lng2 - lng1);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(Rad(lat1)) * Math.Cos(Rad(lat2)) * Math.Sin(dLng / 2) * Math.Sin(dLng / 2);
        return 2 * r * Math.Asin(Math.Min(1, Math.Sqrt(a)));
    }
}
