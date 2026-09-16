namespace TruckingSystem.Application.Routing;

/// <summary>Trừu tượng hóa nhà cung cấp định tuyến — Application chỉ biết
/// interface này, không biết đang gọi VietMap hay nhà cung cấp khác. Cài đặt
/// thật (VietMapRouteProvider) nằm ở Infrastructure.</summary>
public interface IRouteProvider
{
    Task<RouteResult> GetRouteAsync(RouteRequest request);
}
