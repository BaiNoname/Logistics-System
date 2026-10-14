namespace TruckingSystem.Application.Drivers;

/// <summary>HẠNG BẰNG LÁI phải đúng với XE được giao — dùng chung cho Chia xe, Đổi xe/tài xế
/// và Bắt đầu chuyến. Nhận cả hạng CŨ (B2, C, D, E, FC — luật 2008, bằng cũ vẫn còn giá trị)
/// và hạng MỚI (B, C1, C, CE... — Luật TT, ATGT đường bộ 2024, hiệu lực 01/01/2025):
///   • Xe tải đến 3,5 tấn: mọi hạng ô tô (B, B1, B2, C1, C, D..., FC, CE).
///   • Xe tải trên 3,5 tấn đến 7,5 tấn: C1, C, D1, D2, D, E, FC, CE, C1E, D1E, D2E, DE.
///   • Xe tải trên 7,5 tấn: C, D, E, FC, CE, DE (D/E cũ và D mới được lái cả hạng C).
///   • Đầu kéo / container / sơ mi rơ moóc (theo tên Loại xe): FC (cũ) hoặc CE (mới).
/// Tải trọng lấy theo Sức chứa (kg) của xe; xe chưa khai kg (0) thì không chặn theo tải trọng.</summary>
public static class DriverLicenseRules
{
    private static readonly HashSet<string> CarClasses = new(StringComparer.OrdinalIgnoreCase)
        { "B", "B1", "B2", "BE", "C1", "C1E", "C", "CE", "D1", "D1E", "D2", "D2E", "D", "DE", "E", "FB2", "FC", "FD", "FE" };
    private static readonly HashSet<string> Over3_5T = new(StringComparer.OrdinalIgnoreCase)
        { "C1", "C1E", "C", "CE", "D1", "D1E", "D2", "D2E", "D", "DE", "E", "FC", "FD", "FE" };
    private static readonly HashSet<string> Over7_5T = new(StringComparer.OrdinalIgnoreCase)
        { "C", "CE", "D", "DE", "E", "FC", "FD", "FE" };
    private static readonly HashSet<string> Tractor = new(StringComparer.OrdinalIgnoreCase)
        { "FC", "CE" };

    private static readonly string[] TractorKeywords = { "dau keo", "container", "cong te no", "ro mooc", "romooc", "ro-mooc", "so mi" };

    public static bool IsTractor(string? vehicleType)
    {
        var n = DriverPositionRules.NormalizeText(vehicleType);
        return n.Length > 0 && TractorKeywords.Any(n.Contains);
    }

    /// <summary>"Hạng cần" để hiển thị: "B2 trở lên", "C1 trở lên", "C trở lên", "FC/CE".</summary>
    public static string RequiredText(decimal capacityKg, string? vehicleType)
        => IsTractor(vehicleType) ? "FC (cũ) hoặc CE (mới)"
         : capacityKg > 7500 ? "C trở lên"
         : capacityKg > 3500 ? "C1 / C trở lên"
         : "B / B2 trở lên";

    /// <summary>null = bằng lái hạng <paramref name="licenseClass"/> được lái xe này;
    /// ngược lại trả câu lý do (không kèm tên tài xế / biển số).</summary>
    public static string? BlockReason(string? licenseClass, decimal capacityKg, string? vehicleType)
    {
        var cls = (licenseClass ?? "").Trim().Replace(" ", "").ToUpperInvariant();
        if (cls.Length == 0) return "chưa khai hạng bằng lái";

        var tractor = IsTractor(vehicleType);
        var allowed = tractor ? Tractor
                    : capacityKg > 7500 ? Over7_5T
                    : capacityKg > 3500 ? Over3_5T
                    : CarClasses;
        if (allowed.Contains(cls)) return null;

        var what = tractor ? $"xe đầu kéo / container ({vehicleType})"
                 : $"xe tải {capacityKg / 1000m:0.##} tấn";
        return $"bằng hạng {cls} không được lái {what} — cần hạng {RequiredText(capacityKg, vehicleType)}";
    }
}
