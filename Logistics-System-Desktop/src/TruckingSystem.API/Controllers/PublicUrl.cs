using Microsoft.AspNetCore.Http;

namespace TruckingSystem.API.Controllers;

/// <summary>Địa chỉ CÔNG KHAI của API ghi lên lịch Google (Server URL cho app tài xế, link theo dõi của khách).
/// Ưu tiên cấu hình "App:PublicBaseUrl" (VD https://trucking.logisticssystem.dpdns.org) — nếu không,
/// Vận hành thao tác qua https://localhost:8080 thì tài xế / khách nhận link localhost không mở được.</summary>
public static class PublicUrl
{
    public static string For(HttpRequest request)
    {
        var config = request.HttpContext.RequestServices.GetService<IConfiguration>();
        var configured = config?["App:PublicBaseUrl"];
        return !string.IsNullOrWhiteSpace(configured) ? configured.Trim().TrimEnd('/') : $"{request.Scheme}://{request.Host}";
    }
}
