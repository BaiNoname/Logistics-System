using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_VehicleManagement;

namespace TruckingSystem.Desktop.SERVICE_VehicleManagement
{
    public class VehicleApiService
    {
        private readonly ApiConnection _conn;

        public VehicleApiService(ApiConnection conn)
        {
            _conn = conn;
        }

        public Task<List<VehicleListItem>> GetVehiclesAsync()
            => _conn.GetAsync<List<VehicleListItem>>("/api/vehicles")!;

        public Task CreateVehicleAsync(string plateNumber, string vehicleType, decimal capacityQty, decimal capacityKg, decimal capacityM3)
            => _conn.PostAsync<object>("/api/vehicles", new { plateNumber, vehicleType, capacityQty, capacityKg, capacityM3 });

        public Task<VehicleActionResultItem> UpdateVehicleAsync(int id, string plateNumber, string vehicleType, decimal capacityQty, decimal capacityKg, decimal capacityM3, string status)
            => _conn.PutAsync<VehicleActionResultItem>($"/api/vehicles/{id}", new { plateNumber, vehicleType, capacityQty, capacityKg, capacityM3, status })!;

        public Task<List<VehicleDocumentItem>> GetDocumentsAsync(int vehicleId)
            => _conn.GetAsync<List<VehicleDocumentItem>>($"/api/vehicles/{vehicleId}/documents")!;

        public Task<VehicleDocumentItem> CreateDocumentAsync(int vehicleId, string docType, DateTime issueDate, DateTime expiryDate, string? fileUrl)
            => _conn.PostAsync<VehicleDocumentItem>($"/api/vehicles/{vehicleId}/documents", new { docType, issueDate, expiryDate, fileUrl })!;

        public Task<VehicleActionResultItem> DeleteDocumentAsync(int documentId)
            => _conn.DeleteAsync<VehicleActionResultItem>($"/api/vehicles/documents/{documentId}")!;
    }
}