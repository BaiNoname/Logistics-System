namespace TruckingSystem.Application.FreightChat;

/// <summary>Kho có thể nhận lô hàng; Suitable = kho có khu (có ô kệ) phù hợp loại hàng dễ vỡ/nguy hiểm/thường.</summary>
public record WarehouseCandidate(int Id, string Name, string? Address, double Lat, double Lng, bool Suitable);

/// <summary>Chọn kho trung chuyển gần nơi lấy hàng nhất (đường chim bay), ưu tiên kho có khu phù hợp.</summary>
public static class WarehouseLocator
{
    public static double DistanceKm(double lat1, double lng1, double lat2, double lng2)
    {
        const double R = 6371;
        double Rad(double degrees) => degrees * Math.PI / 180;
        var a = Math.Pow(Math.Sin(Rad(lat2 - lat1) / 2), 2)
            + Math.Cos(Rad(lat1)) * Math.Cos(Rad(lat2)) * Math.Pow(Math.Sin(Rad(lng2 - lng1) / 2), 2);
        return R * 2 * Math.Asin(Math.Sqrt(Math.Clamp(a, 0, 1)));
    }

    /// <summary>
    /// Kho gần nhất trong các kho có tọa độ hợp lệ: chỉ xét kho phù hợp nếu có, ngược lại mọi kho (Suitable = false).
    /// null khi không kho nào có tọa độ hợp lệ.
    /// </summary>
    public static (WarehouseCandidate Warehouse, bool Suitable)? Nearest(IEnumerable<WarehouseCandidate> candidates, double lat, double lng)
    {
        var valid = candidates.Where(c => Valid(c.Lat, c.Lng)).ToList();
        if (valid.Count == 0) return null;
        var suitable = valid.Where(c => c.Suitable).ToList();
        var pool = suitable.Count > 0 ? suitable : valid;
        var best = pool.MinBy(c => DistanceKm(lat, lng, c.Lat, c.Lng))!;
        return (best, suitable.Count > 0);
    }

    public static bool Valid(double lat, double lng)
        => double.IsFinite(lat) && double.IsFinite(lng) && Math.Abs(lat) <= 90 && Math.Abs(lng) <= 180 && (lat != 0 || lng != 0);
}
