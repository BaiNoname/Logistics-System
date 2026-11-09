namespace TruckingSystem.Domain.Attendance;

/// <summary>Quy tắc nghiệp vụ chấm công QR + GPS (thuần, không phụ thuộc hạ tầng).</summary>
public static class AttendanceRules
{
    public static decimal PaidDays(DateTime checkIn, DateTime checkOut)
    {
        if (checkOut <= checkIn) throw new ArgumentOutOfRangeException(nameof(checkOut));
        var hours = (checkOut - checkIn).TotalHours;
        return hours >= 8 ? 1m : hours >= 4 ? 0.5m : 0m;
    }

    /// <summary>Tiền tố nội dung QR — phân biệt mã chấm công với QR bất kỳ khác.</summary>
    public const string QrPrefix = "TSATT:";
    /// <summary>Lần quét thứ 2 (Check-out) phải cách Check-in ít nhất chừng này — chặn quét lặp tức thời.</summary>
    public static readonly TimeSpan MinCheckOutGap = TimeSpan.FromMinutes(1);
    /// <summary>Chỉ được xin điều chỉnh cho ngày trong khoảng này tính tới hôm nay.</summary>
    public const int AdjustmentWindowDays = 7;
    public const int MinRadiusMeters = 20, MaxRadiusMeters = 5000;
    public const int MinAccuracyMeters = 5, MaxAccuracyMeters = 1000;

    /// <summary>Khoảng cách mặt đất (mét) giữa 2 tọa độ — công thức Haversine.</summary>
    public static double DistanceMeters(double lat1, double lng1, double lat2, double lng2)
    {
        const double earthRadius = 6371000;
        static double Rad(double deg) => deg * Math.PI / 180;
        var dLat = Rad(lat2 - lat1);
        var dLng = Rad(lng2 - lng1);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
              + Math.Cos(Rad(lat1)) * Math.Cos(Rad(lat2)) * Math.Sin(dLng / 2) * Math.Sin(dLng / 2);
        return earthRadius * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }

    public static bool IsValidCoordinate(double lat, double lng)
        => !double.IsNaN(lat) && !double.IsNaN(lng) && lat is >= -90 and <= 90 && lng is >= -180 and <= 180
           && !(lat == 0 && lng == 0);

    /// <summary>Đọc Token từ nội dung QR ("TSATT:token"); null nếu không phải mã chấm công.</summary>
    public static string? ParseToken(string? qrContent)
    {
        var text = qrContent?.Trim();
        if (string.IsNullOrEmpty(text) || !text.StartsWith(QrPrefix, StringComparison.Ordinal)) return null;
        var token = text[QrPrefix.Length..];
        return token.Length is >= 16 and <= 128 && token.All(c => char.IsAsciiLetterOrDigit(c) || c is '-' or '_') ? token : null;
    }

    /// <summary>Kiểm tra vị trí; trả về lý do từ chối hoặc null nếu hợp lệ.</summary>
    public static string? CheckLocation(double lat, double lng, double accuracy, bool isMock,
        double siteLat, double siteLng, int radiusMeters, int maxAccuracyMeters, out double distance)
    {
        distance = double.NaN;
        if (!IsValidCoordinate(lat, lng) || double.IsNaN(accuracy) || accuracy < 0)
            return "Không đọc được vị trí hợp lệ từ thiết bị.";
        if (isMock) return "Thiết bị đang dùng vị trí giả lập — không thể chấm công.";
        distance = DistanceMeters(lat, lng, siteLat, siteLng);
        if (accuracy > maxAccuracyMeters)
            return $"Sai số GPS quá lớn ({accuracy:0} m, cho phép tối đa {maxAccuracyMeters} m). Ra chỗ thoáng rồi thử lại.";
        if (distance > radiusMeters)
            return $"Bạn đang cách địa điểm chấm công khoảng {distance:0} m (cho phép {radiusMeters} m).";
        return null;
    }

    /// <summary>Ngày có được xin điều chỉnh không (không tương lai, trong cửa sổ cho phép).</summary>
    public static bool CanRequestAdjustment(DateTime workDate, DateTime today)
        => workDate.Date <= today.Date && workDate.Date >= today.Date.AddDays(-AdjustmentWindowDays);
}
