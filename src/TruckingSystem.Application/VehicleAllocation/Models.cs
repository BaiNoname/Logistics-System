namespace TruckingSystem.Application.VehicleAllocation;

public record AllocationResultItem(int TripId, string VehiclePlate, decimal QtyAssigned);
public record AllocateVehiclesResponse(bool Success, decimal TotalQtyToShip, decimal TotalQtyAssigned, List<AllocationResultItem> Trips, string? Message, int EstimatedTripDays = 1);