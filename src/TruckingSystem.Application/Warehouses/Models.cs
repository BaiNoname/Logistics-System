namespace TruckingSystem.Application.Warehouses;

public record SuggestBinRequest(int CargoLineId, decimal QtyToStore);

public record SuggestBinResponse(
    bool Found,
    int? StorageBinId,
    string? BinCode,
    string? ZoneName,
    decimal? RemainingAfterM3,
    string? Message
);

public record WarehouseZoneDto(int Id, string Name, string ZoneType, int BinCount);
public record StorageBinDto(int Id, string Code, decimal CapacityM3, decimal CurrentUsedM3, decimal MaxWeightKg);
public record WarehouseListItemDto(int Id, string Name, string? Address, decimal? CapacityM3);
