using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_TransportOrderManagement;

namespace TruckingSystem.Desktop.SERVICE_TransportOrderManagement
{
    public class TransportOrderApiService
    {
        private readonly ApiConnection _conn;

        public TransportOrderApiService(ApiConnection conn)
        {
            _conn = conn;
        }

        /// <summary>Đổi / bổ sung hạn giao cho đơn Đã xác nhận / Đang chạy (bắt buộc lý do).</summary>
        public Task<OrderActionResultItem> ChangeDeadlineAsync(int orderId, DateTime newDate, string reason, string requestedBy)
            => _conn.PostAsync<OrderActionResultItem>($"/api/transport-orders/{orderId}/deadline",
                new { newDeliveryDate = newDate.Date, reason, requestedBy })!;

        /// <summary>Phí lưu kho của đơn (từng ngày, lịch sử đổi hạn + hạn tính trễ, tổng).</summary>
        public Task<StorageFeeItem> GetStorageFeeAsync(int orderId)
            => _conn.GetAsync<StorageFeeItem>($"/api/transport-orders/{orderId}/storage-fee")!;

        /// <summary>Hàng nằm kho sau hạn tính trễ: party = "Customer" (tính phí, bắt buộc lý do) / "Company" (0 đ).</summary>
        public Task<OrderActionResultItem> SetStorageResponsibilityAsync(int orderId, string party, string? reason)
            => _conn.PostAsync<OrderActionResultItem>($"/api/transport-orders/{orderId}/storage-responsibility", new { party, reason })!;

        /// <summary>Ước tính số ngày chạy của tuyến (Lấy hàng + Giao) — để gợi ý hạn giao khi tạo đơn.</summary>
        public async Task<int> EstimateRouteDaysAsync(int? pickupLocationId, int? dropoffLocationId, int warehouseId, string pickupMethod)
        {
            var result = await _conn.GetAsync<RouteDaysEstimate>(
                $"/api/transport-orders/estimate-days?warehouseId={warehouseId}&pickupMethod={pickupMethod}" +
                (pickupLocationId is int p ? $"&pickupLocationId={p}" : "") +
                (dropoffLocationId is int d ? $"&dropoffLocationId={d}" : ""));
            return Math.Max(1, result?.Days ?? 1);
        }

        private class RouteDaysEstimate
        {
            public int Days { get; set; }
        }

        public Task<List<TransportOrderListItem>> GetTransportOrdersAsync()
            => _conn.GetAsync<List<TransportOrderListItem>>("/api/transport-orders")!;

        public Task CreateTransportOrderAsync(int customerId, string pickup, string dropoff, int warehouseId, DateTime? committedDate,
            List<CargoLineItem> cargoLines, int? pickupLocationId = null, int? dropoffLocationId = null, string pickupMethod = "CompanyPickup",
            int? contractId = null)
            => _conn.PostAsync<object>("/api/transport-orders", new
            {
                customerId,
                pickupPoint = pickup,
                dropoffPoint = dropoff,
                warehouseId,
                committedDeliveryDate = committedDate,
                cargoLines,
                pickupLocationId,
                dropoffLocationId,
                pickupMethod,
                contractId,
            });

        /// <summary>Sửa đơn — CHỈ áp dụng được khi đơn còn Draft (Backend tự kiểm tra lại).</summary>
        public Task<OrderActionResultItem> UpdateTransportOrderAsync(int id, string pickup, string dropoff, int warehouseId, DateTime? committedDate,
            List<CargoLineItem> cargoLines, int? pickupLocationId, int? dropoffLocationId, string pickupMethod, int? contractId = null)
            => _conn.PutAsync<OrderActionResultItem>($"/api/transport-orders/{id}", new
            {
                pickupPoint = pickup,
                dropoffPoint = dropoff,
                warehouseId,
                committedDeliveryDate = committedDate,
                cargoLines,
                pickupLocationId,
                dropoffLocationId,
                pickupMethod,
                contractId,
            })!;

        /// <summary>Hợp đồng đang hiệu lực của khách — dropdown "Hợp đồng" khi tạo / sửa đơn.</summary>
        public Task<List<OrderContractOption>> GetContractOptionsAsync(int customerId)
            => _conn.GetAsync<List<OrderContractOption>>($"/api/transport-orders/contract-options?customerId={customerId}")!;

        public Task<TransportOrderDetailItem> GetOrderDetailAsync(int id)
            => _conn.GetAsync<TransportOrderDetailItem>($"/api/transport-orders/{id}")!;

        /// <param name="acceptLate">Đã xác nhận vẫn chia dù không kịp hạn giao.</param>
        /// <param name="plannedStart">Ngày xuất phát dự kiến (null = hôm nay).</param>
        public Task<AllocateVehiclesResult> AllocateVehiclesAsync(int transportOrderId, bool acceptLate = false, DateTime? plannedStart = null)
            => _conn.PostAsync<AllocateVehiclesResult>(
                $"/api/transport-orders/{transportOrderId}/allocate-vehicles?acceptLate={(acceptLate ? "true" : "false")}" +
                (plannedStart is DateTime d ? $"&plannedStart={d:yyyy-MM-dd}" : ""), new { })!;

        /// <summary>Draft -> Confirmed.</summary>
        public Task<OrderActionResultItem> ConfirmOrderAsync(int id)
            => _conn.PostAsync<OrderActionResultItem>($"/api/transport-orders/{id}/confirm", new { })!;

        /// <summary>Xem trước khi hủy (không đổi dữ liệu): sẽ xảy ra gì / còn việc gì phải làm trước.</summary>
        public async Task<CancelPreviewItem> GetCancelPreviewAsync(int id)
            => await _conn.GetAsync<CancelPreviewItem>($"/api/transport-orders/{id}/cancel-preview")
               ?? throw new InvalidOperationException("Máy chủ không trả kết quả xem trước.");

        /// <summary>-> Cancelled.</summary>
        public Task<OrderActionResultItem> CancelOrderAsync(int id)
            => _conn.PostAsync<OrderActionResultItem>($"/api/transport-orders/{id}/cancel", new { })!;
    }
}
