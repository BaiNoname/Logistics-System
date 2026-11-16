using System.Net;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;

namespace TruckingSystem.ChatBot.Infrastructure;

/// <summary>
/// Giới hạn chatbot công khai 10 lượt/phút/khách cho TruckingSystem.ChatBot.API.
/// Sau Cloudflare Tunnel mọi request tới từ cloudflared (loopback), nên khi đó lấy IP thật trong CF-Connecting-IP;
/// request không đi qua loopback thì bỏ qua header (khách tự gửi header giả để lách giới hạn).
/// </summary>
public static class FreightChatRateLimit
{
    public const string PolicyName = "freight-chat";

    public static IServiceCollection AddFreightChatRateLimiter(this IServiceCollection services)
        => services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddPolicy(PolicyName, context => RateLimitPartition.GetFixedWindowLimiter(
                ClientKey(context),
                _ => new FixedWindowRateLimiterOptions { PermitLimit = 10, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
        });

    public static string ClientKey(HttpContext context)
    {
        var remote = context.Connection.RemoteIpAddress;
        if (remote is not null && IPAddress.IsLoopback(remote)
            && IPAddress.TryParse(context.Request.Headers["CF-Connecting-IP"].ToString().Trim(), out var client))
            return client.ToString();
        return remote?.ToString() ?? "unknown";
    }
}
