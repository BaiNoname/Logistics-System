namespace TruckingSystem.Application.SystemManagement;

public interface ISystemManagementService
{
    Task<List<SystemItemDto>> GetDepartmentsAsync();
    Task<List<SystemItemDto>> GetPositionsAsync();
    Task<SystemActionResult> CreateDepartmentAsync(NameRequest request);
    Task<SystemActionResult> CreatePositionAsync(NameRequest request);
    Task<SystemActionResult> UpdateDepartmentAsync(int id, NameRequest request);
    Task<SystemActionResult> UpdatePositionAsync(int id, NameRequest request);
    Task<SystemActionResult> DeleteDepartmentAsync(int id);
    Task<SystemActionResult> DeletePositionAsync(int id);
}
