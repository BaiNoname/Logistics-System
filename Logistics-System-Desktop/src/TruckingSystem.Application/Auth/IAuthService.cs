namespace TruckingSystem.Application.Auth;

/// <summary>
/// Use case "đăng nhập". Application chỉ khai báo CẦN làm gì (interface),
/// còn LÀM NHƯ THẾ NÀO (dùng ASP.NET Core Identity, UserManager, SignInManager...)
/// là chi tiết kỹ thuật — thuộc về Infrastructure, vì Identity gắn chặt với
/// EF Core + cách lưu mật khẩu, đúng bản chất "chi tiết dễ thay đổi" mà
/// Clean Architecture muốn cách ly khỏi lõi nghiệp vụ.
/// </summary>
public interface IAuthService
{
    Task<LoginResult> LoginAsync(LoginRequest request);
}
