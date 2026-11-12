using Microsoft.Maui.Storage;

namespace LogisticsSystem.Employee.Services;

/// <summary>Lưu phiên đăng nhập. Token nằm trong SecureStorage; cờ trạng thái nằm trong Preferences để kiểm tra đồng bộ.</summary>
public sealed class SessionService
{
    private const string TokenKey = "auth.token";
    private const string UserNameKey = "auth.userName";
    private const string LoggedInKey = "auth.loggedIn";

    public bool IsLoggedIn => Preferences.Default.Get(LoggedInKey, false);

    public string DisplayName => Preferences.Default.Get(UserNameKey, string.Empty);

    public async Task<string?> GetTokenAsync()
    {
        try
        {
            return await SecureStorage.Default.GetAsync(TokenKey);
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task SaveAsync(string token, string displayName)
    {
        await SecureStorage.Default.SetAsync(TokenKey, token);
        Preferences.Default.Set(UserNameKey, displayName);
        Preferences.Default.Set(LoggedInKey, true);
    }

    /// <summary>Cập nhật tên hiển thị theo hồ sơ nhân viên lấy từ API.</summary>
    public void UpdateDisplayName(string fullName)
    {
        if (!string.IsNullOrWhiteSpace(fullName))
            Preferences.Default.Set(UserNameKey, fullName);
    }

    public void Clear()
    {
        try
        {
            SecureStorage.Default.Remove(TokenKey);
        }
        catch (Exception)
        {
            // Bỏ qua: kho bảo mật có thể không khả dụng.
        }

        Preferences.Default.Remove(UserNameKey);
        Preferences.Default.Set(LoggedInKey, false);
    }
}
