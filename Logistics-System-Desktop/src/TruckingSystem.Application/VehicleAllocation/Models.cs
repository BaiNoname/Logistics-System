namespace TruckingSystem.Application.VehicleAllocation;

public record AllocationResultItem(int TripId, string VehiclePlate, decimal QtyAssigned);
/// <summary>NeedsLateConfirmation = chưa chia vì KHÔNG KỊP HẠN GIAO — Desktop hỏi người dùng,
/// đồng ý thì gọi lại với acceptLate=true (hệ thống ghi nhật ký ai đã xác nhận chia trễ hạn).</summary>
public record AllocateVehiclesResponse(bool Success, decimal TotalQtyToShip, decimal TotalQtyAssigned, List<AllocationResultItem> Trips, string? Message, int EstimatedTripDays = 1,
    bool NeedsLateConfirmation = false,
    List<TruckingSystem.Application.Common.FixHint>? Fixes = null); // cách sửa nhanh khi chưa chia được (xem FixHint)