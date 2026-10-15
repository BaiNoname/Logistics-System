using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_ResourceManagement;

namespace TruckingSystem.Desktop.SERVICE_ResourceManagement
{
    /// <summary>API màn "🧭 Nguồn lực theo kho" — tạo bằng new ResourceApiService(api.Connection)
    /// (không cần sửa ApiServices dùng chung).</summary>
    public class ResourceApiService
    {
        private readonly ApiConnection _conn;

        public ResourceApiService(ApiConnection conn)
        {
            _conn = conn;
        }

        /// <summary>warehouseId null = mọi kho.</summary>
        public async Task<ResourceOverview> GetOverviewAsync(int? warehouseId)
            => await _conn.GetAsync<ResourceOverview>("/api/resources/overview" + (warehouseId is int w ? $"?warehouseId={w}" : ""))
               ?? new ResourceOverview();
    }
}
