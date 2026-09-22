namespace TruckingSystem.Application.Vehicles;

public interface IVehicleService
{
    Task<List<VehicleListItemDto>> GetAllAsync();
    Task<VehicleListItemDto> CreateAsync(CreateVehicleRequest request);

    /// <summary>Sửa thông tin xe — cho phép đổi cả trạng thái thủ công.</summary>
    Task<VehicleActionResult> UpdateAsync(int id, UpdateVehicleRequest request);

    /// <summary>Danh sách giấy tờ của 1 xe.</summary>
    Task<List<VehicleDocumentDto>> GetDocumentsAsync(int vehicleId);

    /// <summary>Nhập giấy tờ xe mới.</summary>
    Task<VehicleDocumentDto> CreateDocumentAsync(int vehicleId, CreateVehicleDocumentRequest request);

    Task<VehicleActionResult> DeleteDocumentAsync(int documentId);
}