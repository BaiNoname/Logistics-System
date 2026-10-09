using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_TripManagement;

namespace TruckingSystem.Desktop.SERVICE_TripManagement
{
    /// <summary>Kiểm hàng + ảnh hàng tài xế gửi lúc xác nhận chặng. Tách file riêng —
    /// tạo bằng: new TripPhotoApiService(api.Connection).</summary>
    public class TripPhotoApiService
    {
        private readonly ApiConnection _conn;

        public TripPhotoApiService(ApiConnection conn)
        {
            _conn = conn;
        }

        public async Task<TripCargoChecklistItem?> GetChecklistAsync(int tripId)
            => await _conn.GetAsync<TripCargoChecklistItem>($"/api/trips/{tripId}/cargo-checklist");

        public async Task<List<TripPhotoItem>> GetPhotosAsync(int tripId)
            => await _conn.GetAsync<List<TripPhotoItem>>($"/api/trips/{tripId}/photos") ?? new List<TripPhotoItem>();

        /// <summary>Tải nội dung ảnh (kèm token đăng nhập).</summary>
        public Task<byte[]> DownloadAsync(int photoId)
            => _conn.Http.GetByteArrayAsync($"/api/trips/photos/{photoId}/file");
    }
}
