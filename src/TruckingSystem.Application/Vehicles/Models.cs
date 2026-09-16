namespace TruckingSystem.Application.Vehicles;

public record CreateVehicleRequest(string PlateNumber, string VehicleType, decimal CapacityQty, decimal CapacityKg, decimal CapacityM3);
public record VehicleListItemDto(int Id, string PlateNumber, string VehicleType, decimal CapacityQty, string Status);
