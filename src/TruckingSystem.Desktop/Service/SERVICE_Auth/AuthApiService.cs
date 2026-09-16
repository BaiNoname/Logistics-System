using Newtonsoft.Json;
using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_Auth;

namespace TruckingSystem.Desktop.SERVICE_Auth;

public class AuthApiService
{
    private readonly ApiConnection _conn;

    public AuthApiService(ApiConnection conn)
    {
        _conn = conn;
    }

    public async Task<LoginResult> LoginAsync(string username, string password)
    {
        var (isSuccess, body) = await _conn.PostRawAsync("/api/auth/login", new { username, password });
        var result = JsonConvert.DeserializeObject<LoginResult>(body)
                     ?? new LoginResult { Success = false, Message = "Không đọc được phản hồi từ server." };

        if (result.Success && result.Token != null)
            _conn.SetToken(result.Token);

        return result;
    }
}
