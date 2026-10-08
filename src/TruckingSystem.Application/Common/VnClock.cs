namespace TruckingSystem.Application.Common;

/// <summary>Giờ Việt Nam (UTC+7) — DÙNG CHUNG cho mọi phép so "hôm nay / hết hạn".
/// DB lưu thời điểm theo UTC; nhưng NGÀY hết hạn giấy tờ, bằng lái, ngày làm của tài
/// xế là NGÀY THEO LỊCH VIỆT NAM. So với DateTime.UtcNow thì từ 0h–7h sáng bị lệch
/// 1 ngày (giấy tờ hết hạn ngày 10 bị chặn từ 7h sáng ngày 10 trong khi Desktop vẫn
/// báo "Còn hạn").</summary>
public static class VnClock
{
    /// <summary>Ngày hôm nay theo giờ Việt Nam (00:00).</summary>
    public static DateTime Today => DateTime.UtcNow.AddHours(7).Date;

    /// <summary>Đưa 1 mốc giờ người dùng nhập (Desktop gửi kèm +07:00, hoặc UTC "Z",
    /// hoặc không kèm múi giờ) về GIỜ VIỆT NAM, không kèm múi giờ — dùng cho các cột
    /// lưu theo giờ VN (VD hạn giao cam kết), không phụ thuộc múi giờ máy chủ.</summary>
    public static DateTime? ToVnWallClock(DateTime? value)
    {
        if (value is null) return null;
        var v = value.Value;
        var vn = v.Kind switch
        {
            DateTimeKind.Utc => v.AddHours(7),
            DateTimeKind.Local => v.ToUniversalTime().AddHours(7),
            _ => v,
        };
        return DateTime.SpecifyKind(vn, DateTimeKind.Unspecified);
    }

    /// <summary>Giờ Việt Nam (không múi giờ) -> UTC (không múi giờ, đúng quy ước cột UTC trong DB).</summary>
    public static DateTime VnToUtc(DateTime vnWallClock) => DateTime.SpecifyKind(vnWallClock.AddHours(-7), DateTimeKind.Unspecified);
}
