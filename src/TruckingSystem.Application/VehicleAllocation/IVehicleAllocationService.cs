namespace TruckingSystem.Application.VehicleAllocation;

public interface IVehicleAllocationService
{
    /// <summary>Chạy thuật toán Greedy chia xe cho 1 đơn vận chuyển, tự động tạo Trip + TripLine tương ứng.</summary>
    Task<AllocateVehiclesResponse> AllocateAsync(int transportOrderId);
}
