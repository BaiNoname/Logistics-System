using LogisticsSystem.Employee.Models;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices.Sensors;

namespace LogisticsSystem.Employee.Services;

/// <summary>Điểm truy cập dịch vụ dùng chung (đủ cho quy mô ứng dụng hiện tại).</summary>
public static class AppServices
{
    public static SessionService Session { get; } = new();
    public static ApiClient Api { get; } = new(Session);
    public static AuthService Auth { get; } = new(Api);
    public static AttendanceService Attendance { get; } = new(Api);
}

public sealed class AuthService(ApiClient api)
{
    public async Task<LoginResult> LoginAsync(string username, string password, CancellationToken ct = default)
    {
        var result = await api.PostAsync<LoginResult>(
            ApiRoutes.Login,
            new LoginRequest { Username = username, Password = password },
            ct);

        if (result is null || !result.Success || string.IsNullOrEmpty(result.Token))
            throw new ApiException(result?.Message ?? "Đăng nhập không thành công.");

        return result;
    }
}

public sealed class AttendanceService(ApiClient api)
{
    public Task<MobileTodayDto?> GetMeAsync(CancellationToken ct = default) =>
        api.GetAsync<MobileTodayDto>(ApiRoutes.Me, ct);

    public Task<AttendanceScanResponse?> ScanAsync(AttendanceScanRequest request, CancellationToken ct = default) =>
        api.PostAsync<AttendanceScanResponse>(ApiRoutes.Scan, request, ct);

    public async Task<List<AttendanceDayDto>> GetHistoryAsync(DateTime from, DateTime to, CancellationToken ct = default)
    {
        var path = $"{ApiRoutes.MyHistory}?from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}";
        return await api.GetAsync<List<AttendanceDayDto>>(path, ct) ?? [];
    }

    public async Task<List<AdjustmentRequestDto>> GetAdjustmentsAsync(CancellationToken ct = default) =>
        await api.GetAsync<List<AdjustmentRequestDto>>(ApiRoutes.MyAdjustments, ct) ?? [];

    public async Task<List<AdjustmentReasonDto>> GetAdjustmentReasonsAsync(CancellationToken ct = default) =>
        await api.GetAsync<List<AdjustmentReasonDto>>(ApiRoutes.AdjustmentReasons, ct) ?? [];

    public Task<AttendanceResult?> CreateAdjustmentAsync(CreateAdjustmentRequest request, CancellationToken ct = default) =>
        api.PostAsync<AttendanceResult>(ApiRoutes.MyAdjustments, request, ct);
}

public static class LocationService
{
    /// <summary>Lấy vị trí thiết bị tại thời điểm quét, kèm kiểm tra quyền.</summary>
    public static async Task<Location> GetCurrentAsync(CancellationToken ct = default)
    {
        var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
        if (status != PermissionStatus.Granted)
            status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

        if (status != PermissionStatus.Granted)
            throw new InvalidOperationException("Chưa cấp quyền vị trí. Vui lòng cấp quyền trong Cài đặt để chấm công.");

        var request = new GeolocationRequest(GeolocationAccuracy.High, TimeSpan.FromSeconds(15));
        var location = await Geolocation.Default.GetLocationAsync(request, ct);

        return location ?? throw new InvalidOperationException("Không lấy được vị trí. Hãy bật GPS và thử lại.");
    }
}
