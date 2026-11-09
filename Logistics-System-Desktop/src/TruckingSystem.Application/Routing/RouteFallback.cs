namespace TruckingSystem.Application.Routing;

/// <summary>Ước lượng thời gian lái khi KHÔNG lấy được lộ trình thật từ VietMap (lỗi mạng / hết
/// quota): đường chim bay × 1,3 (hệ số đường bộ) ở tốc độ trung bình 50 km/h của xe tải. Chỉ
/// dùng để tính SỐ NGÀY chuyến (giấy tờ, bằng lái, ngày làm phải phủ đủ) — không dùng làm km kế
/// hoạch của báo cáo.</summary>
public static class RouteFallback
{
    public static int DriveMinutes(double lat1, double lng1, double lat2, double lng2)
    {
        const double earthKm = 6371;
        static double Rad(double d) => d * Math.PI / 180;
        var dLat = Rad(lat2 - lat1);
        var dLng = Rad(lng2 - lng1);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
              + Math.Cos(Rad(lat1)) * Math.Cos(Rad(lat2)) * Math.Sin(dLng / 2) * Math.Sin(dLng / 2);
        var roadKm = 2 * earthKm * Math.Asin(Math.Sqrt(a)) * 1.3;
        return Math.Max(1, (int)Math.Ceiling(roadKm / 50 * 60));
    }
}
