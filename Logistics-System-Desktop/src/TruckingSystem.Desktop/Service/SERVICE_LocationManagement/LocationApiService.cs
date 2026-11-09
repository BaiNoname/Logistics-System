using System.Net;
using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_LocationManagement;

namespace TruckingSystem.Desktop.SERVICE_LocationManagement
{
    public class LocationApiService
    {
        private readonly ApiConnection _conn;

        public LocationApiService(ApiConnection conn)
        {
            _conn = conn;
        }

        public Task<List<LocationItem>> GetLocationsAsync()
            => _conn.GetAsync<List<LocationItem>>("/api/locations")!;

        public Task CreateLocationAsync(string name, string address, double? lat, double? lng)
            => _conn.PostAsync<object>("/api/locations", new { name, address, lat, lng, customerId = (int?)null });

        /// <summary>Gõ tới đâu, gợi ý tới đó — trả về tối đa vài kết quả khớp từ khóa.</summary>
        public Task<List<PlaceSuggestionItem>> SearchPlacesAsync(string text)
            => _conn.GetAsync<List<PlaceSuggestionItem>>($"/api/locations/search?text={WebUtility.UrlEncode(text)}")!;

        /// <summary>Lấy tọa độ thật của 1 gợi ý đã chọn.</summary>
        public Task<PlaceDetailItem> GetPlaceDetailAsync(string refId)
            => _conn.GetAsync<PlaceDetailItem>($"/api/locations/place/{WebUtility.UrlEncode(refId)}")!;
    }
}
