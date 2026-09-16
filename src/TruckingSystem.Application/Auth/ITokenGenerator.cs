namespace TruckingSystem.Application.Auth;

/// <summary>Sinh JWT token — cũng là chi tiết kỹ thuật, Application chỉ cần biết "có token trả về".</summary>
public interface ITokenGenerator
{
    string GenerateToken(string userId, string username, IList<string> roles);
}
