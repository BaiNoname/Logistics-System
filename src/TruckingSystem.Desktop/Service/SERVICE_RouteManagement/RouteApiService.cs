using System.Globalization;
using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_RouteManagement;

namespace TruckingSystem.Desktop.SERVICE_RouteManagement;

public class RouteApiService
{
    private readonly ApiConnection _conn;

    public RouteApiService(ApiConnection conn)
    {
        _conn = conn;
    }

    public Task<RouteResultItem> CalculateAsync(double fromLat, double fromLng, double toLat, double toLng)
    {
        // QUAN TRỌNG: dùng InvariantCulture — nếu máy Windows đặt ngôn ngữ Việt Nam,
        // double.ToString() mặc định sẽ ra "10,79" (dấu phẩy) thay vì "10.79" (dấu
        // chấm), phá vỡ query string khi Backend đọc lại thành số.
        string F(double d) => d.ToString(CultureInfo.InvariantCulture);
        var query = $"/api/routes/calculate?fromLat={F(fromLat)}&fromLng={F(fromLng)}&toLat={F(toLat)}&toLng={F(toLng)}";
        return _conn.GetAsync<RouteResultItem>(query)!;
    }
}
