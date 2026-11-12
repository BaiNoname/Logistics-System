using Microsoft.Maui.Devices;
using Microsoft.Maui.Storage;

namespace LogisticsSystem.Employee.Config;

/// <summary>
/// Cấu hình kết nối tới TruckingSystem.API. Mặc định theo Properties/launchSettings.json của API
/// (https://localhost:8080). Có thể đổi trên màn hình đăng nhập.
/// </summary>
public static class AppSettings
{
    private const string BaseUrlKey = "api.baseUrl";

    public static string DefaultBaseUrl =>
        DeviceInfo.Platform == DevicePlatform.Android
            ? "https://10.0.2.2:8080/"   // máy ảo Android trỏ về localhost của máy phát triển
            : "https://localhost:8080/";

    public static string ApiBaseUrl
    {
        get => Preferences.Default.Get(BaseUrlKey, DefaultBaseUrl);
        set => Preferences.Default.Set(BaseUrlKey, value);
    }

    /// <summary>Kiểm tra và chuẩn hoá địa chỉ máy chủ (thêm https:// nếu thiếu, thêm dấu / cuối).</summary>
    public static bool TryNormalize(string? input, out string normalized)
    {
        normalized = string.Empty;
        var value = input?.Trim();
        if (string.IsNullOrEmpty(value)) return false;

        if (!value.Contains("://", StringComparison.Ordinal))
            value = "https://" + value;

        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            return false;

        normalized = value.EndsWith('/') ? value : value + "/";
        return true;
    }
}
