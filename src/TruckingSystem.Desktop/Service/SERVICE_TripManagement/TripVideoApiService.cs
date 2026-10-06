using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_TripManagement;

namespace TruckingSystem.Desktop.SERVICE_TripManagement
{
    /// <summary>Video hành trình của chuyến (do app tài xế quay và gửi lên).</summary>
    public class TripVideoApiService
    {
        private readonly ApiConnection _conn;

        public TripVideoApiService(ApiConnection conn)
        {
            _conn = conn;
        }

        /// <summary>Danh sách đoạn video của chuyến (rỗng nếu tài xế không bật quay).</summary>
        public async Task<List<TripVideoItem>> GetSegmentsAsync(int tripId)
            => await _conn.GetAsync<List<TripVideoItem>>($"/api/trips/{tripId}/videos") ?? new List<TripVideoItem>();

        /// <summary>Đối chiếu SHA-256 — chứng minh file video chưa bị chỉnh sửa sau khi nhận.</summary>
        public async Task<TripVideoVerifyItem> VerifyAsync(int segmentId)
            => await _conn.GetAsync<TripVideoVerifyItem>($"/api/trips/videos/{segmentId}/verify")
               ?? throw new InvalidOperationException("API không trả kết quả kiểm tra.");

        /// <summary>Ghép link tương đối với địa chỉ API để thẻ &lt;video&gt; phát được.</summary>
        public string AbsoluteUrl(string relative)
            => relative.StartsWith("http", StringComparison.OrdinalIgnoreCase) ? relative : _conn.BaseUrl + relative;
    }
}
