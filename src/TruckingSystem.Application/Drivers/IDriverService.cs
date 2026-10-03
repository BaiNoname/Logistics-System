namespace TruckingSystem.Application.Drivers;

public interface IDriverService
{
    /// <summary>Danh sách TẤT CẢ tài xế (nhân viên đã có DriverProfile),
    /// kèm trạng thái Availability tính động theo đúng logic thật dùng khi
    /// Chia xe — xem chi tiết trong DriverService.GetAllAsync().</summary>
    Task<List<DriverListItemDto>> GetAllAsync();
}