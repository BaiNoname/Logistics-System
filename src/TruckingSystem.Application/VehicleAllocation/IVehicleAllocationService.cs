namespace TruckingSystem.Application.VehicleAllocation;

public interface IVehicleAllocationService
{
    /// <summary>Chạy thuật toán Greedy chia xe cho 1 đơn vận chuyển, tự động tạo Trip + TripLine tương ứng.</summary>
    /// <param name="acceptLate">Người dùng ĐÃ xác nhận vẫn chia dù ước tính không kịp hạn giao.</param>
    /// <param name="userId">Người thao tác (ghi nhật ký khi chia trễ hạn).</param>
    /// <summary>Ước tính số ngày chạy của 1 tuyến (Lấy hàng + Giao, 8 giờ lái/ngày) — để form
    /// tạo đơn GỢI Ý hạn giao. Cùng cách tính với Chia xe.</summary>
    Task<int> EstimateRouteDaysAsync(int? pickupLocationId, int? dropoffLocationId, int warehouseId, string pickupMethod);

    /// <param name="plannedStartDate">Ngày xuất phát dự kiến (lịch VN) — null = hôm nay. Giấy tờ xe,
    /// bằng lái, giấy khám sức khỏe, ngày làm tài xế phải phủ đủ [ngày này .. ngày cuối chuyến].</param>
    Task<AllocateVehiclesResponse> AllocateAsync(int transportOrderId, bool acceptLate = false, string? userId = null, string? userName = null,
        DateTime? plannedStartDate = null);
}
